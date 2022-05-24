namespace StudentsManager.Models.Zoom
{
    /// <summary>
    /// Create OAuth app in Zoom marketplace with 'Redirect URL for OAuth' = http://localhost/zoom
    /// https://marketplace.zoom.us/develop/create
    /// Created app should be displayed in 'Created Apps' => https://marketplace.zoom.us/user/build
    /// Put Client ID and Client Secret into 'zoom' section in appSettings.json 
    /// </summary>
    public class ZoomConfig : Oauth2Config
    {
        public const string configName = "Zoom";
        public override string ConfigName => configName;
    }
}
