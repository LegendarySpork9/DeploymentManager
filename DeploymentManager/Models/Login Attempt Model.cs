// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models
{
    /// <summary>
    /// Stores the state of login attempts for an IP address.
    /// </summary>
    public class LoginAttemptModel
    {
        public int AttemptCount { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime? LockoutExpiry { get; set; }
    }
}
