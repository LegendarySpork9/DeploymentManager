// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Shared;
using DeploymentManager.Values;

namespace DeploymentManager.Services
{
    public class IISService
    {
        private readonly ILoggerService _Logger;
        private readonly IIISClient _IISClient;

        // Sets the class's global variables.
        public IISService(
            ILoggerService _logger,
            IIISClient _iisClient)
        {
            _Logger = _logger;
            _IISClient = _iisClient;
        }

        /// <summary>
        /// Stops the given IIS site.
        /// </summary>
        public async Task<(bool, string?)> StopSite(
            string site,
            string device,
            DeviceAuthModel? auth = null)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Stopping IIS site, {site}");

            bool stopped = false;
            string? message = null;

            try
            {
                message = _IISClient.StopSite(
                    site,
                    device,
                    auth);

                stopped = true;

                if (message != null)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Warning,
                        message);
                }

                else
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Stopped IIS site, {site}");
                }
            }

            catch (Exception ex)
            {
                message = ex.Message;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to stop IIS site, {site}");
            }

            return (stopped, message);
        }

        /// <summary>
        /// Starts the given IIS site.
        /// </summary>
        public async Task<(bool, string?)> StartSite(
            string site,
            string device,
            DeviceAuthModel? auth = null)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Starting IIS site, {site}");

            bool started = false;
            string? errorMessage = null;

            try
            {
                _IISClient.StartSite(
                    site,
                    device,
                    auth);

                started = true;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Started IIS site, {site}");
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
                    $"Failed to start IIS site, {site}");
            }

            return (started, errorMessage);
        }
    }
}
