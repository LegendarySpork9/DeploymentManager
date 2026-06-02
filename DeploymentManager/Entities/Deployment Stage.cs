// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Entities
{
    /// <summary>
    /// Stages of Deployment
    /// </summary>
    public enum DeploymentStage
    {
        FetchArtefacts,
        ExtractArtefacts,
        StopService,
        MoveArtefacts,
        StartService,
        CleanArtefacts
    }
}
