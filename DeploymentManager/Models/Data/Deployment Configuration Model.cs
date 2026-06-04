// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Shared;

namespace DeploymentManager.Models.Data
{
    /// <summary>
    /// Stores information about the deployment.
    /// </summary>
    public class DeploymentConfigurationModel<T>
    {
        public required DeploymentType Type { get; set; }
        public required DeploymentEnvironment Environment { get; set; }
        public required ProjectModel Project { get; set; }
        public required T Artefact { get; set; }
        public required EnvironmentModel PrimaryDeploymentTarget { get; set; }
        public List<AdditionalDeployModel>? SecondaryDeploymentTargets { get; set; }
        public required DeploymentSettingsModel DeploymentSettings { get; set; }

        /// <summary>
        /// Converts the model to a list of deployment models.
        /// </summary>
        public List<DeploymentModel> ToDeploymentModel()
        {
            List<DeploymentModel> deploymentFiles = [];

            deploymentFiles.Add(new()
            {
                ArtefactDeploymentType = ArtefactFileDeploymentType.Primary,
                Device = null,
                Directory = Project.Directory,
                ArtefactFiles = []
            });

            if (SecondaryDeploymentTargets != null)
            {
                foreach (AdditionalDeployModel secondaryDeploymentTarget in SecondaryDeploymentTargets)
                {
                    deploymentFiles.Add(new()
                    {
                        ArtefactDeploymentType = ArtefactFileDeploymentType.Secondary,
                        Device = secondaryDeploymentTarget.Device,
                        Directory = secondaryDeploymentTarget.Directory,
                        ArtefactFiles = []
                    });
                }
            }

            return deploymentFiles;
        }
    }
}
