// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Models.Shared;
using DeploymentManager.Orchestrators.GitHub;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.IntegrationTests.Orchestrators
{
    [TestClass]
    public class DeployReleasesOrchestratorTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IClock> _MockClock = new();
        private readonly Mock<IGitHubClient> _MockGitHubClient = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();
        private readonly Mock<IIISClient> _MockIISClient = new();
        private readonly Mock<ITaskScheduler> _MockTaskScheduler = new();

        private static readonly DateTime TestDate = new(2026, 06, 04, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DefaultDate = new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        private DeployReleasesOrchestrator CreateOrchestrator()
        {
            _MockClock.Setup(c => c.UtcNow)
                .Returns(TestDate);
            _MockClock.Setup(c => c.DefaultDate)
                .Returns(DefaultDate);
            _MockClock.Setup(c => c.DefaultTimeSpan)
                .Returns(TimeSpan.Zero);

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);
            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);
            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);
            TaskSchedulerService taskSchedulerService = new(
                _MockLogger.Object,
                _MockTaskScheduler.Object);

            return new DeployReleasesOrchestrator(
                _MockLogger.Object,
                _MockClock.Object,
                gitHubService,
                documentService,
                iisService,
                taskSchedulerService);
        }

        private static DeploymentConfigurationModel<AssetModel> CreateConfig()
        {
            return new()
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
                Artefact = new()
                {
                    Id = 101,
                    Name = "release.zip",
                    Size = 2097152,
                    Content_Type = "application/zip",
                    Browser_Download_Url = "https://github.com/owner/repo/releases/download/v1.0.0/release.zip"
                },
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
        }

        private static DeploymentHistoryModel<AssetModel> CreateDeploymentHistory(DeploymentConfigurationModel<AssetModel> config)
        {
            DateTime defaultDate = new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            return new()
            {
                Id = 1,
                Type = DeploymentType.GitHub,
                ArtefactType = ArtefactType.ReleaseAsset,
                Status = Status.PendingApproval,
                StartDate = defaultDate,
                EndDate = defaultDate,
                RunTime = TimeSpan.Zero,
                ArtefactId = 101,
                ArtefactName = "release.zip",
                ArtefactSize = 2097152,
                BranchId = "main",
                BranchName = "main",
                DeploymentConfiguration = config,
                Stages =
                [
                    new() { Name = DeploymentStage.FetchArtefacts, Status = Status.NotStarted, StartDate = defaultDate, EndDate = defaultDate, RunTime = TimeSpan.Zero },
                    new() { Name = DeploymentStage.ExtractArtefacts, Status = Status.NotStarted, StartDate = defaultDate, EndDate = defaultDate, RunTime = TimeSpan.Zero },
                    new() { Name = DeploymentStage.FetchArtefactFiles, Status = Status.NotStarted, StartDate = defaultDate, EndDate = defaultDate, RunTime = TimeSpan.Zero },
                    new() { Name = DeploymentStage.StopServices, Status = Status.NotStarted, StartDate = defaultDate, EndDate = defaultDate, RunTime = TimeSpan.Zero },
                    new() { Name = DeploymentStage.MoveArtefacts, Status = Status.NotStarted, StartDate = defaultDate, EndDate = defaultDate, RunTime = TimeSpan.Zero },
                    new() { Name = DeploymentStage.StartServices, Status = Status.NotStarted, StartDate = defaultDate, EndDate = defaultDate, RunTime = TimeSpan.Zero },
                    new() { Name = DeploymentStage.CleanArtefacts, Status = Status.NotStarted, StartDate = defaultDate, EndDate = defaultDate, RunTime = TimeSpan.Zero },
                ]
            };
        }

        /// <summary>
        /// Tests whether the Run method completes all stages successfully for a GitHub Releases deployment.
        /// </summary>
        [TestMethod]
        public async Task TestRunAllStagesComplete()
        {
            DeployReleasesOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<AssetModel> config = CreateConfig();
            DeploymentHistoryModel<AssetModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadReleaseAsset(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((
                    @"C:\Deploy\release.zip",
                    (string?)null));

            FileStream fileStream = new(
                Path.GetTempFileName(),
                FileMode.Open,
                FileAccess.Read,
                FileShare.None,
                4096,
                FileOptions.DeleteOnClose);

            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>()))
                .ReturnsAsync(fileStream);
            _MockFileSystem.Setup(fs => fs.ExtractArchive(
                    It.IsAny<string>(),
                    It.IsAny<FileStream>()))
                .Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>()))
                .ReturnsAsync([@"C:\Deploy\release.zip\file1.dll"]);
            _MockIISClient.Setup(iis => iis.StopSite(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DeviceAuthModel?>()))
                .Returns((string?)null);
            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _MockIISClient.Setup(iis => iis.StartSite(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DeviceAuthModel?>()));
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            DeploymentHistoryModel<AssetModel> result = await orchestrator.Run(
                deployment,
                @"C:\Deploy",
                config);

            Assert.AreEqual(
                Status.Completed,
                result.Status);
            Assert.IsNull(result.FailedAtStage);
            Assert.IsTrue(result.Stages.All(s => s.Status == Status.Completed));
        }

        /// <summary>
        /// Tests whether the Run method fails when the fetch artefacts stage fails.
        /// </summary>
        [TestMethod]
        public async Task TestRunFetchArtefactsFails()
        {
            DeployReleasesOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<AssetModel> config = CreateConfig();
            DeploymentHistoryModel<AssetModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadReleaseAsset(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((
                    string.Empty,
                    (string?)"Download failed"));

            DeploymentHistoryModel<AssetModel> result = await orchestrator.Run(
                deployment,
                @"C:\Deploy",
                config);

            Assert.AreEqual(
                Status.Failed,
                result.Status);
            Assert.AreEqual(
                DeploymentStage.FetchArtefacts,
                result.FailedAtStage);
            Assert.AreEqual(
                Status.Failed,
                result.Stages[0].Status);
            Assert.IsNotNull(result.Stages[0].FailMessages);
        }

        /// <summary>
        /// Tests whether the Run method skips the start services stage when RestartService is false.
        /// </summary>
        [TestMethod]
        public async Task TestRunSkipsStartServicesWhenNotRestarting()
        {
            DeployReleasesOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<AssetModel> config = CreateConfig();
            config.DeploymentSettings = new()
            {
                RestartService = false
            };
            DeploymentHistoryModel<AssetModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadReleaseAsset(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((
                    @"C:\Deploy\release.zip",
                    (string?)null));

            FileStream fileStream = new(
                Path.GetTempFileName(),
                FileMode.Open,
                FileAccess.Read,
                FileShare.None,
                096,
                FileOptions.DeleteOnClose);

            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>()))
                .ReturnsAsync(fileStream);
            _MockFileSystem.Setup(fs => fs.ExtractArchive(
                    It.IsAny<string>(),
                    It.IsAny<FileStream>()))
                .Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>()))
                .ReturnsAsync([@"C:\Deploy\release.zip\file1.dll"]);
            _MockIISClient.Setup(iis => iis.StopSite(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DeviceAuthModel?>()))
                .Returns((string?)null);
            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            DeploymentHistoryModel<AssetModel> result = await orchestrator.Run(
                deployment,
                @"C:\Deploy",
                config);

            Assert.AreEqual(
                Status.Completed,
                result.Status);
            Assert.AreEqual(
                Status.Skipped,
                result.Stages[5].Status);
        }

        /// <summary>
        /// Tests whether the Run method completes with warnings when the stop services stage returns a warning.
        /// </summary>
        [TestMethod]
        public async Task TestRunCompletesWithWarningsWhenStopServiceWarns()
        {
            DeployReleasesOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<AssetModel> config = CreateConfig();
            DeploymentHistoryModel<AssetModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadReleaseAsset(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((
                    @"C:\Deploy\release.zip",
                    (string?)null));

            FileStream fileStream = new(
                Path.GetTempFileName(),
                FileMode.Open,
                FileAccess.Read,
                FileShare.None,
                4096,
                FileOptions.DeleteOnClose);

            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>()))
                .ReturnsAsync(fileStream);
            _MockFileSystem.Setup(fs => fs.ExtractArchive(
                    It.IsAny<string>(),
                    It.IsAny<FileStream>()))
                .Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>()))
                .ReturnsAsync([@"C:\Deploy\release.zip\file1.dll"]);
            _MockIISClient.Setup(iis => iis.StopSite(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DeviceAuthModel?>()))
                .Returns("IIS site 'TestProject' was already stopped");
            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _MockIISClient.Setup(iis => iis.StartSite(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DeviceAuthModel?>()));
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            DeploymentHistoryModel<AssetModel> result = await orchestrator.Run(
                deployment,
                @"C:\Deploy",
                config);

            Assert.AreEqual(
                Status.CompletedWithWarnings,
                result.Status);
            Assert.IsNull(result.FailedAtStage);
            Assert.AreEqual(
                Status.CompletedWithWarnings,
                result.Stages[3].Status);
            Assert.IsNotNull(result.Stages[3].WarningMessages);
            Assert.HasCount(
                1,
                result.Stages[3].WarningMessages);
        }
    }
}
