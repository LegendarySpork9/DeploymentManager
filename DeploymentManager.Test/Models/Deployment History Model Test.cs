// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Responses.Related;

namespace DeploymentManager.Test.Models
{
    [TestClass]
    public class DeploymentHistoryModelTest
    {
        private static readonly DateTime DefaultDate = new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        private static DeploymentHistoryModel<object> CreateObjectHistory(
            object artefact,
            ArtefactType artefactType,
            DeploymentType deploymentType)
        {
            return new()
            {
                Id = 1,
                Type = deploymentType,
                ArtefactType = artefactType,
                Status = Status.Completed,
                StartDate = new(2026, 06, 02, 15, 39, 00, DateTimeKind.Utc),
                EndDate = new(2026, 06, 02, 15, 40, 00, DateTimeKind.Utc),
                RunTime = new(00, 01, 00),
                ArtefactId = 1,
                ArtefactName = "test-artefact",
                ArtefactSize = 1048576,
                BranchId = "abc123",
                BranchName = "main",
                DeploymentConfiguration = new()
                {
                    Type = deploymentType,
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
                },
                Stages =
                [
                    new()
                    {
                        Name = DeploymentStage.FetchArtefacts,
                        Status = Status.Completed,
                        StartDate = DefaultDate,
                        EndDate = DefaultDate,
                        RunTime = TimeSpan.Zero
                    }
                ]
            };
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
                Workflow_Run = new()
                {
                    Id = 456,
                    Head_Branch = "main",
                    Head_Sha = "abc123"
                }
            };

            DeploymentHistoryModel<object> history = CreateObjectHistory(
                artefact,
                ArtefactType.Artefact,
                DeploymentType.GitHub);

            DeploymentHistoryModel<ArtefactModel> result = history.ToArtefactDeployment();

            Assert.AreEqual(
                1,
                result.Id);
            Assert.AreEqual(
                DeploymentType.GitHub,
                result.Type);
            Assert.AreEqual(
                ArtefactType.Artefact,
                result.ArtefactType);
            Assert.AreEqual(
                Status.Completed,
                result.Status);
            Assert.AreEqual(
                123,
                result.DeploymentConfiguration.Artefact.Id);
            Assert.AreEqual(
                "test-artefact",
                result.DeploymentConfiguration.Artefact.Name);
            Assert.AreEqual(
                "abc123",
                result.BranchId);
            Assert.HasCount(
                1,
                result.Stages);
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

            DeploymentHistoryModel<object> history = CreateObjectHistory(
                asset,
                ArtefactType.ReleaseAsset,
                DeploymentType.GitHub);

            DeploymentHistoryModel<AssetModel> result = history.ToAssetDeployment();

            Assert.AreEqual(
                1,
                result.Id);
            Assert.AreEqual(
                ArtefactType.ReleaseAsset,
                result.ArtefactType);
            Assert.AreEqual(
                101,
                result.DeploymentConfiguration.Artefact.Id);
            Assert.AreEqual(
                "release.zip",
                result.DeploymentConfiguration.Artefact.Name);
            Assert.AreEqual(
                2097152,
                result.DeploymentConfiguration.Artefact.Size);
            Assert.HasCount(
                1,
                result.Stages);
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

            DeploymentHistoryModel<object> history = CreateObjectHistory(
                upload,
                ArtefactType.Upload,
                DeploymentType.FileUpload);

            DeploymentHistoryModel<UploadFileModel> result = history.ToUploadDeployment();

            Assert.AreEqual(
                1,
                result.Id);
            Assert.AreEqual(
                ArtefactType.Upload,
                result.ArtefactType);
            Assert.AreEqual(
                1,
                result.DeploymentConfiguration.Artefact.Id);
            Assert.AreEqual(
                "test-upload",
                result.DeploymentConfiguration.Artefact.Name);
            Assert.AreEqual(
                @"C:\Uploads",
                result.DeploymentConfiguration.Artefact.Directory);
            Assert.HasCount(
                1,
                result.Stages);
        }

        /// <summary>
        /// Tests whether the ToObjectDeployment method correctly converts the model.
        /// </summary>
        [TestMethod]
        public void TestToObjectDeployment()
        {
            ArtefactModel artefact = new()
            {
                Id = 123,
                Name = "test-artefact",
                Size_in_Bytes = 1048576,
                Archive_Download_Url = "https://example.com/artifact.zip",
                Workflow_Run = new()
                {
                    Id = 456,
                    Head_Branch = "main",
                    Head_Sha = "abc123"
                }
            };

            DeploymentHistoryModel<ArtefactModel> history = new()
            {
                Id = 1,
                Type = DeploymentType.GitHub,
                ArtefactType = ArtefactType.Artefact,
                Status = Status.Completed,
                StartDate = new(2026, 06, 02, 15, 39, 00, DateTimeKind.Utc),
                EndDate = new(2026, 06, 02, 15, 40, 00, DateTimeKind.Utc),
                RunTime = new(00, 01, 00),
                ArtefactId = 1,
                ArtefactName = "test-artefact",
                ArtefactSize = 1048576,
                BranchId = "abc123",
                BranchName = "main",
                DeploymentConfiguration = new()
                {
                    Type = DeploymentType.GitHub,
                    Environment = DeploymentEnvironment.Live,
                    Project = new()
                    {
                        Type = ProjectType.Website,
                        Name = "TestProject",
                        Directory = @"inetpub\wwwroot\TestProject",
                        GitHub = new()
                        {
                            Repository = "test-repo",
                            Artefact = "test-artefact"
                        }
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
                },
                Stages =
                [
                    new()
                    {
                        Name = DeploymentStage.FetchArtefacts,
                        Status = Status.Completed,
                        StartDate = DefaultDate,
                        EndDate = DefaultDate,
                        RunTime = TimeSpan.Zero
                    }
                ]
            };

            DeploymentHistoryModel<object> result = history.ToObjectDeployment();

            Assert.AreEqual(
                1,
                result.Id);
            Assert.AreEqual(
                DeploymentType.GitHub,
                result.Type);
            Assert.AreEqual(
                Status.Completed,
                result.Status);
            Assert.IsInstanceOfType<ArtefactModel>(result.DeploymentConfiguration.Artefact);
            Assert.AreEqual(
                "TestProject",
                result.DeploymentConfiguration.Project.Name);
            Assert.HasCount(
                1,
                result.Stages);
        }
    }
}
