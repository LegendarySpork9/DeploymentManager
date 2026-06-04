// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Shared;

namespace DeploymentManager.Test.Models
{
    [TestClass]
    public class DeploymentConfigurationModelTest
    {
        /// <summary>
        /// Tests whether the ToDeploymentModel method returns only the primary deployment when no secondary targets exist.
        /// </summary>
        [TestMethod]
        public void TestToDeploymentModelPrimaryOnly()
        {
            DeploymentConfigurationModel<UploadFileModel> config = new()
            {
                Type = DeploymentType.FileUpload,
                Environment = DeploymentEnvironment.Live,
                Project = new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = @"inetpub\wwwroot\TestProject",
                    GitHub = new() { Repository = "test-repo", Artefact = "test-artefact" }
                },
                Artefact = new()
                {
                    Id = 1,
                    Name = "test-upload",
                    Size = 1024,
                    BranchId = "abc",
                    BranchName = "main",
                    Directory = @"C:\Uploads"
                },
                PrimaryDeploymentTarget = new()
                {
                    Device = "TestDevice",
                    Drive = "C",
                    Name = DeploymentEnvironment.Live,
                    ArtefactSource = ArtefactSource.Actions
                },
                SecondaryDeploymentTargets = null,
                DeploymentSettings = new()
            };

            List<DeploymentModel> result = config.ToDeploymentModel();

            Assert.HasCount(1, result);
            Assert.AreEqual(ArtefactFileDeploymentType.Primary, result[0].ArtefactDeploymentType);
            Assert.IsNull(result[0].Device);
            Assert.AreEqual(@"inetpub\wwwroot\TestProject", result[0].Directory);
            Assert.IsEmpty(result[0].ArtefactFiles);
        }

        /// <summary>
        /// Tests whether the ToDeploymentModel method returns the primary and secondary deployments when secondary targets exist.
        /// </summary>
        [TestMethod]
        public void TestToDeploymentModelWithSecondaryTargets()
        {
            DeploymentConfigurationModel<UploadFileModel> config = new()
            {
                Type = DeploymentType.FileUpload,
                Environment = DeploymentEnvironment.Live,
                Project = new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = @"inetpub\wwwroot\TestProject",
                    GitHub = new() { Repository = "test-repo", Artefact = "test-artefact" }
                },
                Artefact = new()
                {
                    Id = 1,
                    Name = "test-upload",
                    Size = 1024,
                    BranchId = "abc",
                    BranchName = "main",
                    Directory = @"C:\Uploads"
                },
                PrimaryDeploymentTarget = new()
                {
                    Device = "PrimaryDevice",
                    Drive = "C",
                    Name = DeploymentEnvironment.Live,
                    ArtefactSource = ArtefactSource.Actions
                },
                SecondaryDeploymentTargets =
                [
                    new()
                    {
                        Device = "SecondaryDevice",
                        Drive = "D",
                        Directory = @"inetpub\wwwroot\TestProject"
                    }
                ],
                DeploymentSettings = new()
            };

            List<DeploymentModel> result = config.ToDeploymentModel();

            Assert.HasCount(2, result);
            Assert.AreEqual(ArtefactFileDeploymentType.Primary, result[0].ArtefactDeploymentType);
            Assert.IsNull(result[0].Device);
            Assert.AreEqual(ArtefactFileDeploymentType.Secondary, result[1].ArtefactDeploymentType);
            Assert.AreEqual("SecondaryDevice", result[1].Device);
        }
    }
}
