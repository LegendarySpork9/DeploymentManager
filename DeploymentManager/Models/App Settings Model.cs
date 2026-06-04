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
        public List<EnvironmentModel> Environments { get; set; }
        public GitHubOptionsModel GitHubOptions { get; set; }
        public List<ProjectModel> Projects { get; set; }
    }
}
