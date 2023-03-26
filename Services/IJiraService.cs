namespace BackupJiraCloud.Services
{
    public interface IJiraService
    {
        string CreateJiraBackup();
        void DownloadFile(string url, string fileName);
    }
}