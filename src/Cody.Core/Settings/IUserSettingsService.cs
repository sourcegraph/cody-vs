using System;

namespace Cody.Core.Settings
{
    public interface IUserSettingsService
    {
        string AnonymousUserID { get; set; }

        string AccessToken { get; set; }
        string ServerEndpoint { get; set; }
        string CustomConfiguration { get; set; }
        bool AcceptNonTrustedCert { get; set; }


        event EventHandler AuthorizationDetailsChanged;
    }
}
