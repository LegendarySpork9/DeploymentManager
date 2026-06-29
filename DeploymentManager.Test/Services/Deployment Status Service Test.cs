// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Services
{
    [TestClass]
    public class DeploymentStatusServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();

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
            GitHubOptions = new()
            {
                Auth = string.Empty,
                Owner = string.Empty
            },
            Projects =
            [
                new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = "Project",
                    GitHub = new()
                    {
                        Repository = "test-repo",
                        Artefact = "test-artefact"
                    },
                    AdditionalDeploy = null,
                    Ignore = null
                }
            ]
        };

        /// <summary>
        /// Tests whether the GetDeploymentStatus method returns the model when given the json.
        /// </summary>
        [TestMethod]
        public async Task TestGetDeploymentStatus()
        {
            string deploymentStatusString = @"[
    {
        ""project"": ""TestProject"",
        ""environment"": ""Live"",
        ""deploymentId"": 1,
        ""artefactName"": ""TestArtefact"",
        ""artefactType"": ""Artefact"",
        ""branchName"": ""main"",
        ""deployedAt"": ""2026-06-07T14:56:39"",
        ""status"": ""Completed""
    }
]";
            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(deploymentStatusString);

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            List<DeploymentStatusModel> actual = await _deploymentHistoryService.GetDeploymentStatus();

            Assert.HasCount(
                1,
                actual);
            Assert.AreEqual(
                "TestProject",
                actual[0].Project);
            Assert.AreEqual(
                DeploymentEnvironment.Live,
                actual[0].Environment);
            Assert.AreEqual(
                1,
                actual[0].DeploymentId);
            Assert.AreEqual(
                "TestArtefact",
                actual[0].ArtefactName);
            Assert.AreEqual(
                ArtefactType.Artefact,
                actual[0].ArtefactType);
            Assert.AreEqual(
                "main",
                actual[0].BranchName);
            Assert.AreEqual(
                new DateTime(2026, 06, 07, 14, 56, 39, DateTimeKind.Utc),
                actual[0].DeployedAt);
            Assert.AreEqual(
                Status.Completed,
                actual[0].Status);
        }

        /// <summary>
        /// Tests whether the GetDeploymentStatus method returns an empty list when no file exists.
        /// </summary>
        [TestMethod]
        public async Task TestGetDeploymentStatusEmpty()
        {
            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(false);

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            List<DeploymentStatusModel> actual = await _deploymentHistoryService.GetDeploymentStatus();

            Assert.HasCount(
                0,
                actual);
        }

        /// <summary>
        /// Tests whether the WriteDeploymentStatus method writes the correct json to the file when there is no existing status file.
        /// </summary>
        [TestMethod]
        public async Task TestWriteDeploymentStatus()
        {
            DeploymentStatusModel entry = new()
            {
                Project = "TestProject",
                Environment = DeploymentEnvironment.Live,
                DeploymentId = 1,
                ArtefactName = "TestArtefact",
                ArtefactType = ArtefactType.Artefact,
                BranchName = "main",
                DeployedAt = new(2026, 06, 07, 14, 56, 39, DateTimeKind.Utc),
                Status = Status.Completed
            };

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(false);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            await _deploymentHistoryService.WriteDeploymentStatus(entry);

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    Path.Combine(
                        AppSettings.DeploymentHistoryLocation,
                        "DeploymentStatus.json"),
                    It.IsAny<string>()),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the WriteDeploymentStatus method upserts the entry when a matching project and environment already exists.
        /// </summary>
        [TestMethod]
        public async Task TestWriteDeploymentStatusUpsert()
        {
            string existingStatusString = @"[
    {
        ""project"": ""TestProject"",
        ""environment"": ""Live"",
        ""deploymentId"": 1,
        ""artefactName"": ""OldArtefact"",
        ""artefactType"": ""Artefact"",
        ""branchName"": ""main"",
        ""deployedAt"": ""2026-06-05T10:00:00"",
        ""status"": ""Completed""
    }
]";
            DeploymentStatusModel entry = new()
            {
                Project = "TestProject",
                Environment = DeploymentEnvironment.Live,
                DeploymentId = 2,
                ArtefactName = "NewArtefact",
                ArtefactType = ArtefactType.Artefact,
                BranchName = "feature/update",
                DeployedAt = new(2026, 06, 07, 14, 56, 39, DateTimeKind.Utc),
                Status = Status.Completed
            };

            string? writtenJson = null;

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(existingStatusString);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Callback<string, string>((_, json) => writtenJson = json);

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            await _deploymentHistoryService.WriteDeploymentStatus(entry);

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    Path.Combine(
                        AppSettings.DeploymentHistoryLocation,
                        "DeploymentStatus.json"),
                    It.IsAny<string>()),
                Times.Once);

            Assert.IsNotNull(writtenJson);

            List<DeploymentStatusModel>? written = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeploymentStatusModel>>(writtenJson);

            Assert.IsNotNull(written);
            Assert.HasCount(
                1,
                written);
            Assert.AreEqual(
                2,
                written[0].DeploymentId);
            Assert.AreEqual(
                "NewArtefact",
                written[0].ArtefactName);
            Assert.AreEqual(
                "feature/update",
                written[0].BranchName);
        }
    }
}
