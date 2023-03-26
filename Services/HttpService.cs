using BackupJiraCloud.Configuration;
using BackupJiraCloud.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace BackupJiraCloud.Services
{
    public class HttpService: IHttpService
    {
        public async Task<byte[]?> GetByteArrayAsync(string url, string user, string token)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            var basicHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{token}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicHeaderValue);

            using var result = await client.GetAsync(url);
            return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
        }

        public async Task<HttpResponseMessage> GetAsync(string url, string user, string token)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var basicHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{token}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicHeaderValue);

            return await client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, string user, string token)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            var basicHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{token}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicHeaderValue); 
            
            var triggerContent = new StringContent("{ \"cbAttachments\" : \"true\", \"exportToCloud\" : \"true\" }", Encoding.UTF8, "application/json");

            return await client.PostAsync(url, triggerContent);
        }
    }
}