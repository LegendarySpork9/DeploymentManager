// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Services
{
    [TestClass]
    public class GitHubServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IGitHubClient> _MockGitHubClient = new();

        private readonly ProjectModel Project = new()
        {
            Type = ProjectType.Website,
            Name = "TestProject",
            Directory = @"C:\Deploy",
            GitHub = new()
            {
                Repository = "test-repo",
                Artefact = "test-artefact"
            },
            AdditionalDeploy = null,
            Ignore = null
        };

        /// <summary>
        /// Tests whether the GetArtefacts method returns the artefact list when the client returns data.
        /// </summary>
        [TestMethod]
        public async Task TestGetArtefactsReturnsData()
        {
            ArtefactListModel expected = new()
            {
                Total_Count = 1,
                Artifacts =
                [
                    new()
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
                    }
                ]
            };

            _MockGitHubClient.Setup(c => c.GetArtefacts(It.IsAny<string>()))
                .ReturnsAsync(expected);

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            ArtefactListModel? actual = await gitHubService.GetArtefacts("test-repo");

            Assert.IsNotNull(actual);
            Assert.AreEqual(
                expected.Total_Count,
                actual.Total_Count);
            Assert.HasCount(
                expected.Artifacts.Count,
                actual.Artifacts);
        }

        /// <summary>
        /// Tests whether the GetArtefacts method returns null when the client returns null.
        /// </summary>
        [TestMethod]
        public async Task TestGetArtefactsReturnsNull()
        {
            _MockGitHubClient.Setup(c => c.GetArtefacts(It.IsAny<string>()))
                .ReturnsAsync((ArtefactListModel?)null);

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            ArtefactListModel? actual = await gitHubService.GetArtefacts("test-repo");

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether the GetArtefacts method returns null when the client throws an exception.
        /// </summary>
        [TestMethod]
        public async Task TestGetArtefactsException()
        {
            _MockGitHubClient.Setup(c => c.GetArtefacts(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            ArtefactListModel? actual = await gitHubService.GetArtefacts("test-repo");

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether the DownloadArtefact method returns the file path when the client downloads successfully.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadArtefactReturnsFilePath()
        {
            ArtefactModel artefact = new()
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
            };

            string expectedPath = @"C:\Deploy\test-artefact.zip";

            _MockGitHubClient.Setup(c => c.DownloadArtefact(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((expectedPath, (string?)null));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            (string actual, string? errorMessage) = await gitHubService.DownloadArtefact(
                @"C:\Deploy",
                artefact,
                "Test");

            Assert.AreEqual(
                expectedPath,
                actual);
            Assert.IsNull(errorMessage);
        }

        /// <summary>
        /// Tests whether the DownloadArtefact method returns an empty string when the client returns empty.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadArtefactReturnsEmpty()
        {
            ArtefactModel artefact = new()
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
            };

            _MockGitHubClient.Setup(c => c.DownloadArtefact(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((string.Empty, (string?)null));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            (string actual, string? errorMessage) = await gitHubService.DownloadArtefact(
                @"C:\Deploy",
                artefact,
                "Test");

            Assert.IsEmpty(actual);
            Assert.IsNull(errorMessage);
        }

        /// <summary>
        /// Tests whether the DownloadArtefact method passes through the error message from the client.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadArtefactReturnsError()
        {
            ArtefactModel artefact = new()
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
            };

            _MockGitHubClient.Setup(c => c.DownloadArtefact(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((string.Empty, (string?)"Connection failed"));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            (string actual, string? errorMessage) = await gitHubService.DownloadArtefact(
                @"C:\Deploy",
                artefact,
                "Test");

            Assert.IsEmpty(actual);
            Assert.AreEqual("Connection failed", errorMessage);
        }

        /// <summary>
        /// Tests whether the GetReleases method returns the release list when the client returns data.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleasesReturnsData()
        {
            List<ReleaseModel> expected =
            [
                new()
                {
                    Id = 789,
                    Name = "v1.0.0",
                    Tag_Name = "v1.0.0",
                    Published_At = new(2026, 06, 03, 12, 00, 00, DateTimeKind.Utc),
                    Assets =
                    [
                        new()
                        {
                            Id = 101,
                            Name = "release.zip",
                            Size = 2097152,
                            Content_Type = "application/zip",
                            Browser_Download_Url = "https://github.com/owner/repo/releases/download/v1.0.0/release.zip"
                        }
                    ]
                }
            ];

            _MockGitHubClient.Setup(c => c.GetReleases(It.IsAny<string>()))
                .ReturnsAsync(expected);

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            List<ReleaseModel>? actual = await gitHubService.GetReleases("test-repo");

            Assert.IsNotNull(actual);
            Assert.HasCount(
                expected.Count,
                actual);
        }

        /// <summary>
        /// Tests whether the GetReleases method returns null when the client returns null.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleasesReturnsNull()
        {
            _MockGitHubClient.Setup(c => c.GetReleases(It.IsAny<string>()))
                .ReturnsAsync((List<ReleaseModel>?)null);

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            List<ReleaseModel>? actual = await gitHubService.GetReleases("test-repo");

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether the GetReleases method returns null when the client throws an exception.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleasesException()
        {
            _MockGitHubClient.Setup(c => c.GetReleases(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            List<ReleaseModel>? actual = await gitHubService.GetReleases("test-repo");

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether the DownloadReleaseAsset method returns the file path when the client downloads successfully.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadReleaseAssetReturnsFilePath()
        {
            AssetModel asset = new()
            {
                Id = 101,
                Name = "release.zip",
                Size = 2097152,
                Content_Type = "application/zip",
                Browser_Download_Url = "https://github.com/owner/repo/releases/download/v1.0.0/release.zip"
            };

            string expectedPath = @"C:\Deploy\release.zip";

            _MockGitHubClient.Setup(c => c.DownloadReleaseAsset(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((expectedPath, (string?)null));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            (string actual, string? errorMessage) = await gitHubService.DownloadReleaseAsset(
                @"C:\Deploy",
                asset,
                "Test");

            Assert.AreEqual(
                expectedPath,
                actual);
            Assert.IsNull(errorMessage);
        }

        /// <summary>
        /// Tests whether the DownloadReleaseAsset method returns an empty string when the client returns empty.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadReleaseAssetReturnsEmpty()
        {
            AssetModel asset = new()
            {
                Id = 101,
                Name = "release.zip",
                Size = 2097152,
                Content_Type = "application/zip",
                Browser_Download_Url = "https://github.com/owner/repo/releases/download/v1.0.0/release.zip"
            };

            _MockGitHubClient.Setup(c => c.DownloadReleaseAsset(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((string.Empty, (string?)null));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            (string actual, string? errorMessage) = await gitHubService.DownloadReleaseAsset(
                @"C:\Deploy",
                asset,
                "Test");

            Assert.IsEmpty(actual);
            Assert.IsNull(errorMessage);
        }

        /// <summary>
        /// Tests whether the DownloadReleaseAsset method passes through the error message from the client.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadReleaseAssetReturnsError()
        {
            AssetModel asset = new()
            {
                Id = 101,
                Name = "release.zip",
                Size = 2097152,
                Content_Type = "application/zip",
                Browser_Download_Url = "https://github.com/owner/repo/releases/download/v1.0.0/release.zip"
            };

            _MockGitHubClient.Setup(c => c.DownloadReleaseAsset(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((string.Empty, (string?)"Connection failed"));

            GitHubService gitHubService = new(
                _MockLogger.Object,
                _MockGitHubClient.Object);

            (string actual, string? errorMessage) = await gitHubService.DownloadReleaseAsset(
                @"C:\Deploy",
                asset,
                "Test");

            Assert.IsEmpty(actual);
            Assert.AreEqual("Connection failed", errorMessage);
        }
    }
}
