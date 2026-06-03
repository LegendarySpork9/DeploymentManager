// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using Microsoft.Web.Administration;

namespace DeploymentManager.Implementations
{
    public class IISClientWrapper : IIISClient
    {
        /// <summary>
        /// Stops the given IIS site.
        /// </summary>
        public void StopSite(string site)
        {
            using (ServerManager serverManager = new())
            {
                Site iisSite = serverManager.Sites[site];
                ApplicationPool appPool = serverManager.ApplicationPools[iisSite.Applications[0].ApplicationPoolName];
                iisSite.Stop();
                appPool.Stop();
            } 
        }

        /// <summary>
        /// Starts the given IIS site.
        /// </summary>
        public void StartSite(string site)
        {
            using (ServerManager serverManager = new())
            {
                Site iisSite = serverManager.Sites[site];
                ApplicationPool appPool = serverManager.ApplicationPools[iisSite.Applications[0].ApplicationPoolName];
                appPool.Start();            
                iisSite.Start();
            }
        }
    }
}
