using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace Cody.Core.Agent
{
    public class WebviewMessageHandler
    {
        private readonly ILog _logger;

        private readonly Action _onOptionsPageShowRequest;

        public WebviewMessageHandler(Action onOptionsPageShowRequest, ILog logger)
        {
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
                    case "command":
                        return HandleCommandCommand(json);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
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
    }
}
