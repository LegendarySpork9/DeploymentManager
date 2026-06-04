// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Shared
{
    /// <summary>
    /// Stores the deployment settings for the deployment.
    /// </summary>
    public class DeploymentSettingsModel
    {
        public bool RunAdditionalDeploys { get; set; } = true;
        public bool RestartService { get; set; } = true;
    }
}
