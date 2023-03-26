using BackupJiraCloud.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)
    .Build();

var builder = new ServiceCollection()
    .AddScoped<IJiraService, JiraService>()
    .AddScoped<IHttpService, HttpService>()
    .AddSingleton<IConfiguration>(configuration)
    .BuildServiceProvider();


var app = builder.GetRequiredService<IJiraService>();

Console.WriteLine("-> Starting backup:");
var url = app.CreateJiraBackup();

Console.WriteLine($"-> Backup URL: {url}");
var fileName = $"jira-cloud-backup_{DateTime.Now:yyyy-MM-dd}.zip";

app.DownloadFile(url, fileName);