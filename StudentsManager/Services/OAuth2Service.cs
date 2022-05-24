using StudentsManager.Models;

namespace StudentsManager.Services
{
    public class OAuth2Service
    {
        private readonly Oauth2TokensService oauth2TokensService;
        private readonly AppConfigService configService;
        protected readonly Oauth2Config oauth2Config;
        private readonly int authCodeReceivingTimeoutSeconds;

        public OAuth2Service(Oauth2TokensService oauth2TokensService, AppConfigService configService, Oauth2Config oauth2Config, 
            int authCodeReceivingTimeoutSeconds)
        {
            this.oauth2TokensService = oauth2TokensService;
            this.configService = configService;
            this.oauth2Config = oauth2Config;
            this.authCodeReceivingTimeoutSeconds = authCodeReceivingTimeoutSeconds;
        }

        public async Task Oauth2Login()
        {
            await oauth2TokensService.GetOauthToken(oauth2Config, authCodeReceivingTimeoutSeconds);

            configService.AddOrUpdateAppSetting($"{oauth2Config.ConfigName}:{nameof(Oauth2Config.Token)}", oauth2Config.Token);
            configService.AddOrUpdateAppSetting($"{oauth2Config.ConfigName}:{nameof(Oauth2Config.RefreshToken)}", oauth2Config.RefreshToken);
            configService.AddOrUpdateAppSetting($"{oauth2Config.ConfigName}:{nameof(Oauth2Config.ExpirationDate)}", oauth2Config.ExpirationDate);
        }
    }
}