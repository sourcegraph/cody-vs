using System;

namespace Cody.Core.Settings
{
    public interface IUserSettingsService
    {
        string AnonymousUserID { get; set; }

        string AccessToken { get; set; }
        string DefaultServerEndpoint { get; }
        string CustomConfiguration { get; set; }
        bool AcceptNonTrustedCert { get; set; }
        bool AutomaticallyTriggerCompletions { get; set; }
        bool ForceAccessTokenForUITests { get; set; }
        bool LastTimeAuthorized { get; set; }
    }
}
