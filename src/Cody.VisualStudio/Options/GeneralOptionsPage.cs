using System;
using System.ComponentModel;
using System.Windows;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.UI.Controls.Options;
using Cody.UI.ViewModels;
using Cody.VisualStudio.Inf;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Options
{
    public class GeneralOptionsPage : UIElementDialogPage
    {

        private GeneralOptionsControl _control;
        private IUserSettingsService _settingsService;

        private CodyPackage _codyPackage;
        private GeneralOptionsViewModel _generalOptionsViewModel;
        private ILog _logger;

        public GeneralOptionsPage()
        {
            _codyPackage = GetPackage();
            if (_codyPackage != null)
            {
                _logger = _codyPackage.Logger;
                _settingsService = _codyPackage.UserSettingsService;

                _logger.Debug("Initialized.");
            }
        }

        private CodyPackage GetPackage()
        {
            // copy&paste from CodyToolWindow 

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
            logger.Error("Couldn't get logger instance from the CodyPackage.");

            return null;
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            _logger.Debug($"Settings page activated.");


            var accessToken = _settingsService.AccessToken;
            var sourcegraphUrl = _settingsService.ServerEndpoint;

            _generalOptionsViewModel.AccessToken = accessToken;
            _generalOptionsViewModel.SourcegraphUrl = sourcegraphUrl;

            _logger.Debug($"Is canceled:{e.Cancel}");

            base.OnActivate(e);
        }
        protected override void OnApply(PageApplyEventArgs args)
        {
            _logger.Debug($"{args.ApplyBehavior}");

            var accessToken = _generalOptionsViewModel.AccessToken;
            var sourcegraphUrl = _generalOptionsViewModel.SourcegraphUrl;

            _settingsService.AccessToken = accessToken;
            _settingsService.ServerEndpoint = sourcegraphUrl;
        }

        protected override void OnDeactivate(CancelEventArgs e)
        {
            _control.ForceBindingsUpdate();

            base.OnDeactivate(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _logger.Debug($"Settings page closed.");

            base.OnClosed(e);
        }

        public override void ResetSettings()
        {
            _settingsService.AccessToken = string.Empty;
            _settingsService.ServerEndpoint = string.Empty;

            base.ResetSettings();
        }

        protected override UIElement Child
        {
            get
            {
                if (_control == null)
                {
                    _logger.Debug("Creating options control ...");
                    
                    _control = new GeneralOptionsControl();
                    _generalOptionsViewModel = new GeneralOptionsViewModel(_logger);
                    _control.DataContext = _generalOptionsViewModel;
                }

                return _control;
            }
        }
    }
}
