// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Implementations;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses;
using DeploymentManager.Models.Responses.Related;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace DeploymentManager.PersistenceTests.Implementations
{
    [TestClass]
    public class GitHubClientWrapperTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();
        private readonly Mock<IRestClientWrapper> _MockRestClient = new();
        private readonly Mock<IHttpDownloadClient> _MockDownloadClient = new();

        private readonly GitHubOptionsModel Options = new()
        {
            Auth = "test-token-123",
            Owner = "test-owner"
        };

        /// <summary>
        /// Tests whether GetArtefacts deserialises the response when the REST client returns a valid artefact list.
        /// </summary>
        [TestMethod]
        public async Task TestGetArtefacts()
        {
            ArtefactListModel expected = new()
            {
                Total_Count = 1,
                Artifacts =
                [
                    new()
                    {
                        Id = 100,
                        Name = "build-output",
                        Size_in_Bytes = 2048,
                        Archive_Download_Url = "https://api.github.com/repos/test-owner/test-repo/actions/artifacts/100/zip",
                        Workflow_Run = new()
                        {
                            Id = 200,
                            Head_Branch = "main",
                            Head_Sha = "def456"
                        }
                    }
                ]
            };

            string json = JsonConvert.SerializeObject(expected);

            _MockRestClient.Setup(c => c.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<RestRequest>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = json,
                    ResponseStatus = ResponseStatus.Completed
                });

            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            ArtefactListModel? actual = await wrapper.GetArtefacts("test-repo");

            Assert.IsNotNull(actual);
            Assert.AreEqual(
                expected.Total_Count,
                actual.Total_Count);
            Assert.HasCount(
                1,
                actual.Artifacts);
            Assert.AreEqual(
                expected.Artifacts[0].Id,
                actual.Artifacts[0].Id);
            Assert.AreEqual(
                expected.Artifacts[0].Name,
                actual.Artifacts[0].Name);
        }

        /// <summary>
        /// Tests whether GetArtefacts returns null when the REST client returns an unauthorised response.
        /// </summary>
        [TestMethod]
        public async Task TestGetArtefactsError()
        {
            _MockRestClient.Setup(c => c.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<RestRequest>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = null,
                    ResponseStatus = ResponseStatus.Completed
                });

            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            ArtefactListModel? actual = await wrapper.GetArtefacts("test-repo");

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether GetReleases deserialises the response when the REST client returns a valid release list.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleases()
        {
            List<ReleaseModel> expected =
            [
                new()
                {
                    Id = 300,
                    Name = "v2.0.0",
                    Tag_Name = "v2.0.0",
                    Published_At = new(2026, 07, 01, 10, 30, 00, DateTimeKind.Utc),
                    Assets =
                    [
                        new()
                        {
                            Id = 400,
                            Name = "app.zip",
                            Size = 4096,
                            Content_Type = "application/zip",
                            Browser_Download_Url = "https://github.com/test-owner/test-repo/releases/download/v2.0.0/app.zip"
                        }
                    ]
                }
            ];

            string json = JsonConvert.SerializeObject(expected);

            _MockRestClient.Setup(c => c.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<RestRequest>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = json,
                    ResponseStatus = ResponseStatus.Completed
                });

            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            List<ReleaseModel>? actual = await wrapper.GetReleases("test-repo");

            Assert.IsNotNull(actual);
            Assert.HasCount(
                1,
                actual);
            Assert.AreEqual(
                expected[0].Id,
                actual[0].Id);
            Assert.AreEqual(
                expected[0].Name,
                actual[0].Name);
            Assert.AreEqual(
                expected[0].Tag_Name,
                actual[0].Tag_Name);
        }

        /// <summary>
        /// Tests whether GetReleases returns null when the REST client returns an error response.
        /// </summary>
        [TestMethod]
        public async Task TestGetReleasesError()
        {
            _MockRestClient.Setup(c => c.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<RestRequest>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = null,
                    ResponseStatus = ResponseStatus.Completed
                });

            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            List<ReleaseModel>? actual = await wrapper.GetReleases("test-repo");

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether DownloadArtefact returns an error message when the download URL is invalid.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadArtefactFailed()
        {
            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            (string downloadedFile, string? errorMessage) = await wrapper.DownloadArtefact(
                "https://invalid-url-that-will-fail.example.com/artifact.zip",
                Path.Combine(
                    Path.GetTempPath(),
                    "test-download"),
                "artefact.zip");

            Assert.IsEmpty(downloadedFile);
            Assert.IsNotNull(errorMessage);
        }

        /// <summary>
        /// Tests whether DownloadReleaseAsset returns an error message when the download URL is invalid.
        /// </summary>
        [TestMethod]
        public async Task TestDownloadReleaseAssetFailed()
        {
            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            (string downloadedFile, string? errorMessage) = await wrapper.DownloadReleaseAsset(
                "https://invalid-url-that-will-fail.example.com/release.zip",
                Path.Combine(
                    Path.GetTempPath(),
                    "test-download"),
                "release.zip");

            Assert.IsEmpty(downloadedFile);
            Assert.IsNotNull(errorMessage);
        }

        /// <summary>
        /// Tests whether the Bearer token is included in the authorisation header of REST requests.
        /// </summary>
        [TestMethod]
        public async Task TestBearerTokenIncluded()
        {
            string? capturedUrl = null;
            RestRequest? capturedRequest = null;

            _MockRestClient.Setup(c => c.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<RestRequest>()))
                .Callback<string, RestRequest>((url, request) =>
                {
                    capturedUrl = url;
                    capturedRequest = request;
                })
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonConvert.SerializeObject(new ArtefactListModel
                    {
                        Total_Count = 0,
                        Artifacts = []
                    }),
                    ResponseStatus = ResponseStatus.Completed
                });

            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            await wrapper.GetArtefacts("test-repo");

            Assert.IsNotNull(capturedRequest);
            Assert.IsNotNull(capturedUrl);

            Assert.Contains(
                "test-owner",
                capturedUrl);
            Assert.Contains(
                "test-repo",
                capturedUrl);

            Parameter? authHeader = capturedRequest.Parameters.FirstOrDefault(
                p => p.Name == "Authorization");

            Assert.IsNotNull(authHeader);
            Assert.AreEqual(
                $"Bearer {Options.Auth}",
                authHeader.Value?.ToString());
        }

        /// <summary>
        /// Tests whether GetArtefacts builds the correct URL containing the owner and repository.
        /// </summary>
        [TestMethod]
        public async Task TestGetArtefactsBuildsCorrectUrl()
        {
            string? capturedUrl = null;

            _MockRestClient.Setup(c => c.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<RestRequest>()))
                .Callback<string, RestRequest>((url, _) =>
                {
                    capturedUrl = url;
                })
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonConvert.SerializeObject(new ArtefactListModel
                    {
                        Total_Count = 0,
                        Artifacts = []
                    }),
                    ResponseStatus = ResponseStatus.Completed
                });

            GitHubClientWrapper wrapper = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                _MockRestClient.Object,
                _MockDownloadClient.Object,
                Options);

            await wrapper.GetArtefacts("my-repo");

            string expected = "https://api.github.com/repos/test-owner/my-repo/actions/artifacts";

            Assert.AreEqual(
                expected,
                capturedUrl);
        }
    }
}
