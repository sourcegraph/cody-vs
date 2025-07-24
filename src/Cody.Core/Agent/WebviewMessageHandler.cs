using Cody.Core.Agent.Protocol;
using Cody.Core.Settings;
using Cody.Core.Workspace;
using Newtonsoft.Json.Linq;
using System;
using Cody.Core.Ide;
using Cody.Core.Logging;

namespace Cody.Core.Agent
{
    public class WebviewMessageHandler
    {
        private readonly IUserSettingsService _settingsService;
        private readonly IFileService _fileService;
        private readonly IDocumentService _documentService;
        private readonly Action _onOptionsPageShowRequest;
        private readonly ILog _logger;

        public WebviewMessageHandler(
            IUserSettingsService settingsService,
            IFileService fileService,
            IDocumentService documentService,
            Action onOptionsPageShowRequest,
            ILog logger
            )
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _documentService = documentService;
            _onOptionsPageShowRequest = onOptionsPageShowRequest;
            _logger = logger;
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
                    case "insert":
                        return HandleTextInsert(json.text.ToString());
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool HandleTextInsert(string text)
        {
            return _documentService.InsertAtCursor(text);
        }

        private bool HandleAuthCommand(dynamic json)
        {
            // login/logout handled by the agent via accessing secret storage 

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
