// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Models.Data.Related
{
    /// <summary>
    /// Stores the stage data for a deployment stage.
    /// </summary>
    public class StageModel
    {
        public required DeploymentStage Name { get; set; }
        public required Status Status { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required TimeSpan RunTime { get; set; }
        public List<string>? FailMessages { get; set; }
    }
}
