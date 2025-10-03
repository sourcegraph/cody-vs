using Cody.UI.ViewModels;
using Cody.UI.Views;
using Cody.VisualStudio.Inf;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

#pragma warning disable VSTHRD010

namespace Cody.VisualStudio
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid(Guids.CodyChatToolWindowString)]
    public class CodyToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodyToolWindow"/> class.
        /// </summary>
        public CodyToolWindow() : base(null)
        {
            this.Caption = "Cody";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.

            var package = GetPackage();
            var logger = package.Logger;
            var notificationsHandlers = package.WebviewNotificationHandlers;
            var webViewsManager = package.WebViewsManager;

            var viewModel = new MainViewModel(webViewsManager, notificationsHandlers, logger);
            var view = new MainView
            {
                DataContext = viewModel
            };

            package.MainViewModel = viewModel;
            package.MainView = view;
            base.Content = view;

        }

        private CodyPackage GetPackage()
        {
            var vsShell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
            IVsPackage package;
            var guidPackage = new Guid(CodyPackage.PackageGuidString);
            if (vsShell.IsPackageLoaded(ref guidPackage, out package) == Microsoft.VisualStudio.VSConstants.S_OK)
            {
                var currentPackage = (CodyPackage)package;
                return currentPackage;
            }

            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.Create();
            logger.Error("Couldn't get an instance of the CodyPackage.");

            return null;
        }
    }
}
