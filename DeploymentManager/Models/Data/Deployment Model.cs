// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Data.Related;

namespace DeploymentManager.Models.Data
{
    /// <summary>
    /// Stores information about the deployment files.
    /// </summary>
    public class DeploymentModel
    {
        public required ArtefactFileDeploymentType ArtefactDeploymentType { get; set; }
        public string? Device { get; set; }
        public required string Directory { get; set; }
        public required List<ArtefactFileModel> ArtefactFiles { get; set; }
    }
}
