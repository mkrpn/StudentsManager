using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using StudentsManager.Models.GoogleSheets;

namespace StudentsManager.Services
{
    public class GoogleSheetsService: OAuth2Service
    {
        readonly HttpService httpService;
        const string baseUrl = "https://content-sheets.googleapis.com/v4/spreadsheets";

        public GoogleSheetsService(IOptions<GoogleSheetsConfig> config, HttpService httpService, Oauth2TokensService oauth2TokensService,
            AppConfigService configService) : base(oauth2TokensService, configService, config.Value)
        {
            this.httpService = httpService;
        }

        public async Task UpdateRange(string sheetId, string range, IEnumerable<string> values)
        {
            var apiToken = oauth2Config.Token;
            var url = $"{baseUrl}/{sheetId}/values/{range}?valueInputOption=RAW&alt=json";

            var data = new
            {
                range = range,
                values = values.Select(v => new[] { v })
            };

            await httpService.ExecuteRequest<object>(url, apiToken, HttpMethod.Put, data);
        }

        public async Task<IEnumerable<string>> GetRangeValues(string sheetId, string range)
        {
            var apiToken = oauth2Config.Token;
            var url = $"{baseUrl}/{sheetId}/values/{range}";

            var result = await httpService.ExecuteRequest<GetSheetValuesResponse>(url, apiToken, HttpMethod.Get);

            return result.Values.Select(v => v.FirstOrDefault());
        }

        public bool IsColumnNameValid(string name) =>
            Regex.Match(name, @"[A-z]+").Success;
    }
}
