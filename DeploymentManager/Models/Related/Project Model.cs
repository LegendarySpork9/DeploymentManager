// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Shared;

namespace DeploymentManager.Models.Related
{
    /// <summary>
    /// Stores the configuration for the project.
    /// </summary>
    public class ProjectModel
    {
        public required ProjectType Type { get; set; }
        public required string Name { get; set; }
        public required string Directory { get; set; }
        public required GitHubModel GitHub { get; set; }
        public List<AdditionalDeployModel>? AdditionalDeploy { get; set; }
        public List<IgnoreModel>? Ignore { get; set; }
        public Dictionary<DeploymentEnvironment, ProjectVariablesModel>? Variables { get; set; }

        /// <summary>
        /// Returns the environment-specific service name, or the project name as a fallback.
        /// </summary>
        public string GetServiceName(DeploymentEnvironment environment)
        {
            if (Variables != null && Variables.TryGetValue(
                environment,
                out ProjectVariablesModel? vars) && vars.ServiceName != null)
            {
                return vars.ServiceName;
            }

            return Name;
        }

        /// <summary>
        /// Returns the environment-specific directory, or the project directory as a fallback.
        /// </summary>
        public string GetDirectory(DeploymentEnvironment environment)
        {
            if (Variables != null && Variables.TryGetValue(
                environment,
                out ProjectVariablesModel? vars) && vars.Directory != null)
            {
                return vars.Directory;
            }

            return Directory;
        }
    }
}
