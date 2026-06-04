// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Data
{
    /// <summary>
    /// Stores information about the uploaded file.
    /// </summary>
    public class UploadFileModel
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required long Size { get; set; }
        public required string BranchId { get; set; }
        public required string BranchName { get; set; }
        public required string Directory { get; set; }
    }
}
