// Copyright © - Unpublished - Toby Hunter
using System.Runtime.InteropServices;
using System.Security.Principal;
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Shared;
using Microsoft.Web.Administration;

namespace DeploymentManager.Implementations
{
    public class IISClientWrapper : IIISClient
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string? lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private const int LOGON32_PROVIDER_WINNT50 = 3;

        /// <summary>
        /// Stops the given IIS site.
        /// </summary>
        public string? StopSite(
            string site,
            string device,
            DeviceAuthModel? auth = null)
        {
            string? warning = null;

            RunWithOptionalImpersonation(auth, () =>
            {
                using (ServerManager serverManager = ConnectToServer(device))
                {
                    Site iisSite = serverManager.Sites[site];
                    ApplicationPool appPool = serverManager.ApplicationPools[iisSite.Applications[0].ApplicationPoolName];

                    if (iisSite.State == ObjectState.Stopped && appPool.State == ObjectState.Stopped)
                    {
                        warning = $"IIS site '{site}' was already stopped";
                        return;
                    }

                    if (iisSite.State != ObjectState.Stopped)
                    {
                        iisSite.Stop();
                    }

                    if (appPool.State != ObjectState.Stopped)
                    {
                        appPool.Stop();
                    }
                }
            });

            return warning;
        }

        /// <summary>
        /// Starts the given IIS site.
        /// </summary>
        public void StartSite(
            string site,
            string device,
            DeviceAuthModel? auth = null)
        {
            RunWithOptionalImpersonation(auth, () =>
            {
                using (ServerManager serverManager = ConnectToServer(device))
                {
                    Site iisSite = serverManager.Sites[site];
                    ApplicationPool appPool = serverManager.ApplicationPools[iisSite.Applications[0].ApplicationPoolName];
                    appPool.Start();
                    iisSite.Start();
                }
            });
        }

        /// <summary>
        /// Connects to IIS on the given device, using a local connection when the device is the current machine.
        /// </summary>
        private static ServerManager ConnectToServer(string device)
        {
            if (string.Equals(device, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                return new ServerManager();
            }

            return ServerManager.OpenRemote(device);
        }

        /// <summary>
        /// Impersonates a user on another machine with the given details.
        /// </summary>
        private void RunWithOptionalImpersonation(
            DeviceAuthModel? auth,
            Action action)
        {
            if (auth == null)
            {
                action();
                return;
            }

            if (!LogonUser(
                auth.Username,
                auth.Domain,
                auth.Password,
                LOGON32_LOGON_NEW_CREDENTIALS,
                LOGON32_PROVIDER_WINNT50,
                out IntPtr token))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                using (WindowsIdentity identity = new(token))
                {
                    WindowsIdentity.RunImpersonated(
                        identity.AccessToken,
                        action);
                }
            }
            finally
            {
                CloseHandle(token);
            }
        }
    }
}
