// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Converters
{
    public static class DeploymentStageConverter
    {
        public static string FormatStageName(DeploymentStage stage)
        {
            return stage switch
            {
                DeploymentStage.FetchArtefacts => "Fetch Artefacts",
                DeploymentStage.ExtractArtefacts => "Extract Artefacts",
                DeploymentStage.FetchArtefactFiles => "Fetch Artefact Files",
                DeploymentStage.StopServices => "Stop Services",
                DeploymentStage.MoveArtefacts => "Move Artefacts",
                DeploymentStage.StartServices => "Start Services",
                DeploymentStage.CleanArtefacts => "Clean Artefacts",
                _ => stage.ToString()
            };
        }
    }
}
