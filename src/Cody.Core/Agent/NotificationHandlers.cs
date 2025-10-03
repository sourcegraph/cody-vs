using Cody.Core.Agent.Protocol;
using Cody.Core.Ide;
using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Cody.Core.Settings;
using Cody.Core.Trace;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cody.Core.Agent
{
    public class NotificationHandlers
    {
        private static TraceLogger trace = new TraceLogger(nameof(NotificationHandlers));

        private readonly Task<IInfobarNotifications> _infobarNotificationsAsync;
        private readonly ILog _logger;
        private readonly IUserSettingsService _settingsService;
        private readonly IStatusbarService _statusbarService;
        private readonly IToastNotificationService _toastNotificationService;


        public event EventHandler<ProtocolAuthStatus> AuthorizationDetailsChanged;
        public event EventHandler OnFocusSidebarRequest;

        public NotificationHandlers(
            IUserSettingsService settingsService,
            ILog logger,
            IDocumentService documentService,
            Task<IInfobarNotifications> infobarNotificationsAsync,
            IStatusbarService statusbarService,
            IToastNotificationService toastNotificationService)
        {
            _settingsService = settingsService;
            _infobarNotificationsAsync = infobarNotificationsAsync;
            _logger = logger;
            _statusbarService = statusbarService;
            _toastNotificationService = toastNotificationService;
        }

        [AgentCallback("debug/message")]
        public void Debug(string channel, string message, string level)
        {
            //_logger.Debug($"[{channel} {message}]");
            trace.TraceEvent("AgentDebug", message);
        }

        [AgentCallback("window/didChangeContext")]
        public void WindowDidChangeContext(string key, string value)
        {
            _logger.Debug(value, $@"WindowDidChangeContext Key - {key}");

            // Check the value to see if Cody is activated or deactivated
            // Deactivated: value = "false", meaning user is no longer authenticated.
            // In this case, we can send Agent a request to get the latest user AuthStatus to
            // confirm if the user is logged out or not.
            if (key == "cody.activated")
            {
                var isAuthenticated = value == "true";
                _logger.Debug(isAuthenticated.ToString(), "User is authenticated");
            }
        }

        [AgentCallback("extensionConfiguration/didChange", deserializeToSingleObject: true)]
        public void ExtensionConfigDidChange(ExtensionConfiguration config)
        {
            _logger.Debug(config.ToString());
        }

        [AgentCallback("ignore/didChange")]
        public void IgnoreDidChange()
        {
            _logger.Debug("Changed");
        }

        [AgentCallback("env/openExternal")]
        public Task<bool> OpenExternalLink(CodyFilePath path)
        {
            // Open the URL in the default browser
            System.Diagnostics.Process.Start(path.Uri);
            return Task.FromResult(true);

        }

        
        [AgentCallback("statusBar/didChange", deserializeToSingleObject: true)]
        public void StatusBarChanged(StatusBarChangeParams param)
        {
            _logger.Info($"statusbar status: {param.TextWithIcon} - {param.Tooltip}");

            var match = Regex.Match(param.TextWithIcon, @"\$\(([^)]+)\)(?:\s+(.+))?");

            string icon = null;
            string text = null;
            string tooltip = param.Tooltip;

            if (match.Success)
            {
                icon = match.Groups[1].Value;
                text = match.Groups[2].Success ? match.Groups[2].Value : null;
            }

            CodyStatus status = CodyStatus.Hide;
            if (text == "Sign In") status = CodyStatus.Unavailable;
            else if (icon == "cody-logo-heavy") status = CodyStatus.Available;
            else if (icon == "cody-logo-heavy-slash") status = CodyStatus.Unavailable;
            else if (icon == "loading~spin") status = CodyStatus.Loading;

            if (icon == "cody-logo-heavy" && tooltip == "Cody Settings") tooltip = "Cody ready. Click to open Cody Chat.";

            _statusbarService.SetCodyStatus(status, tooltip, text);
        }

        [AgentCallback("window/focusSidebar")]
        public void FocusSidebar(object param)
        {
            OnFocusSidebarRequest?.Invoke(this, EventArgs.Empty);
        }

        [AgentCallback("authStatus/didUpdate", deserializeToSingleObject: true)]
        public void AuthStatusDidUpdate(ProtocolAuthStatus authStatus)
        {
            _logger.Debug($"Pending validation: {authStatus.PendingValidation}");

            if (authStatus.PendingValidation)
                return;

            _logger.Debug($"Authenticated: {authStatus.Authenticated}");

            AuthorizationDetailsChanged?.Invoke(this, authStatus);
        }

        [AgentCallback("window/showMessage", deserializeToSingleObject: true)]
        public async Task<string> ShowMessage(ShowWindowMessageParams param)
        {
            // TODO: supports only single auto-edit notification for now
            // because how the code handles enabling/disabling auto-edits via UserSettingsService
            if (param.Message.Contains("You have been enrolled to Cody Auto-edit"))
            {

                var notifications = await _infobarNotificationsAsync;

                _logger.Debug($"ℹ ShowMessage:{param.Message}");
                var selectedValue = await notifications.Show(param);

                _logger.Debug($"Selected value: '{selectedValue}'");

                if (selectedValue == null) // an user want to stay on auto-edits
                    _settingsService.EnableAutoEdit = true;

                return selectedValue;
            }
            else if (param.Message.StartsWith("Edit applied to"))
            {
                _statusbarService.SetText(param.Message);
            }
            else if (param.Items != null && param.Items.Count > 0 && !string.IsNullOrEmpty(param.Items[0]))
            {
                var result = await _toastNotificationService.ShowNotification(
                    param.Severity, param.Message, param.Options?.Detail, param.Items);
                return result;
            }
            else
            {
                _statusbarService.SetText(param.Message);
            }

            return null;
        }
    }
}
