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

            var customConfiguration = _settingsService.CustomConfiguration;
            var sourcegraphUrl = _settingsService.ServerEndpoint;
            var acceptNonTrustedCert = _settingsService.AcceptNonTrustedCert;

            _generalOptionsViewModel.SourcegraphUrl = sourcegraphUrl;
            _generalOptionsViewModel.AcceptNonTrustedCert = acceptNonTrustedCert;
            _generalOptionsViewModel.CustomConfiguration = customConfiguration;

            _logger.Debug($"Is canceled:{e.Cancel}");

            base.OnActivate(e);
        }
        protected override void OnApply(PageApplyEventArgs args)
        {
            _logger.Debug($"{args.ApplyBehavior}");

            var customConfiguration = _generalOptionsViewModel.CustomConfiguration;
            var sourcegraphUrl = _generalOptionsViewModel.SourcegraphUrl;
            var acceptNonTrustedCert = _generalOptionsViewModel.AcceptNonTrustedCert;

            _settingsService.CustomConfiguration = customConfiguration;
            _settingsService.ServerEndpoint = sourcegraphUrl;
            _settingsService.AcceptNonTrustedCert = acceptNonTrustedCert;
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
            _settingsService.CustomConfiguration = string.Empty;
            _settingsService.ServerEndpoint = string.Empty;
            _settingsService.AcceptNonTrustedCert = false;

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

                    _codyPackage.GeneralOptionsViewModel = _generalOptionsViewModel;
                }

                return _control;
            }
        }
    }
}
