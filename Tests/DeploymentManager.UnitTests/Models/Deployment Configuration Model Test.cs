// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Responses.Related;

namespace DeploymentManager.UnitTests.Models
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

            Assert.HasCount(
                1,
                result);
            Assert.AreEqual(
                ArtefactFileDeploymentType.Primary,
                result[0].ArtefactDeploymentType);
            Assert.IsNull(result[0].Device);
            Assert.AreEqual(
                @"inetpub\wwwroot\TestProject",
                result[0].Directory);
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

            Assert.HasCount(
                2,
                result);
            Assert.AreEqual(
                ArtefactFileDeploymentType.Primary,
                result[0].ArtefactDeploymentType);
            Assert.IsNull(result[0].Device);
            Assert.AreEqual(
                ArtefactFileDeploymentType.Secondary,
                result[1].ArtefactDeploymentType);
            Assert.AreEqual(
                "SecondaryDevice",
                result[1].Device);
        }

        /// <summary>
        /// Tests whether the ToArtefactDeployment method correctly converts the model.
        /// </summary>
        [TestMethod]
        public void TestToArtefactDeployment()
        {
            ArtefactModel artefact = new()
            {
                Id = 123,
                Name = "test-artefact",
                Size_in_Bytes = 1048576,
                Archive_Download_Url = "https://example.com/artifact.zip",
                Workflow_Run = new() { Id = 456, Head_Branch = "main", Head_Sha = "abc123" }
            };

            DeploymentConfigurationModel<object> config = new()
            {
                Type = DeploymentType.GitHub,
                Environment = DeploymentEnvironment.Live,
                Project = new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = @"inetpub\wwwroot\TestProject",
                    GitHub = new() { Repository = "test-repo", Artefact = "test-artefact" }
                },
                Artefact = artefact,
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

            DeploymentConfigurationModel<ArtefactModel> result = config.ToArtefactDeployment();

            Assert.AreEqual(
                DeploymentType.GitHub,
                result.Type);
            Assert.AreEqual(
                DeploymentEnvironment.Live,
                result.Environment);
            Assert.AreEqual(
                "TestProject",
                result.Project.Name);
            Assert.AreEqual(
                123,
                result.Artefact.Id);
            Assert.AreEqual(
                "test-artefact",
                result.Artefact.Name);
            Assert.AreEqual(
                "TestDevice",
                result.PrimaryDeploymentTarget.Device);
            Assert.IsNull(result.SecondaryDeploymentTargets);
        }

        /// <summary>
        /// Tests whether the ToAssetDeployment method correctly converts the model.
        /// </summary>
        [TestMethod]
        public void TestToAssetDeployment()
        {
            AssetModel asset = new()
            {
                Id = 101,
                Name = "release.zip",
                Size = 2097152,
                Content_Type = "application/zip",
                Browser_Download_Url = "https://example.com/release.zip"
            };

            DeploymentConfigurationModel<object> config = new()
            {
                Type = DeploymentType.GitHub,
                Environment = DeploymentEnvironment.Live,
                Project = new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = @"inetpub\wwwroot\TestProject",
                    GitHub = new() { Repository = "test-repo", Artefact = "test-artefact" }
                },
                Artefact = asset,
                PrimaryDeploymentTarget = new()
                {
                    Device = "TestDevice",
                    Drive = "C",
                    Name = DeploymentEnvironment.Live,
                    ArtefactSource = ArtefactSource.Releases
                },
                SecondaryDeploymentTargets = null,
                DeploymentSettings = new()
            };

            DeploymentConfigurationModel<AssetModel> result = config.ToAssetDeployment();

            Assert.AreEqual(
                DeploymentType.GitHub,
                result.Type);
            Assert.AreEqual(
                101,
                result.Artefact.Id);
            Assert.AreEqual(
                "release.zip",
                result.Artefact.Name);
            Assert.AreEqual(
                2097152,
                result.Artefact.Size);
        }

        /// <summary>
        /// Tests whether the ToUploadDeployment method correctly converts the model.
        /// </summary>
        [TestMethod]
        public void TestToUploadDeployment()
        {
            UploadFileModel upload = new()
            {
                Id = 1,
                Name = "test-upload",
                Size = 1024,
                BranchId = "abc",
                BranchName = "main",
                Directory = @"C:\Uploads"
            };

            DeploymentConfigurationModel<object> config = new()
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
                Artefact = upload,
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

            DeploymentConfigurationModel<UploadFileModel> result = config.ToUploadDeployment();

            Assert.AreEqual(
                DeploymentType.FileUpload,
                result.Type);
            Assert.AreEqual(
                1,
                result.Artefact.Id);
            Assert.AreEqual(
                "test-upload",
                result.Artefact.Name);
            Assert.AreEqual(
                @"C:\Uploads",
                result.Artefact.Directory);
        }

        /// <summary>
        /// Tests whether the ToObjectDeployment method correctly converts the model.
        /// </summary>
        [TestMethod]
        public void TestToObjectDeployment()
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

            DeploymentConfigurationModel<object> result = config.ToObjectDeployment();

            Assert.AreEqual(
                DeploymentType.FileUpload,
                result.Type);
            Assert.AreEqual(
                DeploymentEnvironment.Live,
                result.Environment);
            Assert.AreEqual(
                "TestProject",
                result.Project.Name);
            Assert.IsInstanceOfType<UploadFileModel>(result.Artefact);
            Assert.AreEqual(
                "TestDevice",
                result.PrimaryDeploymentTarget.Device);
        }
    }
}
