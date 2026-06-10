// Copyright © - Unpublished - Toby Hunter
using System.Text;
using DeploymentManager.Abstractions;
using DeploymentManager.Functions;
using DeploymentManager.Models;
using DeploymentManager.Models.Forms;
using DeploymentManager.Values;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace DeploymentManager.Components.Pages
{
    public partial class Login
    {
        [Inject]
        private ILoggerService _Logger { get; set; } = default!;
        [Inject]
        private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        [Inject]
        private ProtectedSessionStorage SessionStorage { get; set; } = default!;
        [Inject]
        private AppSettingsModel AppSettings { get; set; } = default!;

        private readonly LoginFormModel LoginInformation = new();

        private bool IsLoading;

        private string ErrorMessage = string.Empty;

        /// <summary>
        /// Captures the user IP for logging.
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _Logger.ChangeIdentifier(IPAddressFunction.FetchIpAddress(HttpContextAccessor));
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Opened Login Page");
            }
        }

        /// <summary>
        /// Performs the login steps.
        /// </summary>
        private async Task HandleLogin()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Login Clicked");

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                string credentialsToEncode = $"{LoginInformation.Username}:{HashFunction.HashString(LoginInformation.Password)}";
                byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentialsToEncode);
                string encodedCredentials = Convert.ToBase64String(credentialsBytes);

                if (encodedCredentials == AppSettings.SiteAuth)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        "Login Successful");
                    _Logger.ChangeIdentifier($"{LoginInformation.Username} ({IPAddressFunction.FetchIpAddress(HttpContextAccessor)})");

                    await SessionStorage.SetAsync(
                        "userLoggedIn",
                        true);

                    Navigation.NavigateTo("/");
                }

                else
                {
                    ErrorMessage = "Invalid credentials. Please check your username and password.";
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Warning,
                        ErrorMessage);
                }
            }

            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during authentication. Please try again.";
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            IsLoading = false;
        }
    }
}
