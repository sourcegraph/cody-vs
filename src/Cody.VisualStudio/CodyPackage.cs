using Cody.Core.Agent;
using Cody.Core.Agent.Protocol;
using Cody.Core.Common;
using Cody.Core.DocumentSync;
using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.Core.Trace;
using Cody.UI.Controls;
using Cody.UI.ViewModels;
using Cody.UI.Views;
using Cody.VisualStudio.Client;
using Cody.VisualStudio.Inf;
using Cody.VisualStudio.Options;
using Cody.VisualStudio.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Connected.CredentialStorage;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Sentry;
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
using Configuration = Cody.Core.Common.Configuration;
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

        public static IAgentService AgentService;
        public static IUserSettingsService UserSettingsService;
        public IVersionService VersionService;
        public IVsVersionService VsVersionService;
        public IStatusbarService StatusbarService;
        public IThemeService ThemeService;
        public ISolutionService SolutionService;
        public IWebViewsManager WebViewsManager;
        public IProgressService ProgressService;
        public IAgentProxy AgentClient;
        public ISecretStorageService SecretStorageService;
        public IConfigurationService ConfigurationService;
        public IFileDialogService FileDialogService;

        private IInfobarNotifications InfobarNotifications;

        private readonly TaskCompletionSource<IInfobarNotifications> _infobarNotificationsCompletionSource = new TaskCompletionSource<IInfobarNotifications>();
        public Task<IInfobarNotifications> InfobarNotificationsAsync => _infobarNotificationsCompletionSource.Task;

        public GeneralOptionsViewModel GeneralOptionsViewModel;
        public MainViewModel MainViewModel;

        public MainView MainView;
        public NotificationHandlers NotificationHandlers;
        public ProgressNotificationHandlers ProgressNotificationHandlers;
        public TextDocumentNotificationHandlers TextDocumentNotificationHandlers;
        public DocumentsSyncService DocumentsSyncService;
        public static TestingSupportService TestingSupportService;
        public IDocumentService DocumentService;
        public IVsUIShell VsUIShell;
        public OleMenuCommandService OleMenuService;
        public IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            try
            {
                InitializeErrorHandling();

                var loggerFactory = InitializeMainLogger();
                LoadDevConfiguration();

                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                InitializeTrace();
                InitializeServices(loggerFactory);
                InitOleMenu();

                InitializeAgent();

                ReportSentryVsVersion();
            }
            catch (Exception ex)
            {
                Logger?.Error("Cody Package initialization failed.", ex);
            }
        }

        private LoggerFactory InitializeMainLogger()
        {
            var loggerFactory = new LoggerFactory();
            Logger = loggerFactory.Create(WindowPaneLogger.DefaultCody);

            return loggerFactory;
        }

        private void LoadDevConfiguration()
        {
            try
            {
                Configuration.Initialize(Logger);
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (location != null)
                {
                    var devConfig = Path.Combine(location, "CodyDevConfig.json");
                    Configuration.AddFromJsonFile(devConfig);
                }

                Configuration.AddFromEnviromentVariableJsonFile("CODY_VS_DEV_CONFIG");
            }
            catch (Exception ex)
            {
                Logger.Error("Loading DEV config failed.", ex);
            }
        }

        private void InitializeServices(LoggerFactory loggerFactory)
        {
            AgentLogger = loggerFactory.Create(Configuration.ShowCodyAgentOutput ? WindowPaneLogger.CodyAgent : null);
            AgentNotificationsLogger = loggerFactory.Create(Configuration.ShowCodyNotificationsOutput ? WindowPaneLogger.CodyNotifications : null);


            var componentModel = this.GetService<SComponentModel, IComponentModel>();
            var vsSolution = this.GetService<SVsSolution, IVsSolution>();
            SolutionService = new SolutionService(vsSolution, Logger);
            VersionService = loggerFactory.GetVersionService();
            VsVersionService = new VsVersionService(Logger);

            var vsSecretStorage = this.GetService<SVsCredentialStorageService, IVsCredentialStorageService>();
            SecretStorageService = new SecretStorageService(vsSecretStorage, Logger);
            SecretStorageService.AccessTokenRefreshed += AccessTokenRefreshed;
            UserSettingsService = new UserSettingsService(new UserSettingsProvider(this), SecretStorageService, Logger);
            OleMenuService = this.GetService<IMenuCommandService, OleMenuCommandService>();

            ConfigurationService = new ConfigurationService(VersionService, VsVersionService, SolutionService, UserSettingsService, Logger);

            StatusbarService = new StatusbarService();
            ThemeService = new ThemeService(this, Logger);


            var statusCenterService = this.GetService<SVsTaskStatusCenterService, IVsTaskStatusCenterService>();
            ProgressService = new ProgressService(statusCenterService);
            TestingSupportService = null; // new TestingSupportService(statusCenterService);
            VsEditorAdaptersFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            DocumentService = new DocumentService(Logger, this, vsSolution, VsEditorAdaptersFactoryService);

            NotificationHandlers = new NotificationHandlers(UserSettingsService, AgentNotificationsLogger, DocumentService, SecretStorageService, InfobarNotificationsAsync);

            NotificationHandlers.OnOptionsPageShowRequest += HandleOnOptionsPageShowRequest;
            NotificationHandlers.OnFocusSidebarRequest += HandleOnFocusSidebarRequest;
            NotificationHandlers.AuthorizationDetailsChanged += AuthorizationDetailsChanged;

            var sidebarController = WebView2Dev.InitializeController(ThemeService.GetThemingScript(), Logger);
            ThemeService.ThemeChanged += sidebarController.OnThemeChanged;
            NotificationHandlers.PostWebMessageAsJson = WebView2Dev.PostWebMessageAsJson;

            var runningDocumentTable = this.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();


            VsUIShell = this.GetService<SVsUIShell, IVsUIShell>();
            FileDialogService = new FileDialogService(SolutionService, Logger);

            ProgressNotificationHandlers = new ProgressNotificationHandlers(ProgressService);
            TextDocumentNotificationHandlers = new TextDocumentNotificationHandlers(DocumentService, FileDialogService, StatusbarService, Logger);




            Logger.Info($"Visual Studio version: {VsVersionService.DisplayVersion} ({VsVersionService.EditionName})");
        }

        private void ReportSentryVsVersion()
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("vs", VsVersionService.DisplayVersion);
                scope.SetTag("agent", VersionService.Agent);
            });

            VsShellUtilities.ShutdownToken.Register(() => SentrySdk.ConfigureScope(s => s.SetTag("isShuttingDown", "true")));
        }

        private static void InitializeTrace()
        {
            if (Configuration.Trace)
            {
                if (!string.IsNullOrEmpty(Configuration.TraceFile))
                    TraceManager.Listeners.Add(new FileTraceListener(Configuration.TraceFile));

                TraceManager.Enabled = true;
            }
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

        private async void AuthorizationDetailsChanged(object sender, ProtocolAuthStatus status)
        {
            try
            {
                Logger.Debug($"Checking authorization status ...");
                EnableContextMenu(status.Authenticated);
                if (ConfigurationService == null || AgentService == null)
                {
                    Logger.Debug("Not changed.");
                    return;
                }

                UpdateCurrentWorkspaceFolder();

                if (!status.Authenticated && UserSettingsService.LastTimeAuthorized)
                    await ShowToolWindowWhenLoggedOut();

                UserSettingsService.LastTimeAuthorized = status.Authenticated;

                Logger.Debug($"Auth status: Authenticated: {status.Authenticated}");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed.", ex);
            }
        }

        private async void AccessTokenRefreshed(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug($"Checking authorization status ...");
                if (ConfigurationService == null || AgentService == null)
                {
                    Logger.Debug("Not changed.");
                    return;
                }

                var config = ConfigurationService.GetConfiguration();
                var status = await AgentService.ConfigurationChange(config);

                Logger.Debug($"After access token refresh. Authenticated: {status.Authenticated}");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed.", ex);
            }
        }

        private async Task ShowToolWindowWhenLoggedOut()
        {
            Logger.Debug($"Logout detected. Showing tool window ...");
            await ShowToolWindowAsync();
        }

        private void InitializeNotificationsBar()
        {
            try
            {
                if (InfobarNotifications != null) return;

                var vsShell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
                if (vsShell != null)
                {
                    vsShell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var obj);

                    var host = obj as IVsInfoBarHost;
                    if (host != null)
        {
                        //Logger.Debug($"InitializeInfoBarService:{host}");

                        var uiFactory = ServiceProvider.GlobalProvider.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
                        if (uiFactory != null)
        {
                            InfobarNotifications = new InfobarNotifications(host, uiFactory, AgentNotificationsLogger);
                            _infobarNotificationsCompletionSource.SetResult(InfobarNotifications);

                        }
                        else
                    {
                            Logger.Error("Cannot get IVsInfoBarUIFactory");
                        }

                    }
                    else
                        Logger.Error("Cannot get IVsInfoBarHost");
                }
                else
                    Logger.Error("Cannot get IVsShell");
            }
            catch (Exception ex)
            {
                Logger.Error("Cannot initialize InfoBarLicensePreordersService.", ex);
            }
        }

        private void PrepareAgentConfiguration()
        {
            try
            {
                var agentDir = Configuration.AgentDirectory ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

                if (Configuration.RemoteAgentPort.HasValue)
                    Logger.Debug($"Connecting to the agent using port:{Configuration.RemoteAgentPort}");

                var options = new AgentClientOptions
                {
                    CallbackHandlers = new List<object> { NotificationHandlers, ProgressNotificationHandlers, TextDocumentNotificationHandlers },
                    AgentDirectory = agentDir,
                    RestartAgentOnFailure = true,
                    ConnectToRemoteAgent = Configuration.RemoteAgentPort.HasValue,
                    RemoteAgentPort = Configuration.RemoteAgentPort ?? 0,
                    AcceptNonTrustedCertificates = UserSettingsService.AcceptNonTrustedCert,
                    Debug = Configuration.AllowNodeDebug
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

        private async void OnAgentInitialized(object sender, ServerInfo e)
        {
            try
            {
                if (e.Authenticated == true && e.AuthStatus is ProtocolAuthenticatedAuthStatus status)
                {
                    StatusbarService.SetText($"Hello {status.DisplayName}! Press Alt + L to open Cody Chat.");
                    Logger.Info("Authenticated.");
                    UserSettingsService.LastTimeAuthorized = true;
                    SentrySdk.ConfigureScope(scope =>
                    {
                        scope.User = new SentryUser
                        {
                            Email = status.PrimaryEmail,
                            Username = status.Username,
                        };
                    });
                }
                else
                {
                    Logger.Warn("Authentication failed. Please check the validity of the access token.");
                    if (e.AuthStatus is ProtocolUnauthenticatedAuthStatus unauth && unauth.Error != null)
                    {
                        Logger.Warn(unauth.Error.Message);
                    }

                    if (UserSettingsService.LastTimeAuthorized)
                    {
                        // show tool window only once, when user was logged out between IDE restarts
                        await ShowToolWindowWhenLoggedOut();
                        UserSettingsService.LastTimeAuthorized = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed.", ex);
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
                        await TestServerConnection(UserSettingsService.DefaultServerEndpoint);
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
            InitializeNotificationsBar();
        }

        private void UpdateCurrentWorkspaceFolder()
        {
            try
            {
                var solutionUri = SolutionService.GetSolutionDirectory().ToUri();
                var workspaceFolderEvent = new WorkspaceFolderDidChangeEvent
                {
                    Uris = new List<string> { solutionUri }
                };
                AgentService.WorkspaceFolderDidChange(workspaceFolderEvent);
                Logger.Debug($"Workspace updated:{solutionUri}");

                if (DocumentsSyncService == null)
                {
                    var documentSyncCallback = new DocumentSyncCallback(AgentService, Logger);
                    DocumentsSyncService = new DocumentsSyncService(VsUIShell, documentSyncCallback, VsEditorAdaptersFactoryService, UserSettingsService, Logger);
                DocumentsSyncService.Initialize();
            }
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
                DocumentsSyncService = null;
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
