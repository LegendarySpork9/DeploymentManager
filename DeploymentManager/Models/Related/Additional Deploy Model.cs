// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores the additional deployment details.
    /// </summary>
    public class AdditionalDeployModel
    {
        public required string Device { get; set; }
        public required string Drive { get; set; }
        public required string Directory { get; set; }
    }
}
