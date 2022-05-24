using StudentsManager.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace StudentsManager.Services
{
    public class Oauth2TokensService
    {
        public async Task GetOauthToken(Oauth2Config oauth2Config)
        {
            CodeGrantOauth auth = new CodeGrantOauth(oauth2Config);

            if (string.IsNullOrEmpty(oauth2Config.RefreshToken))
            {
                await auth.Oauth2Authorize();

                if (!string.IsNullOrEmpty(auth.Error))
                {
                    throw new Exception(auth.Error);
                }
            }
            else
            {
                await auth.RefreshAccessToken();

                if (!string.IsNullOrEmpty(auth.Error))
                {
                    oauth2Config.RefreshToken = null;
                    await GetOauthToken(oauth2Config);
                }
            }
        }

        public class CodeGrantOauth
        {
            const string GRANT_TYPE_AUTHORIZATION_CODE = "authorization_code";
            const string GRANT_TYPE_REFRESH_TOKEN = "refresh_token";

            string authorizationCode = null;
            Oauth2Config config = null;

            public string Error { get; private set; }

            public CodeGrantOauth(Oauth2Config config)
            {
                this.config = config;
            }

            public async Task Oauth2Authorize()
            {
                try
                {
                    authorizationCode = GetAuthCode();

                    var tokenData = await GetToken(this.config.AccessTokenUrl, GRANT_TYPE_AUTHORIZATION_CODE);

                    config.Token = tokenData.AccessToken;
                    config.RefreshToken = tokenData.RefreshToken;
                    config.ExpirationDate = DateTime.UtcNow.AddSeconds(tokenData.Expiration);
                    
                }
                catch (Exception ex)
                {
                    Error = ex.ToString();
                }
            }

            public async Task RefreshAccessToken()
            {
                try
                {
                    var tokenData = await GetToken(config.AccessTokenUrl, GRANT_TYPE_REFRESH_TOKEN);

                    config.Token = tokenData.AccessToken;
                    config.ExpirationDate = DateTime.UtcNow.AddSeconds(tokenData.Expiration);

                    if (!string.IsNullOrEmpty(tokenData.RefreshToken))
                    {
                        config.RefreshToken = tokenData.RefreshToken;
                    }
                }
                catch (Exception ex)
                {
                    Error = ex.ToString();
                }
            }

            #region Private
            string GetAuthCode()
            {
                using (var httpListener = new HttpListener())
                {
                    httpListener.Prefixes.Add(this.config.CallbackUrl + "/");

                    httpListener.Start();

                    var url = string.Format("{0}?client_id={1}&scope={2}&response_type=code&redirect_uri={3}",
                        config.AuthUrl, config.ClientId, config.Scope, config.CallbackUrl);

                    Process.Start(new ProcessStartInfo("cmd", "/c start \"\" \"" + url + "\"")
                    {
                        CreateNoWindow = true
                    });

                    return httpListener.GetContext().Request.QueryString.Get("code");
                }
            }

            async Task<AccessTokenData> GetToken(string uri, string grantType)
            {
                using (var httpClient = new HttpClient())
                {
                    var formData = new Dictionary<string, string>
                    {
                        { "grant_type",  grantType},
                        { "redirect_uri",  config.CallbackUrl},
                        { "client_id",  config.ClientId}
                    };

                    if(grantType == GRANT_TYPE_AUTHORIZATION_CODE)
                    {
                        formData.Add("code", authorizationCode);
                    }
                    else if (grantType == GRANT_TYPE_REFRESH_TOKEN)
                    {
                        formData.Add("refresh_token", config.RefreshToken);
                    }

                    var formContent = new FormUrlEncodedContent(formData);
                    var authenticationString = $"{config.ClientId}:{config.ClientSecret}";
                    var base64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

                    var response = await httpClient.PostAsync(uri.ToString(), formContent);
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(JsonConvert.SerializeObject(response));
                    }

                    return JsonConvert.DeserializeObject<AccessTokenData>(jsonContent);
                }
            }
            #endregion
        }
    }
}