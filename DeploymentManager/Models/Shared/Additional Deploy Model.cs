// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Related;

namespace DeploymentManager.Models.Shared
{
    /// <summary>
    /// Stores the additional deployment details.
    /// </summary>
    public class AdditionalDeployModel
    {
        public required string Device { get; set; }
        public DeviceAuthModel? Auth { get; set; }
        public required string Drive { get; set; }
        public required string Directory { get; set; }
    }
}
