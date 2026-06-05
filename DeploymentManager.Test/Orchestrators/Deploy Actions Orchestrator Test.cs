// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Models.Shared;
using DeploymentManager.Orchestrators.GitHub;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Orchestrators
{
    [TestClass]
    public class DeployActionsOrchestratorTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IClock> _MockClock = new();
        private readonly Mock<IGitHubClient> _MockGitHubClient = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();
        private readonly Mock<IIISClient> _MockIISClient = new();
        private readonly Mock<ITaskScheduler> _MockTaskScheduler = new();

        private static readonly DateTime TestDate = new(2026, 06, 04, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime DefaultDate = new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        private DeployActionsOrchestrator CreateOrchestrator()
        {
            _MockClock.Setup(c => c.UtcNow).Returns(TestDate);
            _MockClock.Setup(c => c.DefaultDate).Returns(DefaultDate);
            _MockClock.Setup(c => c.DefaultTimeSpan).Returns(TimeSpan.Zero);

            GitHubService gitHubService = new(_MockLogger.Object, _MockGitHubClient.Object);
            DocumentService documentService = new(_MockLogger.Object, _MockFileSystem.Object);
            IISService iisService = new(_MockLogger.Object, _MockIISClient.Object);
            TaskSchedulerService taskSchedulerService = new(_MockLogger.Object, _MockTaskScheduler.Object);

            return new DeployActionsOrchestrator(
                _MockLogger.Object,
                _MockClock.Object,
                gitHubService,
                documentService,
                iisService,
                taskSchedulerService);
        }

        private static DeploymentConfigurationModel<ArtefactModel> CreateConfig()
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
                    GitHub = new() { Repository = "test-repo", Artefact = "test-artefact" }
                },
                Artefact = new()
                {
                    Id = 123,
                    Name = "test-artefact",
                    Size_in_Bytes = 1048576,
                    Archive_Download_Url = "https://api.github.com/repos/owner/repo/actions/artifacts/123/zip",
                    Workflow_Run = new() { Id = 456, Head_Branch = "main", Head_Sha = "abc123" }
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
        }

        private static DeploymentHistoryModel<ArtefactModel> CreateDeploymentHistory(
            DeploymentConfigurationModel<ArtefactModel> config)
        {
            DateTime defaultDate = new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            return new()
            {
                Id = 1,
                Type = DeploymentType.GitHub,
                ArtefactType = ArtefactType.Artefact,
                Status = Status.PendingApproval,
                StartDate = defaultDate,
                EndDate = defaultDate,
                RunTime = TimeSpan.Zero,
                ArtefactId = 123,
                ArtefactName = "test-artefact",
                ArtefactSize = 1048576,
                BranchId = "abc123",
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
        /// Tests whether the Run method completes all stages successfully for a GitHub Actions deployment.
        /// </summary>
        [TestMethod]
        public async Task TestRunAllStagesComplete()
        {
            DeployActionsOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<ArtefactModel> config = CreateConfig();
            DeploymentHistoryModel<ArtefactModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadArtefact(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((@"C:\Deploy\test-artefact.zip", (string?)null));

            FileStream fileStream = new(
                Path.GetTempFileName(), FileMode.Open, FileAccess.Read,
                FileShare.None, 4096, FileOptions.DeleteOnClose);

            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>())).ReturnsAsync(fileStream);
            _MockFileSystem.Setup(fs => fs.ExtractArchive(It.IsAny<string>(), It.IsAny<FileStream>())).Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>())).ReturnsAsync([@"C:\Deploy\test-artefact\file1.dll"]);
            _MockIISClient.Setup(iis => iis.StopSite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DeviceAuthModel?>())).Returns((string?)null);
            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>())).ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _MockIISClient.Setup(iis => iis.StartSite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DeviceAuthModel?>()));
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>())).Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Returns(Task.CompletedTask);

            DeploymentHistoryModel<ArtefactModel> result = await orchestrator.Run(
                deployment, @"C:\Deploy", config);

            Assert.AreEqual(Status.Completed, result.Status);
            Assert.IsNull(result.FailedAtStage);
            Assert.IsTrue(result.Stages.All(s => s.Status == Status.Completed));
        }

        /// <summary>
        /// Tests whether the Run method fails when the fetch artefacts stage fails.
        /// </summary>
        [TestMethod]
        public async Task TestRunFetchArtefactsFails()
        {
            DeployActionsOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<ArtefactModel> config = CreateConfig();
            DeploymentHistoryModel<ArtefactModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadArtefact(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string.Empty, (string?)"Download failed"));

            DeploymentHistoryModel<ArtefactModel> result = await orchestrator.Run(
                deployment, @"C:\Deploy", config);

            Assert.AreEqual(Status.Failed, result.Status);
            Assert.AreEqual(DeploymentStage.FetchArtefacts, result.FailedAtStage);
            Assert.AreEqual(Status.Failed, result.Stages[0].Status);
            Assert.IsNotNull(result.Stages[0].FailMessages);
        }

        /// <summary>
        /// Tests whether the Run method skips the start services stage when RestartService is false.
        /// </summary>
        [TestMethod]
        public async Task TestRunSkipsStartServicesWhenNotRestarting()
        {
            DeployActionsOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<ArtefactModel> config = CreateConfig();
            config.DeploymentSettings = new() { RestartService = false };
            DeploymentHistoryModel<ArtefactModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadArtefact(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((@"C:\Deploy\test-artefact.zip", (string?)null));

            FileStream fileStream = new(
                Path.GetTempFileName(), FileMode.Open, FileAccess.Read,
                FileShare.None, 4096, FileOptions.DeleteOnClose);

            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>())).ReturnsAsync(fileStream);
            _MockFileSystem.Setup(fs => fs.ExtractArchive(It.IsAny<string>(), It.IsAny<FileStream>())).Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>())).ReturnsAsync([@"C:\Deploy\test-artefact\file1.dll"]);
            _MockIISClient.Setup(iis => iis.StopSite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DeviceAuthModel?>())).Returns((string?)null);
            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>())).ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>())).Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Returns(Task.CompletedTask);

            DeploymentHistoryModel<ArtefactModel> result = await orchestrator.Run(
                deployment, @"C:\Deploy", config);

            Assert.AreEqual(Status.Completed, result.Status);
            Assert.AreEqual(Status.Skipped, result.Stages[5].Status);
        }

        /// <summary>
        /// Tests whether the Run method completes with warnings when the stop services stage returns a warning.
        /// </summary>
        [TestMethod]
        public async Task TestRunCompletesWithWarningsWhenStopServiceWarns()
        {
            DeployActionsOrchestrator orchestrator = CreateOrchestrator();
            DeploymentConfigurationModel<ArtefactModel> config = CreateConfig();
            DeploymentHistoryModel<ArtefactModel> deployment = CreateDeploymentHistory(config);

            _MockGitHubClient.Setup(c => c.DownloadArtefact(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((@"C:\Deploy\test-artefact.zip", (string?)null));

            FileStream fileStream = new(
                Path.GetTempFileName(), FileMode.Open, FileAccess.Read,
                FileShare.None, 4096, FileOptions.DeleteOnClose);

            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>())).ReturnsAsync(fileStream);
            _MockFileSystem.Setup(fs => fs.ExtractArchive(It.IsAny<string>(), It.IsAny<FileStream>())).Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>())).ReturnsAsync([@"C:\Deploy\test-artefact\file1.dll"]);
            _MockIISClient.Setup(iis => iis.StopSite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DeviceAuthModel?>())).Returns("IIS site 'TestProject' was already stopped");
            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>())).ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _MockIISClient.Setup(iis => iis.StartSite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DeviceAuthModel?>()));
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>())).Returns(Task.CompletedTask);
            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Returns(Task.CompletedTask);

            DeploymentHistoryModel<ArtefactModel> result = await orchestrator.Run(
                deployment, @"C:\Deploy", config);

            Assert.AreEqual(Status.CompletedWithWarnings, result.Status);
            Assert.IsNull(result.FailedAtStage);
            Assert.AreEqual(Status.CompletedWithWarnings, result.Stages[3].Status);
            Assert.IsNotNull(result.Stages[3].WarningMessages);
            Assert.HasCount(1, result.Stages[3].WarningMessages);
        }
    }
}
