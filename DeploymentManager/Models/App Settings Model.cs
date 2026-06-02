// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Related;

namespace DeploymentManager.Models
{
    /// <summary>
    /// Stores the settings used by the site.
    /// </summary>
    public class AppSettingsModel
    {
        public required string SiteAuth { get; set; }
        public required string DeploymentHistoryLocation { get; set; }
        public required List<EnvironmentModel> Environments { get; set; }
        public required GitHubOptionsModel GitHubOptions { get; set; }
        public required List<ProjectModel> Projects { get; set; }
    }
}
