using StudentsManager.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace StudentsManager.Services
{
    public class Oauth2TokensService
    {
        public async Task GetOauthToken(Oauth2Config oauth2Config, int authCodeReceivingTimeoutSeconds)
        {
            CodeGrantOauth auth = new CodeGrantOauth(oauth2Config);

            if (string.IsNullOrEmpty(oauth2Config.RefreshToken))
            {
                await auth.Oauth2Authorize(authCodeReceivingTimeoutSeconds);

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
                    await GetOauthToken(oauth2Config, authCodeReceivingTimeoutSeconds);
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

            public async Task Oauth2Authorize(int authCodeReceivingTimeout)
            {
                try
                {
                    authorizationCode = await GetAuthCode(authCodeReceivingTimeout);

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
            async Task<string> GetAuthCode(int authCodeReceivingTimeoutSeconds)
            {
                using (var httpListener = new HttpListener())
                {
                    httpListener.Prefixes.Add(this.config.CallbackUrl + "/");

                    httpListener.Start();

                    var url = string.Format("{0}?client_id={1}&scope={2}&response_type=code&redirect_uri={3}",
                        config.AuthUrl, config.ClientId, config.Scope, config.CallbackUrl);

                    OpenBrowser(url);

                    var cancellationToken = new CancellationTokenSource();

                    cancellationToken.CancelAfter(authCodeReceivingTimeoutSeconds * 1000);

                    var code = await ReceiveCode(httpListener, cancellationToken.Token);

                    return code;
                }
            }

            private async Task<string> ReceiveCode(HttpListener httpListener, CancellationToken ct)
            {
                ct.Register(() =>
                {
                    httpListener.Stop();
                });

                try
                {
                    var context = await httpListener.GetContextAsync();

                    return context.Request.QueryString.Get("code");
                }
                catch when (ct.IsCancellationRequested)
                {
                    throw new Exception("Auth Code receiving timeout! Please, restart program and try again.");
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

            private void OpenBrowser(string url)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = Regex.Replace(url, "(\\\\*)\"", "$1$1\\\"");
                    url = Regex.Replace(url, "(\\\\+)$", "$1$1");
                    Process.Start(new ProcessStartInfo("cmd", "/c start \"\" \"" + url + "\"")
                    {
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    throw new NotSupportedException(
                        "Failed to launch browser for authorization; platform not supported.");
                }
                else
                {
                    Process.Start("open", url);
                }
            }
            #endregion
        }
    }
}