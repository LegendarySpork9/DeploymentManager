// Copyright © - Unpublished - Toby Hunter
using System.Collections.Concurrent;
using DeploymentManager.Abstractions;
using DeploymentManager.Models;

namespace DeploymentManager.Implementations
{
    public class LoginRateLimiterWrapper : ILoginRateLimiter
    {
        private readonly ConcurrentDictionary<string, LoginAttemptModel> _attempts = new();
        private readonly IClock _clock;
        private readonly AppSettingsModel _appSettings;

        private const int DefaultMaxAttempts = 5;
        private const int DefaultAttemptWindowMinutes = 5;
        private const int DefaultLockoutDurationMinutes = 15;

        // Sets the class's global variables.
        public LoginRateLimiterWrapper(
            IClock clock,
            AppSettingsModel appSettings)
        {
            _clock = clock;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Checks whether the given IP address is currently locked out.
        /// </summary>
        public (bool IsLockedOut, TimeSpan RemainingTime) CheckLockout(string ipAddress)
        {
            bool isLockedOut = false;
            TimeSpan remainingTime = TimeSpan.Zero;

            if (!string.IsNullOrWhiteSpace(ipAddress) && _attempts.TryGetValue(
                ipAddress,
                out LoginAttemptModel attempt))
            {
                if (attempt.LockoutExpiry.HasValue && attempt.LockoutExpiry.Value > _clock.UtcNow)
                {
                    isLockedOut = true;
                    remainingTime = attempt.LockoutExpiry.Value - _clock.UtcNow;
                }

                else if (attempt.LockoutExpiry.HasValue && attempt.LockoutExpiry.Value <= _clock.UtcNow)
                {
                    _attempts.TryRemove(
                        ipAddress,
                        out _);
                }
            }

            return (
                isLockedOut,
                remainingTime);
        }

        /// <summary>
        /// Records a failed login attempt for the given IP address.
        /// </summary>
        public void RecordFailedAttempt(string ipAddress)
        {
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                int maxAttempts = _appSettings.MaxLoginAttempts > 0 ? _appSettings.MaxLoginAttempts : DefaultMaxAttempts;
                int windowMinutes = _appSettings.LoginAttemptWindowMinutes > 0 ? _appSettings.LoginAttemptWindowMinutes : DefaultAttemptWindowMinutes;
                int lockoutMinutes = _appSettings.LockoutDurationMinutes > 0 ? _appSettings.LockoutDurationMinutes : DefaultLockoutDurationMinutes;

                _attempts.AddOrUpdate(
                    ipAddress,
                    _ => new LoginAttemptModel
                    {
                        AttemptCount = 1,
                        WindowStart = _clock.UtcNow
                    },
                    (_, existing) =>
                    {
                        if (existing.WindowStart.AddMinutes(windowMinutes) < _clock.UtcNow)
                        {
                            existing.AttemptCount = 1;
                            existing.WindowStart = _clock.UtcNow;
                            existing.LockoutExpiry = null;
                        }

                        else
                        {
                            existing.AttemptCount++;
                        }

                        if (existing.AttemptCount >= maxAttempts)
                        {
                            existing.LockoutExpiry = _clock.UtcNow.AddMinutes(lockoutMinutes);
                        }

                        return existing;
                    });
            }
        }

        /// <summary>
        /// Resets the attempt counter for the given IP address.
        /// </summary>
        public void ResetAttempts(string ipAddress)
        {
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                _attempts.TryRemove(
                    ipAddress,
                    out _);
            }
        }
    }
}
