// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Services;

namespace DeploymentManager.Implementations
{
    public class LoggerServiceWrapper : ILoggerService
    {
        private string IPAddress;

        public LoggerServiceWrapper(string ipAddress)
        {
            IPAddress = ipAddress;
        }

        /// <summary>
        /// Changes the identifier of the logger.
        /// </summary>
        public void ChangeIdentifier(string value) => IPAddress = value;

        /// <summary>
        /// Logs the given message to the log file.
        /// </summary>
        public void LogMessage(
            string level,
            string message,
            string? summary = null)
        {
            LoggerService _logger = new(
                IPAddress,
                "Logs");
            _logger.LogMessage(
                level,
                message,
                summary);
        }
    }
}
