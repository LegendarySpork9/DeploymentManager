// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models;
using DeploymentManager.Models.Forms;
using DeploymentManager.Services;
using DeploymentManager.Values;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Pages
{
    public partial class AuthenticatorSetup
    {
        [Inject]
        private ILoggerService _Logger { get; set; } = default!;
        [Inject]
        private ApprovalService ApprovalService { get; set; } = default!;

        private readonly CodeFormModel VerificationForm = new();

        private bool? IsAlreadySetup = null;
        private bool IsLoading;
        private bool IsReconfiguring;
        private bool IsVerified;

        private string ErrorMessage = string.Empty;

        private string Secret = string.Empty;
        private string QRCodeBase64 = string.Empty;

        /// <summary>
        /// Checks if the authenticator is already set up.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Opened Authenticator Setup Page");

            IsLoading = true;
            IsAlreadySetup = await ApprovalService.IsSetupComplete();
            IsLoading = false;
        }

        /// <summary>
        /// Generates a new secret and QR code.
        /// </summary>
        private void GenerateSetup()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generating authenticator setup");

            IsLoading = true;

            Secret = ApprovalService.GenerateSecret();
            string uri = ApprovalService.GenerateQRCodeURL(Secret);
            QRCodeBase64 = ApprovalService.GenerateQRCodeBase64(uri);

            IsLoading = false;

            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generated authenticator setup");
        }

        /// <summary>
        /// Verifies the code and saves the credential.
        /// </summary>
        private async Task HandleVerification()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Verify Clicked");

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (ApprovalService.ValidateCode(
                    Secret,
                    VerificationForm.Code))
                {
                    AuthenticatorCredentialModel credential = new()
                    {
                        Secret = Secret,
                        RegisteredDate = DateTime.UtcNow
                    };

                    await ApprovalService.SaveCredential(credential);

                    IsVerified = true;
                    IsAlreadySetup = true;
                    IsReconfiguring = false;

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        "Authenticator setup verified and saved");
                }

                else
                {
                    ErrorMessage = "Invalid code. Please check the code in your authenticator app and try again.";
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Warning,
                        ErrorMessage);
                }
            }

            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during verification. Please try again.";
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            IsLoading = false;
        }

        /// <summary>
        /// Starts the reconfiguration process.
        /// </summary>
        private void StartReconfigure()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Reconfiguring authenticator");

            IsReconfiguring = true;
            QRCodeBase64 = string.Empty;
            Secret = string.Empty;
            IsVerified = false;
            VerificationForm.Code = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}
