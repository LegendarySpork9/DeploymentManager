// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Shared;

namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores the configuration for the project.
    /// </summary>
    public class ProjectModel
    {
        public required ProjectType Type { get; set; }
        public required string Name { get; set; }
        public required string Directory { get; set; }
        public required GitHubModel GitHub { get; set; }
        public List<AdditionalDeployModel>? AdditionalDeploy { get; set; }
        public List<IgnoreModel>? Ignore { get; set; }
    }
}
