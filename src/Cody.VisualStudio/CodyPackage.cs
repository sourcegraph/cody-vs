using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.DocumentSync;
using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.Core.Workspace;
using Cody.UI.Controls;
using Cody.UI.ViewModels;
using Cody.UI.Views;
using Cody.VisualStudio.Client;
using Cody.VisualStudio.Inf;
using Cody.VisualStudio.Options;
using Cody.VisualStudio.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Connected.CredentialStorage;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;
using Task = System.Threading.Tasks.Task;

namespace Cody.VisualStudio
{

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(GeneralOptionsPage), "Cody", "General", 0, 0, true)]
    [ProvideToolWindow(typeof(CodyToolWindow), Style = VsDockStyle.Tabbed, Window = VsConstants.VsWindowKindSolutionExplorer)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed partial class CodyPackage : AsyncPackage
    {

        public const string PackageGuidString = "9b8925e1-803e-43d9-8f43-c4a4f35b4325";

        public ILog Logger;
        public ILog AgentLogger;
        public ILog AgentNotificationsLogger;

        public IVersionService VersionService;
        public IVsVersionService VsVersionService;
        public IAgentService AgentService;
        public IUserSettingsService UserSettingsService;
        public IStatusbarService StatusbarService;
        public IThemeService ThemeService;
        public ISolutionService SolutionService;
        public IWebViewsManager WebViewsManager;
        public IProgressService ProgressService;
        public IAgentProxy AgentClient;
        public ISecretStorageService SecretStorageService;
        public IConfigurationService ConfigurationService;

        public GeneralOptionsViewModel GeneralOptionsViewModel;
        public MainViewModel MainViewModel;

        public MainView MainView;
        public NotificationHandlers NotificationHandlers;
        public ProgressNotificationHandlers ProgressNotificationHandlers;
        public DocumentsSyncService DocumentsSyncService;
        public IFileService FileService;
        public IVsUIShell VsUIShell;
        public IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {

            try
            {
                InitializeErrorHandling();

                // When initialized asynchronously, the current thread may be a background thread at this point.
                // Do any initialization that requires the UI thread after switching to the UI thread.
                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                InitializeServices();
                await InitOleMenu();

                InitializeAgent();

            }
            catch (Exception ex)
            {
                Logger?.Error("Cody Package initialization failed.", ex);
            }
        }

        private void InitializeServices()
        {
            //TraceManager.Listeners.Add(new FileTraceListener(@"c:\tmp\cody.log"));
            //TraceManager.Enabled = true;

            var loggerFactory = new LoggerFactory();
            AgentLogger = loggerFactory.Create(WindowPaneLogger.CodyAgent);
            AgentNotificationsLogger = loggerFactory.Create(WindowPaneLogger.CodyNotifications);
            Logger = loggerFactory.Create();

            var vsSolution = this.GetService<SVsSolution, IVsSolution>();
            SolutionService = new SolutionService(vsSolution, Logger);
            VersionService = loggerFactory.GetVersionService();
            VsVersionService = new VsVersionService(Logger);

            var vsSecretStorage = this.GetService<SVsCredentialStorageService, IVsCredentialStorageService>();
            SecretStorageService = new SecretStorageService(vsSecretStorage);
            UserSettingsService = new UserSettingsService(new UserSettingsProvider(this), SecretStorageService, Logger);
            UserSettingsService.AuthorizationDetailsChanged += AuthorizationDetailsChanged;

            ConfigurationService = new ConfigurationService(VersionService, VsVersionService, SolutionService, UserSettingsService, Logger);

            StatusbarService = new StatusbarService();
            ThemeService = new ThemeService(this, Logger);
            FileService = new FileService(this, Logger);
            var statusCenterService = this.GetService<SVsTaskStatusCenterService, IVsTaskStatusCenterService>();
            ProgressService = new ProgressService(statusCenterService);
            NotificationHandlers = new NotificationHandlers(UserSettingsService, AgentNotificationsLogger, FileService, SecretStorageService);
            NotificationHandlers.OnOptionsPageShowRequest += HandleOnOptionsPageShowRequest;
            NotificationHandlers.OnFocusSidebarRequest += HandleOnFocusSidebarRequest;


            ProgressNotificationHandlers = new ProgressNotificationHandlers(ProgressService);

            var sidebarController = WebView2Dev.InitializeController(ThemeService.GetThemingScript(), Logger);
            ThemeService.ThemeChanged += sidebarController.OnThemeChanged;
            NotificationHandlers.PostWebMessageAsJson = WebView2Dev.PostWebMessageAsJson;

            var runningDocumentTable = this.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
            var componentModel = this.GetService<SComponentModel, IComponentModel>();
            VsEditorAdaptersFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            VsUIShell = this.GetService<SVsUIShell, IVsUIShell>();

            Logger.Info($"Visual Studio version: {VsVersionService.DisplayVersion} ({VsVersionService.EditionName})");
        }

        private void HandleOnOptionsPageShowRequest(object sender, EventArgs e)
        {
            try
            {
                ShowOptionPage(typeof(GeneralOptionsPage));
            }
            catch (Exception ex)
            {
                Logger.Error($"Navigation to '{nameof(GeneralOptionsPage)}' failed.", ex);
            }
        }

        private async void HandleOnFocusSidebarRequest(object sender, EventArgs e)
        {
            try
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                Application.Current.MainWindow.Activate();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed.", ex);
            }
        }

        private async void AuthorizationDetailsChanged(object sender, EventArgs eventArgs)
        {
            try
            {
                Logger.Debug($"Changing authorization details ...");

                var config = ConfigurationService.GetConfiguration();
                var status = await AgentService.ConfigurationChange(config);

                UpdateCurrentWorkspaceFolder();

                Logger.Debug($"Auth status: Authenticated: {status.Authenticated}");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed.", ex);
            }
        }

        private async Task InitOleMenu()
        {
            try
            {
                if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService oleMenuService)
                {
                    var commandId = new CommandID(Guids.CodyPackageCommandSet, (int)CommandIds.CodyToolWindow);
                    oleMenuService.AddCommand(new MenuCommand(ShowToolWindow, commandId));
                }
                else
                {
                    throw new NotSupportedException($"Cannot get {nameof(OleMenuCommandService)}");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error("Cannot initialize menu items", ex);
            }
        }

        public async void ShowToolWindow(object sender, EventArgs eventArgs)
        {
            await ShowToolWindowAsync();
        }

        public async Task ShowToolWindowAsync()
        {
            try
            {
                Logger.Debug("Toggling Tool Window ...");
                var window = await ShowToolWindowAsync(typeof(CodyToolWindow), 0, true, DisposalToken);
                if (window?.Frame is IVsWindowFrame windowFrame)
                {
                    bool isVisible = windowFrame.IsVisible() == 0;
                    bool isOnScreen = windowFrame.IsOnScreen(out int screenTmp) == 0 && screenTmp == 1;

                    Logger.Debug($"IsVisible:{isVisible} IsOnScreen:{isOnScreen}");

                    if (!isVisible || !isOnScreen)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.Show());
                        Logger.Debug("Shown.");

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Cannot toggle Tool Window.", ex);
            }
        }

        private void PrepareAgentConfiguration()
        {
            try
            {
                var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

                var devPort = Environment.GetEnvironmentVariable("CODY_VS_DEV_PORT", EnvironmentVariableTarget.User);
                var portNumber = int.TryParse(devPort, out int port) ? port : 3113;

                if (devPort != null)
                    Logger.Debug($"Connecting to the agent using port:{devPort}");

                var options = new AgentClientOptions
                {
                    CallbackHandlers = new List<object> { NotificationHandlers, ProgressNotificationHandlers },
                    AgentDirectory = agentDir,
                    RestartAgentOnFailure = true,
                    ConnectToRemoteAgent = devPort != null,
                    RemoteAgentPort = portNumber,
                    AcceptNonTrustedCertificates = UserSettingsService.AcceptNonTrustedCert,
                    Debug = true
                };

                AgentClient = new AgentClient(options, Logger, AgentLogger);
                AgentClient.OnInitialized += OnAgentInitialized;

                WebViewsManager = new WebViewsManager(AgentClient, NotificationHandlers, Logger);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed.", ex);
            }
        }

        private void OnAgentInitialized(object sender, ServerInfo e)
        {
            if (e.Authenticated == true)
            {
                StatusbarService.SetText($"Hello {e.AuthStatus.DisplayName}! Press Alt + L to open Cody Chat.");

                Logger.Info("Authenticated.");
            }
            else
            {
                Logger.Warn("Authentication failed. Please check the validity of the access token.");
            }
        }

        private void InitializeAgent()
        {
            try
            {
                PrepareAgentConfiguration();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await TestServerConnection(UserSettingsService.ServerEndpoint);
                        AgentClient.Start();

                        var clientConfig = ConfigurationService.GetClientInfo();
                        AgentService = await AgentClient.Initialize(clientConfig);

                        WebViewsManager.SetAgentService(AgentService);
                        NotificationHandlers.SetAgentClient(AgentService);
                        ProgressNotificationHandlers.SetAgentService(AgentService);

                        if (SolutionService.IsSolutionOpen()) OnAfterBackgroundSolutionLoadComplete();
                        SolutionEvents.OnAfterBackgroundSolutionLoadComplete += OnAfterBackgroundSolutionLoadComplete;
                        SolutionEvents.OnAfterCloseSolution += OnAfterCloseSolution;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Agent initialization failed.", ex);
                    }

                });
            }
            catch (Exception ex)
            {
                Logger?.Error("Cannot initialize agent.", ex);
            }
        }

        private void OnAfterBackgroundSolutionLoadComplete(object sender = null, EventArgs e = null)
        {
            UpdateCurrentWorkspaceFolder();
        }

        private void UpdateCurrentWorkspaceFolder()
        {
            try
            {
                var solutionUri = new Uri(SolutionService.GetSolutionDirectory()).AbsoluteUri;
                var workspaceFolderEvent = new WorkspaceFolderDidChangeEvent
                {
                    Uris = new List<string> { solutionUri }
                };
                AgentService.WorkspaceFolderDidChange(workspaceFolderEvent);
                Logger.Debug($"Workspace updated:{solutionUri}");

                if (DocumentsSyncService == null)
                {
                    var documentSyncCallback = new DocumentSyncCallback(AgentService, Logger);
                    DocumentsSyncService = new DocumentsSyncService(VsUIShell, documentSyncCallback, VsEditorAdaptersFactoryService, Logger);
                }
                DocumentsSyncService.Initialize();
            }
            catch (Exception ex)
            {
                Logger?.Error("After open solution error.", ex);
            }
        }

        private void OnAfterCloseSolution(object sender, EventArgs e)
        {
            try
            {
                DocumentsSyncService?.Deinitialize();
                var workspaceFolderEvent = new WorkspaceFolderDidChangeEvent
                {
                    Uris = new List<string>()
                };
                AgentService.WorkspaceFolderDidChange(workspaceFolderEvent);

            }
            catch (Exception ex)
            {
                Logger?.Error("After close solution error.", ex);
            }
        }

        private async Task TestServerConnection(string serverAddress)
        {
            if (string.IsNullOrWhiteSpace(serverAddress)) return;

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    await webClient.DownloadStringTaskAsync(serverAddress);
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.TrustFailure)
                {
                    if (!UserSettingsService.AcceptNonTrustedCert)
                    {
                        AgentLogger.Warn("SSL certificate problem: self-signed certificate in servers certificate chain. " +
                            "Consider using 'Accept non-trusted certificates' option in Cody settings window.");
                    }
                    else
                    {
                        AgentLogger.Warn("SSL certificate problem.");
                    }
                }
                catch
                {
                    AgentLogger.Warn($"Connection problem with '{serverAddress}'");
                }
            }
        }

    }
}
