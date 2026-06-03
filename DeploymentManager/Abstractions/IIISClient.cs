// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for IIS.
    /// </summary>
    public interface IIISClient
    {
        void StopSite(string site);
        void StartSite(string site);
    }
}
