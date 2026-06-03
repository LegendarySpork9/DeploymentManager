// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
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
        public async Task<bool> StopSite(string site)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Stopping IIS site, {site}");

            bool stopped = false;

            try
            {
                _IISClient.StopSite(site);

                stopped = true;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Stopped IIS site, {site}");
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to stop IIS site, {site}");
            }

            return stopped;
        }

        /// <summary>
        /// Starts the given IIS site.
        /// </summary>
        public async Task<bool> StartSite(string site)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Starting IIS site, {site}");

            bool started = false;

            try
            {
                _IISClient.StartSite(site);

                started = true;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Started IIS site, {site}");
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to start IIS site, {site}");
            }

            return started;
        }
    }
}
