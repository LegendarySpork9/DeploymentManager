// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Responses.Related;

namespace DeploymentManager.Models.Responses
{
    /// <summary>
    /// Stores the list artefact api response.
    /// </summary>
    public class ArtefactListModel
    {
        public required int Total_Count { get; set; }
        public required List<ArtefactModel> Artifacts { get; set; }
    }
}
