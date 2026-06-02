// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores the details of deployment files to ignore.
    /// </summary>
    public class IgnoreModel
    {
        public required IgnoreType Type { get; set; }
        public required string Name { get; set; }
    }
}
