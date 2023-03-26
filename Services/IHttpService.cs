using BackupJiraCloud.Configuration;
using BackupJiraCloud.Entities;
using BackupJiraCloud.Services;

namespace BackupJiraCloud.Services
{
    public interface IHttpService
    {
        Task<byte[]?> GetByteArrayAsync(string url, string user, string token);
        Task<HttpResponseMessage> GetAsync(string url, string user, string token);
        Task<HttpResponseMessage> PostAsync(string url, string user, string token);
    }
}