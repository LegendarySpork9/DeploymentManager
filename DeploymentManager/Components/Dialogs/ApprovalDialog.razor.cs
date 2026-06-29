// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models;
using DeploymentManager.Models.Forms;
using DeploymentManager.Services;
using DeploymentManager.Values;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Dialogs
{
    public partial class ApprovalDialog
    {
        [Inject]
        private ILoggerService _Logger { get; set; } = default!;
        [Inject]
        private ApprovalService ApprovalService { get; set; } = default!;

        [Parameter]
        public EventCallback OnApproved { get; set; }
        [Parameter]
        public EventCallback OnCancelled { get; set; }

        private readonly CodeFormModel ApprovalForm = new();

        private bool IsVisible;
        private bool IsLoading;

        private string ErrorMessage = string.Empty;

        /// <summary>
        /// Shows the approval dialog.
        /// </summary>
        public void Show()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Showing Approval Dialog");

            IsVisible = true;
            ErrorMessage = string.Empty;
            ApprovalForm.Code = string.Empty;
            StateHasChanged();
        }

        /// <summary>
        /// Validates the code and fires the approved event.
        /// </summary>
        private async Task HandleApproval()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Approve Clicked");

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                AuthenticatorCredentialModel? credential = await ApprovalService.GetCredential();

                if (credential == null)
                {
                    ErrorMessage = "Authenticator not configured. Please set up your authenticator first.";
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Warning,
                        ErrorMessage);
                }

                else if (ApprovalService.ValidateCode(credential.Secret, ApprovalForm.Code))
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        "Deployment Approved");

                    IsVisible = false;
                    await OnApproved.InvokeAsync();
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
                ErrorMessage = "An error occurred during approval. Please try again.";
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
        /// Cancels the approval and fires the cancelled event.
        /// </summary>
        private async Task HandleCancel()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Approval Cancelled");

            IsVisible = false;
            await OnCancelled.InvokeAsync();
        }
    }
}
