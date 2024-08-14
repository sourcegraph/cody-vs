using System;
using System.ComponentModel;
using System.Windows;
using Cody.Core.Logging;
using Cody.UI.Controls.Options;
using Cody.UI.ViewModels;
using Cody.VisualStudio.Inf;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Options
{
    public class GeneralOptionsPage : UIElementDialogPage
    {

        private GeneralOptionsControl _control;
        private WritableSettingsStore _settingsStore;

        private CodyPackage _codyPackage;
        private GeneralOptionsViewModel _generalOptionsViewModel;
        private ILog _logger;

        public GeneralOptionsPage()
        {
            _codyPackage = GetPackage();
            if (_codyPackage != null)
            {
                _logger = _codyPackage.Logger;
                _logger.Debug("Initialization ...");
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


            _logger.Debug($"Is canceled:{e.Cancel}");

            base.OnActivate(e);
        }
        protected override void OnApply(PageApplyEventArgs args)
        {
            _logger.Debug($"{args.ApplyBehavior}");

            var accessToken = _generalOptionsViewModel.AccessToken;
            var sourcegraphUrl = _generalOptionsViewModel.SourcegraphUrl;
            ;
        }

        protected override void OnDeactivate(CancelEventArgs e)
        {
            _control.ForceBindingsUpdate();

            base.OnDeactivate(e);
        }

        //protected override void LoadSettingFromStorage(PropertyDescriptor prop)
        //{
        //    base.LoadSettingFromStorage(prop);
        //}

        //protected override void SaveSetting(PropertyDescriptor property)
        //{
        //    base.SaveSetting(property);
        //}

        //protected override object GetDefaultPropertyValue(PropertyDescriptor property)
        //{
        //    return base.GetDefaultPropertyValue(property);
        //}

        protected override void OnClosed(EventArgs e)
        {
            _logger.Debug($"Settings page closed.");

            base.OnClosed(e);
        }

        public override void ResetSettings()
        {
            base.ResetSettings();
        }

        private void InitializeSettingsStore()
        {
            try
            {
                //var cModel = (IComponentModel)(Site.GetService(typeof(SComponentModel))); // for VS 2019 https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.componentmodelhost.scomponentmodel?view=visualstudiosdk-2022
                //var sp = cModel.GetService<SVsServiceProvider>();
                //var manager = new ShellSettingsManager(sp);

                //_settingsStore = manager.GetWritableSettingsStore(SettingsScope.UserSettings);

                _logger.Debug("VS settings store initialized.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed VS settings store initialization.", ex);
            }
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

        public override void LoadSettingsFromStorage()
        {
            try
            {
                _logger.Debug("Loading settings ...");

                _logger.Debug("Settings loaded.");
                base.LoadSettingsFromStorage(); // without it, LoadSettingsFromStorage() is called second time
            }
            catch (Exception ex)
            {
                _logger.Error("Failed loading settings.", ex);
            }
        }
    }
}
