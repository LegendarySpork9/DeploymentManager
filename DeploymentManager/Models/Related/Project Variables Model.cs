// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores per-environment variable overrides for a project.
    /// </summary>
    public class ProjectVariablesModel
    {
        public string? ServiceName { get; set; }
        public string? Directory { get; set; }
    }
}
