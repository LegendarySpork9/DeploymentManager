// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Responses.Related;

namespace DeploymentManager.Models.Responses
{
    /// <summary>
    /// Stores the release data.
    /// </summary>
    public class ReleaseModel
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required string Tag_Name { get; set; }
        public required DateTime Published_At { get; set; }
        public required List<AssetModel> Assets { get; set; }
    }
}
