// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for the login rate limiter.
    /// </summary>
    public interface ILoginRateLimiter
    {
        (bool IsLockedOut, TimeSpan RemainingTime) CheckLockout(string ipAddress);
        void RecordFailedAttempt(string ipAddress);
        void ResetAttempts(string ipAddress);
    }
}
