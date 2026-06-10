// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models.Shared
{
    /// <summary>
    /// Stores the settings for connecting to another device.
    /// </summary>
    public class DeviceAuthModel
    {
        public string? Domain { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
