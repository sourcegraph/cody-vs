using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Cody.Core.Ide;
using Cody.Core.Inf;
using Cody.Core.Logging;
using Cody.UI.Views;
using Cody.UI.Controls;
using Cody.VisualStudio.Inf;
using Cody.VisualStudio.Services;
using Task = System.Threading.Tasks.Task;
using System.Reflection;
using System.IO;
using Cody.Core.Settings;
using Cody.Core.Infrastructure;
using Cody.Core.Agent.Connector;
using Cody.Core.Agent;
using Cody.Core.DocumentSync;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CodyPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsPage), "Cody", "General", 0, 0, true)]
    [ProvideToolWindow(typeof(CodyToolWindow), Style = VsDockStyle.Tabbed, Window = VsConstants.VsWindowKindSolutionExplorer)]
    public sealed class CodyPackage : AsyncPackage
    {

        public const string PackageGuidString = "9b8925e1-803e-43d9-8f43-c4a4f35b4325";

        public ILog Logger;
        public IVersionService VersionService;
        public IVsVersionService VsVersionService;
        public MainView MainView;
        public AgentConnector AgentConnector;
        public IUserSettingsService UserSettingsService;
        public InitializeCallback InitializeService;
        public IStatusbarService StatusbarService;
        public IThemeService ThemeService;
        public NotificationHandlers NotificationHandlers;
        public IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService;
        public IVsUIShell VsUIShell;
        public IAgentClientFactory AgentClientFactory;
        public DocumentsSyncService DocumentsSyncService;

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
                await InitializeAgent();

            }
            catch (Exception ex)
            {
                Logger?.Error("Cody Package initialization failed.", ex);
            }
        }

        private void InitializeServices()
        {
            var loggerFactory = new LoggerFactory();
            Logger = loggerFactory.Create();
            VersionService = loggerFactory.GetVersionService();
            VsVersionService = new VsVersionService(Logger);
            UserSettingsService = new UserSettingsService(new UserSettingsProvider(this), Logger);
            StatusbarService = new StatusbarService();
            InitializeService = new InitializeCallback(UserSettingsService, VersionService, VsVersionService, StatusbarService, Logger);
            ThemeService = new ThemeService(this);

            var runningDocumentTable = this.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
            var componentModel = this.GetService<SComponentModel, IComponentModel>();
            VsEditorAdaptersFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            VsUIShell = this.GetService<SVsUIShell, IVsUIShell>();

            Logger.Info($"Visual Studio version: {VsVersionService.Version}");
        }

        private async Task InitOleMenu()
        {
            try
            {
                var oleMenuService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (oleMenuService != null)
                {
                    var commandId = new CommandID(Guids.CodyPackageCommandSet, (int)CommandIds.CodyToolWindow);
                    var menuItem = new MenuCommand(ShowToolWindow, commandId);
                    oleMenuService.AddCommand(menuItem);
                }
                else
                {
                    Logger.Error($"Cannot get {typeof(OleMenuCommandService)}");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error("Cannot initialize menu items", ex);
            }
        }

        public async void ShowToolWindow(object sender, EventArgs eventArgs)
        {
            try
            {
                Logger.Debug("Showing Tool Window ...");
                var toolWindow = await ShowToolWindowAsync(typeof(CodyToolWindow), 0, true, DisposalToken);
                if (toolWindow?.Frame == null)
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Cannot open Tool Window.", ex);
            }
        }

        private void InitializeErrorHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private async Task InitializeAgent()
        {
            try
            {
                var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

                NotificationHandlers = new NotificationHandlers();
                // Set the env var to 3113 when running with local agent.
                var portNumber = int.TryParse(Environment.GetEnvironmentVariable("CODY_VS_DEV_PORT"), out int port) ? port : (int?)null;

                var options = new AgentConnectorOptions
                {
                    NotificationsTarget = NotificationHandlers,
                    AgentDirectory = agentDir,
                    RestartAgentOnFailure = true,
                    AfterConnection = (client) => InitializeService.Initialize(client),
                    Port = portNumber,
                };

                AgentConnector = new AgentConnector(options, Logger);
                AgentClientFactory = new AgentClientFactory(AgentConnector);

                WebView2Dev.InitializeController(ThemeService.GetThemingScript());
                NotificationHandlers.PostWebMessageAsJson = WebView2Dev.PostWebMessageAsJson;

                _ = Task.Run(() => AgentConnector.Connect())
                .ContinueWith(x =>
                {
                    var documentSyncCallback = new DocumentSyncCallback(AgentClientFactory, Logger);
                    DocumentsSyncService = new DocumentsSyncService(VsUIShell, documentSyncCallback, VsEditorAdaptersFactoryService);
                    DocumentsSyncService.Initialize();
                })
                .ContinueWith(t =>
                {
                    foreach (var ex in t.Exception.Flatten().InnerExceptions)
                        Logger.Error("Agent connecting error", ex);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                Logger?.Error("Cannot initialize agent.", ex);
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error($"Unhandled domain exception:{e.ExceptionObject}");
            Logger.Error($"Unhandled domain exception, is terminating:{e.IsTerminating}");

            var exception = e.ExceptionObject as Exception;
            Logger.Error("Unhandled domain exception occurred.", exception);

        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            Logger.Error("Unhandled exception occurred on the UI thread.", exception);

            if (!System.Diagnostics.Debugger.IsAttached)
                e.Handled = true;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                if (!System.Diagnostics.Debugger.IsAttached)
                    e.SetObserved();

                var thirdPartyException = e.Exception;
                var exceptionDetails = new StringBuilder();
                if (thirdPartyException != null)
                {
                    exceptionDetails.AppendLine(thirdPartyException.Message);
                    exceptionDetails.AppendLine(thirdPartyException.StackTrace);

                    if (thirdPartyException.InnerExceptions.Any())
                    {
                        foreach (var inner in thirdPartyException.InnerExceptions)
                        {
                            exceptionDetails.AppendLine(inner.Message);
                            exceptionDetails.AppendLine(inner.StackTrace);
                        }
                    }

                    if (!exceptionDetails.ToString().Contains("Cody")) return;
                }

                Logger.Error("Unhandled exception occurred on the non-UI thread.", e.Exception);
                foreach (var ex in e.Exception.InnerExceptions)
                {
                    Logger.Error("Inner exception", ex);
                }
            }
            catch
            {
                // catching everything because if not VS will freeze/crash on the exception
            }
        }
    }
}
