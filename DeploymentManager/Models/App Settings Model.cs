// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Shared;

namespace DeploymentManager.Models
{
    /// <summary>
    /// Stores the settings used by the site.
    /// </summary>
    public class AppSettingsModel
    {
        public string SiteAuth { get; set; }
        public string DeploymentHistoryLocation { get; set; }
        public string ApprovalCredentialLocation { get; set; }
        public string ArtefactDownloadLocation { get; set; }
        public string ProjectConfigurationLocation { get; set; }
        public List<EnvironmentModel> Environments { get; set; }
        public GitHubOptionsModel GitHubOptions { get; set; }
        public List<ProjectModel> Projects { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int LoginAttemptWindowMinutes { get; set; }
        public int LockoutDurationMinutes { get; set; }
    }
}
