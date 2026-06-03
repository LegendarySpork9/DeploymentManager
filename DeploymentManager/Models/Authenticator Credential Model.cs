// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Models
{
    /// <summary>
    /// Stores the authenticator credential data.
    /// </summary>
    public class AuthenticatorCredentialModel
    {
        public required string Secret { get; set; }
        public required DateTime RegisteredDate { get; set; }
    }
}
