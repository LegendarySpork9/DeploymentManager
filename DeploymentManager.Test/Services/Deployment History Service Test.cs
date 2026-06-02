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
    public class DeploymentHistoryServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();

        private readonly AppSettingsModel AppSettings = new()
        {
            SiteAuth = string.Empty,
            DeploymentHistoryLocation = @"C:\DeployHistory",
            Environments =
            [
                new()
                {
                    Device = string.Empty,
                    Drive = string.Empty,
                    Name = string.Empty
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
                    Name = string.Empty,
                    Directory = string.Empty,
                    GitHub = new()
                    {
                        Repository = string.Empty,
                        Artefact = string.Empty
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
        ""type"": ""GitHubActions"",
        ""status"": ""Completed"",
        ""startDate"": ""2026-06-02T15:39:00"",
        ""endDate"": ""2026-06-02T15:40:00"",
        ""runTime"": ""00:01:00"",
        ""artefactId"": 1,
        ""artefactName"": ""DeploymentManager_020620261535"",
        ""artefactSize"": 1048576,
        ""gitHubBranchId"": ""745hjfrnr7854uht543bhunfre7y84t3yub"",
        ""gitHubBranchName"": ""2/Logging"",
        ""stages"": [
            {
                ""name"": """",
                ""status"": ""Completed"",
                ""startDate"": ""2026-06-02T15:39:05"",
                ""endDate"": ""2026-06-02T15:39:55"",
                ""runTime"": ""00:00:50""
            }
        ]
    }
]";
            List<DeploymentHistoryModel> expected =
            [
                new()
                {
                    Id = 1,
                    Type = DeploymentType.GitHubActions,
                    Status = Status.Completed,
                    StartDate = new(2026, 06, 02, 15, 39, 00, DateTimeKind.Utc),
                    EndDate = new(2026, 06, 02, 15, 40, 00, DateTimeKind.Utc),
                    RunTime = new(00, 01, 00),
                    ArtefactId = 1,
                    ArtefactName = "DeploymentManager_020620261535",
                    ArtefactSize = 1048576,
                    GitHubBranchId = "745hjfrnr7854uht543bhunfre7y84t3yub",
                    GitHubBranchName = "2/Logging",
                    Stages =
                    [
                        new()
                        {
                            Name = "",
                            Status = Status.Completed,
                            StartDate = new(2026, 06, 02, 15, 39, 05, DateTimeKind.Utc),
                            EndDate = new(2026, 06, 02, 15, 39, 55, DateTimeKind.Utc),
                            RunTime = new(00, 00, 50),
                        }
                    ]
                }
            ];

            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(deploymentHistoryString);

            DeploymentHistoryService _deploymentHistoryService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            List<DeploymentHistoryModel> actual = await _deploymentHistoryService.GetDeploymentHistory(AppSettings.Projects[0].Name);

            Assert.HasCount(
                expected.Count,
                actual);

            for (int x = 0; x  < expected.Count; x++)
            {
                Assert.AreEqual(
                    expected[x].Id,
                    actual[x].Id);
                Assert.AreEqual(
                    expected[x].Type,
                    actual[x].Type);
                Assert.AreEqual(
                    expected[x].Status,
                    actual[x].Status);
                Assert.AreEqual(
                    expected[x].StartDate,
                    actual[x].StartDate);
                Assert.AreEqual(
                    expected[x].EndDate,
                    actual[x].EndDate);

                for (int y = 0; y < expected[x].Stages.Count; y++)
                {
                    Assert.AreEqual(
                        expected[x].Stages[y].Name,
                        actual[x].Stages[y].Name);
                    Assert.AreEqual(
                        expected[x].Stages[y].Status,
                        actual[x].Stages[y].Status);
                    Assert.AreEqual(
                        expected[x].Stages[y].StartDate,
                        actual[x].Stages[y].StartDate);
                    Assert.AreEqual(
                        expected[x].Stages[y].EndDate,
                        actual[x].Stages[y].EndDate);
                }
            }
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

            List<DeploymentHistoryModel> actual = await _deploymentHistoryService.GetDeploymentHistory(AppSettings.Projects[0].Name);

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
            DeploymentHistoryModel newDeployment = new()
            {
                Id = 1,
                Type = DeploymentType.GitHubActions,
                Status = Status.Completed,
                StartDate = new(2026, 06, 02, 15, 39, 00),
                EndDate = new(2026, 06, 02, 15, 40, 00),
                RunTime = new(00, 01, 00),
                ArtefactId = 1,
                ArtefactName = "DeploymentManager_020620261535",
                ArtefactSize = 1048576,
                GitHubBranchId = "745hjfrnr7854uht543bhunfre7y84t3yub",
                GitHubBranchName = "2/Logging",
                Stages =
                [
                    new()
                    {
                        Name = "",
                        Status = Status.Completed,
                        StartDate = new(2026, 06, 02, 15, 39, 05),
                        EndDate = new(2026, 06, 02, 15, 39, 55),
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
                    $@"{AppSettings.DeploymentHistoryLocation}\{AppSettings.Projects[0].Name}",
                    It.IsAny<string>()),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the WriteDeploymentHistory method writes the correct json to the file when there is an existing history.
        /// </summary>
        [TestMethod]
        public async Task TestWriteDeploymentHistoryExisting()
        {
            string existingHistoryString = @"[
    {
        ""id"": 1,
        ""type"": ""GitHubActions"",
        ""status"": ""Completed"",
        ""startDate"": ""2026-06-02T15:39:00"",
        ""endDate"": ""2026-06-02T15:40:00"",
        ""runTime"": ""00:01:00"",
        ""artefactId"": 1,
        ""artefactName"": ""DeploymentManager_020620261535"",
        ""artefactSize"": 1048576,
        ""gitHubBranchId"": ""745hjfrnr7854uht543bhunfre7y84t3yub"",
        ""gitHubBranchName"": ""2/Logging"",
        ""stages"": [
            {
                ""name"": """",
                ""status"": ""Completed"",
                ""startDate"": ""2026-06-02T15:39:05"",
                ""endDate"": ""2026-06-02T15:39:55"",
                ""runTime"": ""00:00:50""
            }
        ]
    }
]";

            DeploymentHistoryModel newDeployment = new()
            {
                Id = 2,
                Type = DeploymentType.FileUpload,
                Status = Status.PendingApproval,
                StartDate = new(2026, 06, 02, 16, 00, 00),
                EndDate = new(2026, 06, 02, 16, 05, 00),
                RunTime = new(00, 05, 00),
                ArtefactId = 2,
                ArtefactName = "DeploymentManager_020620261600",
                ArtefactSize = 2097152,
                GitHubBranchId = "abc123def456",
                GitHubBranchName = "3/Logging",
                Stages = []
            };

            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(existingHistoryString);
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
                    $@"{AppSettings.DeploymentHistoryLocation}\{AppSettings.Projects[0].Name}",
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
