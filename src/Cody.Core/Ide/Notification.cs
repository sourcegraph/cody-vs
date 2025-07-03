using System;
using System.Threading.Tasks;
using System.Timers;
using Cody.Core.Logging;

namespace Cody.Core.Ide
{
    public class Notification : IDisposable
    {
        private readonly ILog _logger;
        private Timer _autoCloseTimer;

        private bool _disposed = false;

        private readonly TaskCompletionSource<string> _selectedValueCompletionSource = new TaskCompletionSource<string>();
        

        public Task<string> SelectedValueAsync => _selectedValueCompletionSource.Task;

        private TimeSpan _defaultClosingTimeout = TimeSpan.FromSeconds(15);

        public Notification(uint cookie, ILog logger)
        {
            _logger = logger;
            Cookie = cookie;
        }

        public uint Cookie { private set; get; }

        public void SetValue(string s)
        {
            if (!_selectedValueCompletionSource.Task.IsCompleted)
            {
                _selectedValueCompletionSource.SetResult(s);
            }
        }

        public void StartAutoCloseTimer(Action closeCallback)
        {
            if (_disposed || _autoCloseTimer != null) return;

            _autoCloseTimer = new Timer(_defaultClosingTimeout.TotalMilliseconds);
            _autoCloseTimer.Elapsed += (sender, e) =>
            {
                try
                {
                    StopAutoCloseTimer();
                    closeCallback?.Invoke();

                    _logger.Debug("Notification timer invoked.");
                }
                catch(Exception ex)
                {
                    _logger.Error("Running timer failed.", ex);
                }
            };
            _autoCloseTimer.AutoReset = false;
            _autoCloseTimer.Start();
        }

        public void StopAutoCloseTimer()
        {
            if (_autoCloseTimer != null)
            {
                _autoCloseTimer.Stop();
                _autoCloseTimer.Dispose();
                _autoCloseTimer = null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopAutoCloseTimer();
                
                if (!_selectedValueCompletionSource.Task.IsCompleted)
                    _selectedValueCompletionSource.SetResult(null);
                    
                _disposed = true;
            }
        }

    }
}
