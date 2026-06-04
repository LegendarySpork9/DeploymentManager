// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Components;
using DeploymentManager.Implementations;
using DeploymentManager.Models;
using DeploymentManager.Orchestrators;
using DeploymentManager.Orchestrators.GitHub;
using DeploymentManager.Services;
using DeploymentManager.Values;

namespace DeploymentManager
{
    public class Program
    {
        /// <summary>
        /// Configures the application at startup.
        /// </summary>
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(
                AppContext.BaseDirectory,
                "log4net.config")));

            ILoggerService _logger = new LoggerServiceWrapper("System");
            _logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Starting Website");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Created Builder");

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Added Razor Components");

            AppSettingsModel appSettings = new();

            builder.Configuration.Bind(
                "AppSettings",
                appSettings);

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Loaded Configuration");

            if (appSettings != null)
            {
                builder.Services.AddSingleton(appSettings);
                builder.Services.AddSingleton(appSettings.GitHubOptions);
            }

            builder.Services.AddSingleton<ILoggerService>(_logger);
            builder.Services.AddSingleton<IFileSystem, FileSystemWrapper>();
            builder.Services.AddSingleton<IGitHubClient, GitHubClientWrapper>();
            builder.Services.AddSingleton<IIISClient, IISClientWrapper>();
            builder.Services.AddSingleton<ITaskScheduler, TaskSchedulerWrapper>();
            builder.Services.AddSingleton<IClock, SystemClockProvider>();
            builder.Services.AddSingleton<IServiceProvider, ServiceProvider>();
            builder.Services.AddSingleton<ApprovalService>();
            builder.Services.AddSingleton<GitHubService>();
            builder.Services.AddSingleton<DocumentService>();
            builder.Services.AddSingleton<IISService>();
            builder.Services.AddSingleton<TaskSchedulerService>();
            builder.Services.AddSingleton<DeploymentHistoryService>();
            builder.Services.AddSingleton<DeploymentOrchestrator>();
            builder.Services.AddHttpContextAccessor();

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Configured Services");

            WebApplication app = builder.Build();

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Built Application");

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Configured HTTPS Redirection");

            app.UseStatusCodePagesWithReExecute(
                "/not-found",
                createScopeForStatusCodePages: true);

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Configured Status Code Pages");

            app.UseAntiforgery();

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Configured Antiforgery");

            app.MapStaticAssets();

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Configured Static Assets");

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            _logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Mapped Razor Components with Interactive Server Render Mode");
            _logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Running Website");

            app.Run();
        }
    }
}
