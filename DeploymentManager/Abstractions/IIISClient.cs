// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Related;

namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for the IIS operations.
    /// </summary>
    public interface IIISClient
    {
        string? StopSite(string site, string device, DeviceAuthModel? auth = null);
        void StartSite(string site, string device, DeviceAuthModel? auth = null);
    }
}
