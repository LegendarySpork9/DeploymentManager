// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Related;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace DeploymentManager.Implementations
{
    public class TaskSchedulerWrapper : ITaskScheduler
    {
        /// <summary>
        /// Stops the given task.
        /// </summary>
        public string? StopTask(
            string taskName,
            string device,
            DeviceAuthModel? auth = null)
        {
            using (TaskService taskService = new(device, auth?.Username, auth?.Domain, auth?.Password))
            {
                Task task = taskService.GetTask(taskName);

                if (!task.Definition.Settings.Enabled && task.State == TaskState.Disabled)
                {
                    return $"Task '{taskName}' was already stopped";
                }

                if (task.State == TaskState.Running)
                {
                    task.Stop();
                }

                task.Definition.Settings.Enabled = false;
                task.RegisterChanges();
            }

            return null;
        }

        /// <summary>
        /// Starts the given task.
        /// </summary>
        public void StartTask(
            string taskName,
            string device,
            DeviceAuthModel? auth = null)
        {
            using (TaskService taskService = new(device, auth?.Username, auth?.Domain, auth?.Password))
            {
                Task task = taskService.GetTask(taskName);
                task.Definition.Settings.Enabled = true;
                task.RegisterChanges();
                task.Run();
            }
        }
    }
}
