using DeploymentManager.Models.Responses;

namespace DeploymentManager.Abstractions
{
    public interface IGitHubClient
    {
        Task<ArtefactListModel?> GetArtefacts(string repository);
        Task<string> DownloadArtefact(string downloadURL, string downloadPath, string downloadFile);
        Task<List<ReleaseModel>?> GetReleases(string repository);
        Task<string> DownloadReleaseAsset(string downloadURL, string downloadPath, string downloadFile);
    }
}
