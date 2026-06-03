// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Responses.Related
{
    /// <summary>
    /// Stores the asset data.
    /// </summary>
    public class AssetModel
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required long Size { get; set; }
        public required string Content_Type { get; set; }
        public required string Browser_Download_Url { get; set; }
    }
}
