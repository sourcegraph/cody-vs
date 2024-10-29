using Cody.Core.Logging;
using Cody.UI.MVVM;
using System;
using System.Diagnostics;

namespace Cody.UI.ViewModels
{
    public class GeneralOptionsViewModel : NotifyPropertyChangedBase
    {
        private readonly ILog _logger;

        public GeneralOptionsViewModel(ILog logger)
        {
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

        private string _codyConfig;

        public string CodyConfigurations
        {
            get => _codyConfig;
            set
            {
                if (SetProperty(ref _codyConfig, value))
                {
                    _logger.Debug($"Cody Configurations set:{CodyConfigurations}");
                }

            }
        }

        private string _sourcegraphUrl;
        public string SourcegraphUrl
        {
            get => _sourcegraphUrl;
            set
            {
                if (SetProperty(ref _sourcegraphUrl, value))
                {
                    _logger.Debug($"Sourcegraph Url set:{SourcegraphUrl}");
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
                    _logger.Debug($"AcceptNonTrustedCert set:{SourcegraphUrl}");
                }
            }
        }

        private bool _automaticallyTriggerCompletions;
        public bool AutomaticallyTriggerCompletions
        {
            get => _automaticallyTriggerCompletions;
            set
            {
                if (SetProperty(ref _automaticallyTriggerCompletions, value))
                {
                    _logger.Debug($"AutomaticallyTriggerCompletions set:{value}");
                }
            }
        }

    }
}
