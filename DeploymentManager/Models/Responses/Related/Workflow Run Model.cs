// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Responses.Related
{
    /// <summary>
    /// Stores the workflow run data.
    /// </summary>
    public class WorkflowRunModel
    {
        public required long Id { get; set; }
        public required string Head_Branch { get; set; }
        public required string Head_Sha { get; set; }
    }
}
