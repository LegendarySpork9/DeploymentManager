// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Models.Shared;
using DeploymentManager.Orchestrators;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Orchestrators
{
    [TestClass]
    public class DeploymentOrchestratorTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IClock> _MockClock = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();

        private static readonly DateTime DefaultDate = new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan DefaultTimeSpan = TimeSpan.Zero;

        private readonly AppSettingsModel AppSettings = new()
        {
            SiteAuth = string.Empty,
            DeploymentHistoryLocation = @"C:\DeployHistory",
            ApprovalCredentialLocation = string.Empty,
            ArtefactDownloadLocation = @"C:\Deploy",
            Environments =
            [
                new()
                {
                    Device = "TestDevice",
                    Drive = "C",
                    Name = DeploymentEnvironment.Live,
                    ArtefactSource = ArtefactSource.Actions
                }
            ],
            GitHubOptions = new() { Auth = string.Empty, Owner = string.Empty },
            Projects =
            [
                new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = @"inetpub\wwwroot\TestProject",
                    GitHub = new() { Repository = "test-repo", Artefact = "test-artefact" }
                }
            ]
        };

        /// <summary>
        /// Tests whether the SetUp method creates 7 stages for an ArtefactModel deployment.
        /// </summary>
        [TestMethod]
        public async Task TestSetUpWithArtefactModel()
        {
            _MockClock.Setup(c => c.DefaultDate).Returns(DefaultDate);
            _MockClock.Setup(c => c.DefaultTimeSpan).Returns(DefaultTimeSpan);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).ReturnsAsync(string.Empty);

            DeploymentHistoryService deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            DeploymentOrchestrator orchestrator = new(
                _MockLogger.Object,
                _MockClock.Object,
                Mock.Of<IServiceProvider>(),
                deploymentHistoryService,
                AppSettings);

            DeploymentConfigurationModel<ArtefactModel> config = new()
            {
                Type = DeploymentType.GitHub,
                Environment = DeploymentEnvironment.Live,
                Project = AppSettings.Projects[0],
                Artefact = new()
                {
                    Id = 123,
                    Name = "test-artefact",
                    Size_in_Bytes = 1048576,
                    Archive_Download_Url = "https://example.com/artifact.zip",
                    Workflow_Run = new() { Id = 456, Head_Branch = "main", Head_Sha = "abc123" }
                },
                PrimaryDeploymentTarget = AppSettings.Environments[0],
                DeploymentSettings = new()
            };

            DeploymentHistoryModel<ArtefactModel> result = await orchestrator.SetUp(config);

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(ArtefactType.Artefact, result.ArtefactType);
            Assert.AreEqual(Status.PendingApproval, result.Status);
            Assert.HasCount(7, result.Stages);
            Assert.AreEqual(DeploymentStage.FetchArtefacts, result.Stages[0].Name);
            Assert.AreEqual(DeploymentStage.ExtractArtefacts, result.Stages[1].Name);
            Assert.AreEqual(DeploymentStage.FetchArtefactFiles, result.Stages[2].Name);
            Assert.AreEqual(DeploymentStage.StopServices, result.Stages[3].Name);
            Assert.AreEqual(DeploymentStage.MoveArtefacts, result.Stages[4].Name);
            Assert.AreEqual(DeploymentStage.StartServices, result.Stages[5].Name);
            Assert.AreEqual(DeploymentStage.CleanArtefacts, result.Stages[6].Name);
            Assert.AreEqual("abc123", result.BranchId);
            Assert.AreEqual("main", result.BranchName);
        }

        /// <summary>
        /// Tests whether the SetUp method creates 7 stages for an AssetModel deployment.
        /// </summary>
        [TestMethod]
        public async Task TestSetUpWithAssetModel()
        {
            _MockClock.Setup(c => c.DefaultDate).Returns(DefaultDate);
            _MockClock.Setup(c => c.DefaultTimeSpan).Returns(DefaultTimeSpan);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).ReturnsAsync(string.Empty);

            DeploymentHistoryService deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            DeploymentOrchestrator orchestrator = new(
                _MockLogger.Object,
                _MockClock.Object,
                Mock.Of<IServiceProvider>(),
                deploymentHistoryService,
                AppSettings);

            DeploymentConfigurationModel<AssetModel> config = new()
            {
                Type = DeploymentType.GitHub,
                Environment = DeploymentEnvironment.Live,
                Project = AppSettings.Projects[0],
                Artefact = new()
                {
                    Id = 101,
                    Name = "release.zip",
                    Size = 2097152,
                    Content_Type = "application/zip",
                    Browser_Download_Url = "https://example.com/release.zip"
                },
                PrimaryDeploymentTarget = AppSettings.Environments[0],
                DeploymentSettings = new()
            };

            DeploymentHistoryModel<AssetModel> result = await orchestrator.SetUp(config);

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(ArtefactType.ReleaseAsset, result.ArtefactType);
            Assert.HasCount(7, result.Stages);
            Assert.AreEqual(DeploymentStage.FetchArtefacts, result.Stages[0].Name);
            Assert.AreEqual("main", result.BranchId);
            Assert.AreEqual("main", result.BranchName);
        }

        /// <summary>
        /// Tests whether the SetUp method creates 6 stages for an UploadFileModel deployment.
        /// </summary>
        [TestMethod]
        public async Task TestSetUpWithUploadFileModel()
        {
            _MockClock.Setup(c => c.DefaultDate).Returns(DefaultDate);
            _MockClock.Setup(c => c.DefaultTimeSpan).Returns(DefaultTimeSpan);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).ReturnsAsync(string.Empty);

            DeploymentHistoryService deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            DeploymentOrchestrator orchestrator = new(
                _MockLogger.Object,
                _MockClock.Object,
                Mock.Of<IServiceProvider>(),
                deploymentHistoryService,
                AppSettings);

            DeploymentConfigurationModel<UploadFileModel> config = new()
            {
                Type = DeploymentType.FileUpload,
                Environment = DeploymentEnvironment.Live,
                Project = AppSettings.Projects[0],
                Artefact = new()
                {
                    Id = 1,
                    Name = "test-upload",
                    Size = 1024,
                    BranchId = "upload-abc",
                    BranchName = "feature/upload",
                    Directory = @"C:\Uploads"
                },
                PrimaryDeploymentTarget = AppSettings.Environments[0],
                DeploymentSettings = new()
            };

            DeploymentHistoryModel<UploadFileModel> result = await orchestrator.SetUp(config);

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(ArtefactType.Upload, result.ArtefactType);
            Assert.HasCount(6, result.Stages);
            Assert.AreEqual(DeploymentStage.ExtractArtefacts, result.Stages[0].Name);
            Assert.AreEqual(DeploymentStage.FetchArtefactFiles, result.Stages[1].Name);
            Assert.AreEqual(DeploymentStage.StopServices, result.Stages[2].Name);
            Assert.AreEqual(DeploymentStage.MoveArtefacts, result.Stages[3].Name);
            Assert.AreEqual(DeploymentStage.StartServices, result.Stages[4].Name);
            Assert.AreEqual(DeploymentStage.CleanArtefacts, result.Stages[5].Name);
            Assert.AreEqual("upload-abc", result.BranchId);
            Assert.AreEqual("feature/upload", result.BranchName);
        }
    }
}
