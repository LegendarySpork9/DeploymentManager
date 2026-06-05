// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Models.Related;

namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for Task Scheduler.
    /// </summary>
    public interface ITaskScheduler
    {
        string? StopTask(string taskName, string device, DeviceAuthModel? auth = null);
        void StartTask(string taskName, string device, DeviceAuthModel? auth = null);
    }
}
