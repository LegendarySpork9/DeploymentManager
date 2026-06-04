// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;

namespace DeploymentManager.Implementations
{
    public class SystemClockProvider : IClock
    {
        /// <summary>
        /// Returns the current UTC Date and time.
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Returns the default date and time.
        /// </summary>
        public DateTime DefaultDate => new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns the default time span.
        /// </summary>
        public TimeSpan DefaultTimeSpan => new(0, 0, 0, 0, 0, 0);
    }
}
