// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores the project specific GitHub details.
    /// </summary>
    public class GitHubModel
    {
        public required string Repository { get; set; }
        public required string Artefact { get; set; }
    }
}
