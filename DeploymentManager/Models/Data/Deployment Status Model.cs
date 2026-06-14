// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Models.Data
{
    /// <summary>
    /// Stores the last deployment status for a project environment.
    /// </summary>
    public class DeploymentStatusModel
    {
        public required string Project { get; set; }
        public required DeploymentEnvironment Environment { get; set; }
        public required int DeploymentId { get; set; }
        public required string ArtefactName { get; set; }
        public required ArtefactType ArtefactType { get; set; }
        public required string BranchName { get; set; }
        public required DateTime DeployedAt { get; set; }
        public required Status Status { get; set; }
    }
}
