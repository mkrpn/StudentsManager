using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace StudentsManager.Services
{
    public class HttpService
    {
        public async Task<T> ExecuteRequest<T>(string url, string token, HttpMethod method, object data = null)
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage(method, url);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            else
            {
                throw new Exception(responseString);
            }
        }
    }
}
