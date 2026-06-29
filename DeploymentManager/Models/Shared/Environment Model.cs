// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Models.Shared
{
    /// <summary>
    /// Stores the configuration for the environment.
    /// </summary>
    public class EnvironmentModel
    {
        public required string Device { get; set; }
        public required string Drive { get; set; }
        public required DeploymentEnvironment Name { get; set; }
        public required ArtefactSource ArtefactSource { get; set; }
        public DeviceAuthModel? Auth { get; set; }
    }
}
