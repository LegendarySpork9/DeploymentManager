// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Models.Shared;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Services
{
    [TestClass]
    public class DeploymentHistoryServiceTest
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
        /// Tests whether the GetDeploymentHistory method returns the model when given the json.
        /// </summary>
        [TestMethod]
        public async Task TestGetDeploymentHistory()
        {
            string deploymentHistoryString = @"[
    {
        ""id"": 1,
        ""type"": ""GitHub"",
        ""artefactType"": ""Artefact"",
        ""status"": ""Completed"",
        ""startDate"": ""2026-06-02T15:39:00"",
        ""endDate"": ""2026-06-02T15:40:00"",
        ""runTime"": ""00:01:00"",
        ""artefactId"": 1,
        ""artefactName"": ""DeploymentManager_020620261535"",
        ""artefactSize"": 1048576,
        ""branchId"": ""745hjfrnr7854uht543bhunfre7y84t3yub"",
        ""branchName"": ""2/Logging"",
        ""failedAtStage"": null,
        ""deploymentConfiguration"": {
            ""type"": ""GitHub"",
            ""environment"": ""Live"",
            ""project"": {
                ""type"": ""Website"",
                ""name"": ""TestProject"",
                ""directory"": ""Project"",
                ""gitHub"": {
                    ""repository"": ""test-repo"",
                    ""artefact"": ""test-artefact""
                },
                ""additionalDeploy"": null,
                ""ignore"": null
            },
            ""artefact"": {
                ""id"": 123,
                ""name"": ""test-artefact"",
                ""size_in_Bytes"": 1048576,
                ""archive_Download_Url"": ""https://api.github.com/repos/owner/repo/actions/artifacts/123/zip"",
                ""workflow_Run"": {
                    ""id"": 456,
                    ""head_Branch"": ""main"",
                    ""head_Sha"": ""abc123""
                }
            },
            ""primaryDeploymentTarget"": {
                ""device"": ""TestDevice"",
                ""drive"": ""C"",
                ""name"": ""Live"",
                ""artefactSource"": ""Actions""
            },
            ""secondaryDeploymentTargets"": null,
            ""deploymentSettings"": {
                ""runAdditionalDeploys"": true,
                ""restartService"": true
            }
        },
        ""stages"": [
            {
                ""name"": ""FetchArtefacts"",
                ""status"": ""Completed"",
                ""startDate"": ""2026-06-02T15:39:05"",
                ""endDate"": ""2026-06-02T15:39:55"",
                ""runTime"": ""00:00:50"",
                ""failMessages"": null
            }
        ]
    }
]";
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(deploymentHistoryString);

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            List<DeploymentHistoryModel<object>> actual = await _deploymentHistoryService.GetDeploymentHistory(AppSettings.Projects[0].Name);

            Assert.HasCount(
                1,
                actual);
            Assert.AreEqual(
                1,
                actual[0].Id);
            Assert.AreEqual(
                DeploymentType.GitHub,
                actual[0].Type);
            Assert.AreEqual(
                ArtefactType.Artefact,
                actual[0].ArtefactType);
            Assert.AreEqual(
                Status.Completed,
                actual[0].Status);
            Assert.AreEqual(
                new DateTime(2026, 06, 02, 15, 39, 00, DateTimeKind.Utc),
                actual[0].StartDate);
            Assert.AreEqual(
                new DateTime(2026, 06, 02, 15, 40, 00, DateTimeKind.Utc),
                actual[0].EndDate);
            Assert.HasCount(
                1,
                actual[0].Stages);
            Assert.AreEqual(
                DeploymentStage.FetchArtefacts,
                actual[0].Stages[0].Name);
            Assert.AreEqual(
                Status.Completed,
                actual[0].Stages[0].Status);
            Assert.AreEqual(
                new DateTime(2026, 06, 02, 15, 39, 05, DateTimeKind.Utc),
                actual[0].Stages[0].StartDate);
            Assert.AreEqual(
                new DateTime(2026, 06, 02, 15, 39, 55, DateTimeKind.Utc),
                actual[0].Stages[0].EndDate);
        }

        /// <summary>
        /// Tests whether the GetDeploymentHistory method returns an empty list when given an empty string.
        /// </summary>
        [TestMethod]
        public async Task TestGetDeploymentHistoryEmpty()
        {
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            List<DeploymentHistoryModel<object>> actual = await _deploymentHistoryService.GetDeploymentHistory(AppSettings.Projects[0].Name);

            Assert.HasCount(
                0,
                actual);
        }

        /// <summary>
        /// Tests whether the WriteDeploymentHistory method writes the correct json to the file when there is no existing history.
        /// </summary>
        [TestMethod]
        public async Task TestWriteDeploymentHistory()
        {
            DeploymentHistoryModel<ArtefactModel> newDeployment = new()
            {
                Id = 1,
                Type = DeploymentType.GitHub,
                ArtefactType = ArtefactType.Artefact,
                Status = Status.Completed,
                StartDate = new(2026, 06, 02, 15, 39, 00, DateTimeKind.Utc),
                EndDate = new(2026, 06, 02, 15, 40, 00, DateTimeKind.Utc),
                RunTime = new(00, 01, 00),
                ArtefactId = 1,
                ArtefactName = "DeploymentManager_020620261535",
                ArtefactSize = 1048576,
                BranchId = "745hjfrnr7854uht543bhunfre7y84t3yub",
                BranchName = "2/Logging",
                DeploymentConfiguration = new()
                {
                    Type = DeploymentType.GitHub,
                    Environment = DeploymentEnvironment.Live,
                    Project = AppSettings.Projects[0],
                    Artefact = new()
                    {
                        Id = 123,
                        Name = "test-artefact",
                        Size_in_Bytes = 1048576,
                        Archive_Download_Url = "https://api.github.com/repos/owner/repo/actions/artifacts/123/zip",
                        Workflow_Run = new()
                        {
                            Id = 456,
                            Head_Branch = "main",
                            Head_Sha = "abc123"
                        }
                    },
                    PrimaryDeploymentTarget = AppSettings.Environments[0],
                    DeploymentSettings = new()
                },
                Stages =
                [
                    new()
                    {
                        Name = DeploymentStage.FetchArtefacts,
                        Status = Status.Completed,
                        StartDate = new(2026, 06, 02, 15, 39, 05, DateTimeKind.Utc),
                        EndDate = new(2026, 06, 02, 15, 39, 55, DateTimeKind.Utc),
                        RunTime = new(00, 00, 50),
                    }
                ]
            };

            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            await _deploymentHistoryService.WriteDeploymentHistory(
                AppSettings.Projects[0].Name,
                newDeployment);

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    Path.Combine(
                        AppSettings.DeploymentHistoryLocation,
                        AppSettings.Projects[0].Name),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
