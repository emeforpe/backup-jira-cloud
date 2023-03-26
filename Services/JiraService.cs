using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using BackupJiraCloud.Entities;
using BackupJiraCloud.Configuration;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace BackupJiraCloud.Services
{
    public class JiraService : IJiraService
    {
        public IConfiguration _config;
        public IHttpService _httpService;

        private const string JIRA_CONFIG = "JiraConfig";
        private const string RESULT = "result";
        private const string TASK_ID = "taskId";
        private const string RUN_BACKUP_PATH = "/rest/backup/1/export/runbackup";
        private const string GET_LAST_TASKID_PATH = "/rest/backup/1/export/lastTaskId";
        private const string GET_PROGRESS_PATH = "/rest/backup/1/export/getProgress?taskId=";
        private const string RESULT_ID_PATH = "/plugins/servlet/";

        public JiraService(IConfiguration config, IHttpService httpService)
        {
            _config = config;
            _httpService = httpService;
        }

        public string CreateJiraBackup()
        {

            var jiraConfig = _config.GetSection(JIRA_CONFIG).Get<JiraConfig>();
            if (jiraConfig == null || string.IsNullOrEmpty(jiraConfig.UserEmail) || string.IsNullOrEmpty(jiraConfig.ApiToken)) return string.Empty;

            var postResponse = _httpService.PostAsync(string.Concat(jiraConfig.HostUrl, RUN_BACKUP_PATH), jiraConfig.UserEmail, jiraConfig.ApiToken)
                            .GetAwaiter().GetResult();

            if (postResponse.StatusCode == HttpStatusCode.OK)
            {
                var content = postResponse.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(content)) { return string.Empty; }

                var data = JObject.Parse(content);
                var taskId = (string?)data[TASK_ID];
                if (string.IsNullOrEmpty(taskId)) { return string.Empty; }

                Console.WriteLine($"-> Backup process successfully started (taskId={taskId})");

                while (!JObject.Parse(content).ContainsKey(RESULT))
                {
                    var statusResponse = _httpService.GetAsync(string.Concat(jiraConfig.HostUrl, GET_PROGRESS_PATH, taskId), jiraConfig.UserEmail, jiraConfig.ApiToken)
                                    .GetAwaiter().GetResult();
                    content = statusResponse.Content.ToString() ?? string.Empty;
                }

                var resultId = (string?)JObject.Parse(content)[RESULT];

                return string.Concat(jiraConfig.HostUrl, RESULT_ID_PATH, resultId);

            }
            else if (postResponse.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                var getLastTaskIdResponse = _httpService.GetAsync(string.Concat(jiraConfig.HostUrl, GET_LAST_TASKID_PATH), jiraConfig.UserEmail, jiraConfig.ApiToken)
                                        .GetAwaiter().GetResult();

                if (getLastTaskIdResponse.StatusCode != HttpStatusCode.OK)
                    throw new Exception(getLastTaskIdResponse.RequestMessage != null ? getLastTaskIdResponse.RequestMessage.ToString() : "Response Error!");
                else
                {
                    var taskId = getLastTaskIdResponse.Content.ReadAsStringAsync().Result;

                    var getProgressResponse = _httpService.GetAsync(string.Concat(jiraConfig.HostUrl, GET_PROGRESS_PATH, taskId), jiraConfig.UserEmail, jiraConfig.ApiToken)
                                    .GetAwaiter().GetResult();
                    var content = getProgressResponse.Content.ReadAsStringAsync().Result;
                    var resultId = (string?)JObject.Parse(content)[RESULT];

                    return string.Concat(jiraConfig.HostUrl, RESULT_ID_PATH, resultId);
                }

            }
            else
                throw new Exception(postResponse.RequestMessage != null ? postResponse.RequestMessage.ToString() : "Response Error!");

        }

        public void DownloadFile(string url, string fileName)
        {
            var jiraConfig = _config.GetSection(JIRA_CONFIG).Get<JiraConfig>();
            if (jiraConfig == null || string.IsNullOrEmpty(jiraConfig.BackupPath) || string.IsNullOrEmpty(jiraConfig.UserEmail) || string.IsNullOrEmpty(jiraConfig.ApiToken))
            throw new Exception("Backup path not found!");

            Console.WriteLine($"-> Downloading file from URL: {url}");

            var filePath = Path.Combine(jiraConfig.BackupPath, fileName);
            
            var content = _httpService.GetByteArrayAsync(url, jiraConfig.UserEmail, jiraConfig.ApiToken).GetAwaiter().GetResult();

            if (content != null)
                File.WriteAllBytes(filePath, content);

            Console.WriteLine(filePath);
        }
    }
}