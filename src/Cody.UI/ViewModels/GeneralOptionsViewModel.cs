using Cody.Core.Logging;
using Cody.UI.MVVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Cody.Core.Ide;

namespace Cody.UI.ViewModels
{
    public class GeneralOptionsViewModel : NotifyPropertyChangedBase, IDataErrorInfo
    {
        private readonly IVsVersionService _vsVersionService;
        private readonly ILog _logger;

        public GeneralOptionsViewModel(IVsVersionService vsVersionService, ILog logger)
        {
            _vsVersionService = vsVersionService;
            _logger = logger;

            _logger.Debug("Initialized.");
        }


        private DelegateCommand _getTokenCommand;
        public DelegateCommand ActivateBetaCommand
        {
            get { return _getTokenCommand = _getTokenCommand ?? new DelegateCommand(GetHelpCommandHandler); }
        }

        private void GetHelpCommandHandler()
        {
            var uri = string.Empty;
            try
            {
                uri = new Uri("https://community.sourcegraph.com/").AbsoluteUri;
                Process.Start(new ProcessStartInfo(uri));
            }
            catch (Exception ex)
            {
                _logger.Error($"Opening '{uri}' failed.", ex);
            }
        }

        private bool IsCompletionSupported()
        {
            if (!HasCompletionSupport)
            {
                _logger.Debug("Completion API not supported by VS");
                return false;
            }

            return true;
        }

        public bool HasCompletionSupport => _vsVersionService.HasCompletionSupport;

        private string _customConfiguration;

        public string CustomConfiguration
        {
            get => _customConfiguration;
            set
            {
                if (SetProperty(ref _customConfiguration, value))
                {
                    _logger.Debug($"Custom Configurations set:{CustomConfiguration}");
                }

            }
        }

        private bool _acceptNonTrustedCert;
        public bool AcceptNonTrustedCert
        {
            get => _acceptNonTrustedCert;
            set
            {
                if (SetProperty(ref _acceptNonTrustedCert, value))
                {
                    _logger.Debug($"AcceptNonTrustedCert set:{AcceptNonTrustedCert}");
                }
            }
        }

        private bool _automaticallyTriggerCompletions;
        public bool AutomaticallyTriggerCompletions
        {
            get => _automaticallyTriggerCompletions;
            set
            {
                if (!IsCompletionSupported()) return;

                if (SetProperty(ref _automaticallyTriggerCompletions, value))
                {
                    _logger.Debug($"AutomaticallyTriggerCompletions set:{value}");
                }
            }
        }

        private bool _enableAutoEdit;
        public bool EnableAutoEdit
        {
            get => _enableAutoEdit;
            set
            {
                if (!IsCompletionSupported()) return;

                if (SetProperty(ref _enableAutoEdit, value))
                {
                    _logger.Debug($"EnableAutoEdit set:{value}");
                }
            }
        }

        public bool IsCustomConfigurationValid()
        {
            try
            {
                JsonConvert.DeserializeObject<Dictionary<string, object>>(CustomConfiguration);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(CustomConfiguration))
                {
                    if (!IsCustomConfigurationValid()) return "Invalid custom settings. Make sure you enter the correct JSON (including opening and closing brackets).";
                }

                return null;
            }
        }
    }
}
