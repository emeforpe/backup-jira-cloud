namespace BackupJiraCloud.Configuration
{
    public class JiraConfig
    {
        public string? UserEmail { get; set; }
        public string? ApiToken { get; set; }
        public bool IncludeAttachments { get; set; }
        public string? HostUrl { get; set; }
        public string? BackupPath { get; set; }
    }
}