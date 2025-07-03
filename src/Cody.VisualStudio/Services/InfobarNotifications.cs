using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Cody.Core.Agent.Protocol;
using Cody.Core.Ide;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Services
{
    public class InfobarNotifications: IVsInfoBarUIEvents, IInfobarNotifications
    {
        private readonly ILog _logger;
        private readonly Dictionary<IVsInfoBarUIElement, Notification> _notifications;


        private readonly IVsInfoBarHost _infoBarHost;
        private readonly IVsInfoBarUIFactory _infoBarUiFactory;

        public InfobarNotifications(IVsInfoBarHost infoBarHost, IVsInfoBarUIFactory infoBarUiFactory, ILog logger)
        {
            _infoBarHost = infoBarHost;
            _infoBarUiFactory = infoBarUiFactory;
            _logger = logger;

            _notifications = new Dictionary<IVsInfoBarUIElement, Notification>();
        }

        public void OnClosed(IVsInfoBarUIElement notification)
        {
            Close(notification);
        }

        public void OnActionItemClicked(IVsInfoBarUIElement notification, IVsInfoBarActionItem actionItem)
        {
            try
            {
                _logger.Debug($"Selected '{actionItem.Text}'");

                if (_notifications.TryGetValue(notification, out var n))
                {
                    n.SetValue(actionItem.Text);
                }
                else
                    _logger.Error($"Cannot find matching notification: {notification}");

                Close(notification);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed notification callback.", ex);
            }
        }

        private void Close(IVsInfoBarUIElement notification)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (_notifications.TryGetValue(notification, out var n))
                    {
                        n.StopAutoCloseTimer();
                        
                        var cookie = n.Cookie;
                        notification.Unadvise(cookie);
                        notification.Close();

                        if (!n.SelectedValueAsync.IsCompleted)
                            n.SetValue(null);

                        n.Dispose();
                        _notifications.Remove(notification);
                        _logger.Debug("Notification closed.");
                    }
                    else
                        _logger.Error($"Cannot find matching notification: {notification}");
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed closing notification.", ex);
                }
            });
        }

        public async Task<string> Show(ShowWindowMessageParams messageParams)
        {
            try
            {
                var text = new InfoBarTextSpan(messageParams.Message);
                var items = new List<InfoBarHyperlink>();
                if (messageParams.Items.Any())
                {
                    items.AddRange(messageParams.Items.Where(i => i != null)
                        .Select(item => new InfoBarHyperlink(item)));
                }

                var spans = new[] { text };
                var actions = items.Cast<InfoBarActionItem>().ToArray();
                var infoBarModel = new InfoBarModel(spans, actions, KnownMonikers.InfoTipInline,
                    isCloseButtonVisible: true);

                var notification = await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var notificationBar = _infoBarUiFactory.CreateInfoBar(infoBarModel);
                    notificationBar.Advise(this, out var cookie);
                    _infoBarHost.AddInfoBar(notificationBar);

                    var notificationObj = new Notification(cookie, _logger);
                    notificationObj.StartAutoCloseTimer(() => Close(notificationBar));
                    _notifications.Add(notificationBar, notificationObj);

                    return notificationObj;
                });

                return await notification.SelectedValueAsync;

            }
            catch (Exception ex)
            {
                _logger.Error($"Failed showing notification: {messageParams.Message}", ex);
            }

            return null;
        }
    }
}
