// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for HTTP file download operations.
    /// </summary>
    public interface IHttpDownloadClient
    {
        Task<Stream?> DownloadStreamAsync(string url, string bearerToken);
        Task<Stream?> DownloadStreamAsync(string url);
    }
}
