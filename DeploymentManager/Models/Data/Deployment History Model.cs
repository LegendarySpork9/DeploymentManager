// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Data.Related;

namespace DeploymentManager.Models.Data
{
    /// <summary>
    /// Stores the deployment history data for a project. 
    /// </summary>
    public class DeploymentHistoryModel
    {
        public required int Id { get; set; }
        public required DeploymentType Type { get; set; }
        public required Status Status { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required TimeSpan RunTime { get; set; }
        public required long ArtefactId { get; set; }
        public required string ArtefactName { get; set; }
        public required long ArtefactSize { get; set; }
        public required string GitHubBranchId { get; set; }
        public required string GitHubBranchName { get; set; }
        public required List<StageModel> Stages { get; set; }
    }
}
