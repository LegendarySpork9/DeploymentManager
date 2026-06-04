// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Data.Related
{
    /// <summary>
    /// Stores information about the artefact file.
    /// </summary>
    public class ArtefactFileModel
    {
        public required string Name { get; set; }
        public required KeyValuePair<string, string> Paths { get; set; }
    }
}
