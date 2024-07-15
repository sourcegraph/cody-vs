namespace Cody.Core.Settings
{
    public interface IUserSettingsService
    {
        string AnonymousUserID { get; set; }

        string AccessToken { get; set; }
        string ServerEndpoint { get; set; }
    }
}