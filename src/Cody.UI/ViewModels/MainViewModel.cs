using Cody.Core.Logging;
using Cody.UI.MVVM;

namespace Cody.UI.ViewModels
{
    public class MainViewModel: NotifyPropertyChangedBase
    {
        private readonly ILog _logger;

        public MainViewModel(ILog logger)
        {
            _logger = logger;

            _logger.Debug("Initialized.");
        }
    }
}
