using System;
using System.Diagnostics;
using Cody.Core.Logging;
using Cody.UI.MVVM;

namespace Cody.UI.ViewModels
{
    public class GeneralOptionsViewModel: NotifyPropertyChangedBase
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
            get { return _getTokenCommand = _getTokenCommand ?? new DelegateCommand(GetTokenCommandHandler); }
        }

        private void GetTokenCommandHandler()
        {
            var uri = string.Empty;
            try
            {
                uri = new Uri("https://sourcegraph.com/user/settings/tokens").AbsoluteUri;
                Process.Start(new ProcessStartInfo(uri));
            }
            catch(Exception ex)
            {
                _logger.Error($"Opening '{uri}' failed.", ex);
            }
        }

        private string _accessToken;

        public string AccessToken
        {
            get => _accessToken;
            set
            {
                if (SetProperty(ref _accessToken, value))
                {
                    _logger.Debug($"Access Token set:{AccessToken}");
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

    }
}
