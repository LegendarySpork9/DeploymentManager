// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Responses.Related
{
    /// <summary>
    /// Stores the artefact data.
    /// </summary>
    public class ArtefactModel
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required long Size_in_Bytes { get; set; }
        public required string Archive_Download_Url { get; set; }
        public required WorkflowRunModel Workflow_Run { get; set; }
    }
}
