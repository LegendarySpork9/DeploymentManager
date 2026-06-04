// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Related;
using DeploymentManager.Values;

namespace DeploymentManager.Services
{
    public class TaskSchedulerService
    {
        private readonly ILoggerService _Logger;
        private readonly ITaskScheduler _TaskScheduler;

        // Sets the class's global variables.
        public TaskSchedulerService(
            ILoggerService _logger,
            ITaskScheduler _taskScheduler)
        {
            _Logger = _logger;
            _TaskScheduler = _taskScheduler;
        }

        /// <summary>
        /// Stops the given task scheduler task.
        /// </summary>
        public async Task<(bool, string?)> StopTask(
            string taskName,
            string device,
            DeviceAuthModel? auth = null)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Stopping Task Scheduler task, {taskName}");

            bool stopped = false;
            string? errorMessage = null;

            try
            {
                _TaskScheduler.StopTask(
                    taskName,
                    device,
                    auth);

                stopped = true;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Stopped Task Scheduler task, {taskName}");
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    errorMessage);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to stop Task Scheduler task, {taskName}");
            }

            return (stopped, errorMessage);
        }

        /// <summary>
        /// Starts the given task scheduler task.
        /// </summary>
        public async Task<(bool, string?)> StartTask(
            string taskName,
            string device,
            DeviceAuthModel? auth = null)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Starting Task Scheduler task, {taskName}");

            bool started = false;
            string? errorMessage = null;

            try
            {
                _TaskScheduler.StartTask(
                    taskName,
                    device,
                    auth);

                started = true;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Started Task Scheduler task, {taskName}");
            }

            catch (Exception ex)
            {
                errorMessage= ex.Message;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    errorMessage);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to start Task Scheduler task, {taskName}");
            }

            return (started, errorMessage);
        }
    }
}
