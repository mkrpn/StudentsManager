namespace StudentsManager.Models.GoogleSheets
{
    /// <summary>
    /// Enable Sheets API on your account, Generate OAuth 2.0 Credentials with access to Sheets
    /// Put Client ID and Client Secret into 'googleSheets' section in appSettings.json 
    /// https://console.cloud.google.com/marketplace/product/google/sheets.googleapis.com
    /// https://console.cloud.google.com/apis/credentials
    /// </summary>
    public class GoogleSheetsConfig : Oauth2Config
    {
        public const string configName = "GoogleSheets";
        public override string ConfigName => configName;
    }
}
