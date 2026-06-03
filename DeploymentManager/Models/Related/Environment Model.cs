// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores the configuration for the environment.
    /// </summary>
    public class EnvironmentModel
    {
        public required string Device { get; set; }
        public required string Drive { get; set; }
        public required string Name { get; set; }
        public required ArtefactSource ArtefactSource { get; set; }
    }
}
