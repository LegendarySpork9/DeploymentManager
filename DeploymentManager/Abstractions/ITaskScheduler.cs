// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Shared;

namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for Task Scheduler operations.
    /// </summary>
    public interface ITaskScheduler
    {
        string? StopTask(string taskName, string device, DeviceAuthModel? auth = null);
        void StartTask(string taskName, string device, DeviceAuthModel? auth = null);
    }
}
