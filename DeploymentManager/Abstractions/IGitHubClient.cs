using DeploymentManager.Models.Responses;

namespace DeploymentManager.Abstractions
{
    public interface IGitHubClient
    {
        Task<ArtefactListModel?> GetArtefacts(string repository);
        Task<(string, string?)> DownloadArtefact(string downloadURL, string downloadPath, string downloadFile);
        Task<List<ReleaseModel>?> GetReleases(string repository);
        Task<(string, string?)> DownloadReleaseAsset(string downloadURL, string downloadPath, string downloadFile);
    }
}
