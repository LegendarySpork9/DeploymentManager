// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores the settings to access GitHub.
    /// </summary>
    public class GitHubOptionsModel
    {
        public required string Auth { get; set; }
        public required string Owner { get; set; }
    }
}
