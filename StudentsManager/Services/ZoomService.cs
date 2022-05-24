using Microsoft.Extensions.Options;
using StudentsManager.Models.Zoom;

namespace StudentsManager.Services
{
    public class ZoomService: OAuth2Service
    {
        readonly HttpService httpService;
        const string baseUrl = "https://api.zoom.us/v2";

        public ZoomService(IOptions<ZoomConfig> config, HttpService httpService, Oauth2TokensService oauth2TokensService,
            AppConfigService configService): base(oauth2TokensService, configService, config.Value)
        {
            this.httpService = httpService;
        }

        public async Task<List<string>> GetMeetingParticipants(string meetingId)
        {
            var token = oauth2Config.Token;
            var url = $"{baseUrl}/past_meetings/{meetingId}/participants";

            var response = await httpService.ExecuteRequest<ParticipantsResponse>(url, token, HttpMethod.Get);

            return response.Participants.Select(p => p.Name).ToList();
        }
    }
}
