namespace Cody.Core.Settings
{
    internal interface ISettingsService
    {
        string AccessToken { get; set; }
        string ServerEndpoint { get; set; }
    }
}