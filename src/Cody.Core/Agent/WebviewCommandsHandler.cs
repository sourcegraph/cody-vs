using Cody.Core.Agent.Protocol;
using Cody.Core.Settings;
using Cody.Core.Workspace;
using Newtonsoft.Json.Linq;
using System;

namespace Cody.Core.Agent
{
    public class WebviewCommandsHandler
    {
        private readonly IUserSettingsService _settingsService;
        private readonly IFileService _fileService;
        private readonly Action _onOptionsPageShowRequest;

        public WebviewCommandsHandler(IUserSettingsService settingsService, IFileService fileService, Action onOptionsPageShowRequest)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _onOptionsPageShowRequest = onOptionsPageShowRequest;
        }

        public bool HandleMessage(string message)
        {
            try
            {
                dynamic json = JObject.Parse(message);
                // Return true for message requests that only need to be handled by the client.
                // Return false for messages that are not handled by the filter,
                // or for messages that are intercepted but should still be forwarded to the agent.
                switch (json.command?.ToString())
                {
                    case "auth":
                        return HandleAuthCommand(json);
                    case "command":
                        return HandleCommandCommand(json);
                    case "openFileLink":
                        return HandleOpenFileLinkCommand(json);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool HandleAuthCommand(dynamic json)
        {
            if (json.authKind == "signout")
            {
                _settingsService.AccessToken = string.Empty;
            }
            else if (json.authKind == "signin")
            {
                var token = json.value;
                var endpoint = json.endpoint;

                _settingsService.ServerEndpoint = endpoint;
                _settingsService.AccessToken = token;
            }
            // Always return false to allow the request to be forwarded to the agent.
            return false;
        }

        private bool HandleCommandCommand(dynamic json)
        {
            if (json.id == "cody.status-bar.interacted")
            {
                _onOptionsPageShowRequest?.Invoke();
                return true;
            }
            return false;
        }

        private bool HandleOpenFileLinkCommand(dynamic json)
        {
            var path = json.uri?.path?.ToString();
            var range = json.range?.ToObject<Range>();
            if (!string.IsNullOrEmpty(path))
            {
                _fileService.OpenFileInEditor(path, range);
                return true;
            }
            return false;
        }
    }
}
