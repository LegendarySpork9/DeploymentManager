// Copyright © - Unpublished - Toby Hunter
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace DeploymentManager.Components.Layout
{
    public partial class MainLayout
    {
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        [Inject]
        private ProtectedSessionStorage SessionStorage { get; set; } = default!;

        private bool IsLoggedIn;
        private bool IsInitialised;

        /// <summary>
        /// Checks if the user is logged in.
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    ProtectedBrowserStorageResult<bool> loggedInResult = await SessionStorage.GetAsync<bool>("userLoggedIn");

                    if (loggedInResult.Success && (loggedInResult.Value))
                    {
                        IsInitialised = true;
                        IsLoggedIn = true;
                        StateHasChanged();
                    }

                    else
                    {
                        Navigation.NavigateTo(
                            "/login",
                            forceLoad: true);
                    }
                }

                catch
                {
                    Navigation.NavigateTo(
                        "/login",
                        forceLoad: true);
                }
            }
        }

        /// <summary>
        /// Performs the logout steps.
        /// </summary>
        private async Task SignOut()
        {
            await SessionStorage.DeleteAsync("userLoggedIn");

            IsLoggedIn = false;

            Navigation.NavigateTo(
                "/login",
                forceLoad: true);
        }
    }
}
