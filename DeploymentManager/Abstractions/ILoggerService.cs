// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for the logger service.
    /// </summary>
    public interface ILoggerService
    {
        void ChangeIdentifier(string value);
        void LogMessage(string level, string message, string? summary = null);
    }
}
