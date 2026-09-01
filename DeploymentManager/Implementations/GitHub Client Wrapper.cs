// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses;
using DeploymentManager.Values;
using Newtonsoft.Json;
using RestSharp;

namespace DeploymentManager.Implementations
{
    public class GitHubClientWrapper : IGitHubClient
    {
        private readonly ILoggerService _Logger;
        private readonly IFileSystem _FileSystem;
        private readonly IRestClientWrapper _RestClient;
        private readonly IHttpDownloadClient _DownloadClient;
        private readonly GitHubOptionsModel Options;
        private readonly string BaseURL = "https://api.github.com";

        // Sets the class's global variables.
        public GitHubClientWrapper(
            ILoggerService _logger,
            IFileSystem _fileSystem,
            IRestClientWrapper _restClient,
            IHttpDownloadClient _downloadClient,
            GitHubOptionsModel options)
        {
            _Logger = _logger;
            _FileSystem = _fileSystem;
            _RestClient = _restClient;
            _DownloadClient = _downloadClient;
            Options = options;
        }

        /// <summary>
        /// Returns a list of the artefacts from the API.
        /// </summary>
        public async Task<ArtefactListModel?> GetArtefacts(string repository)
        {
            ArtefactListModel? artefactList = null;

            try
            {
                string url = BuildURL(
                    "/actions/artifacts",
                    repository);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"URL: {url}");

                RestRequest request = new()
                {
                    Method = Method.Get
                };

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Configured Rest Request");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Sending Request");

                request.AddHeader(
                    "Authorization",
                    $"Bearer {Options.Auth}");

                RestResponse response = await _RestClient.ExecuteAsync(
                    url,
                    request);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Response Code: {response.StatusCode}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                {
                    artefactList = JsonConvert.DeserializeObject<ArtefactListModel?>(response.Content);
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            return artefactList;
        }

        /// <summary>
        /// Downloads the given artefact from the API.
        /// </summary>
        public async Task<(string, string?)> DownloadArtefact(
            string downloadURL,
            string downloadPath,
            string downloadFile)
        {
            string downloadedFile = string.Empty;
            string? errorMessage = null;

            try
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Sending Request");

                Stream? downloadStream = await _DownloadClient.DownloadStreamAsync(
                    downloadURL,
                    Options.Auth);

                if (downloadStream == null)
                {
                    throw new Exception("Failed to download artefact");
                }

                await _FileSystem.CreateDirectory(downloadPath);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Created Directory");

                using (Stream stream = downloadStream)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        "Downloading File");

                    await _FileSystem.WriteStream(
                        Path.Combine(
                            downloadPath,
                            downloadFile),
                        stream);
                }

                downloadedFile = Path.Combine(
                    downloadPath,
                    downloadFile);
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    errorMessage);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            return (downloadedFile, errorMessage);
        }

        /// <summary>
        /// Returns a list of the releases from the API.
        /// </summary>
        public async Task<List<ReleaseModel>?> GetReleases(string repository)
        {
            List<ReleaseModel>? releases = null;

            try
            {
                string url = BuildURL(
                    "/releases",
                    repository);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"URL: {url}");

                RestRequest request = new()
                {
                    Method = Method.Get
                };

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Configured Rest Request");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Sending Request");

                request.AddHeader(
                    "Authorization",
                    $"Bearer {Options.Auth}");

                RestResponse response = await _RestClient.ExecuteAsync(
                    url,
                    request);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Response Code: {response.StatusCode}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Response Message: {response.ErrorException?.Message ?? response.Content}");

                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                {
                    releases = JsonConvert.DeserializeObject<List<ReleaseModel>>(response.Content) ?? [];

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Releases Returned: {releases.Count}");
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            return releases;
        }

        /// <summary>
        /// Downloads the given release asset from the API.
        /// </summary>
        public async Task<(string, string?)> DownloadReleaseAsset(
            string downloadURL,
            string downloadPath,
            string downloadFile)
        {
            string downloadedFile = string.Empty;
            string? errorMessage = null;

            try
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Sending Request");

                Stream? downloadStream = await _DownloadClient.DownloadStreamAsync(
                    downloadURL);

                if (downloadStream == null)
                {
                    throw new Exception("Failed to download release asset");
                }

                await _FileSystem.CreateDirectory(downloadPath);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Created Directory");

                using (Stream stream = downloadStream)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        "Downloading File");

                    await _FileSystem.WriteStream(
                        Path.Combine(
                            downloadPath,
                            downloadFile),
                        stream);
                }

                downloadedFile = Path.Combine(
                    downloadPath,
                    downloadFile);
            }

            catch (Exception ex)
            {
                errorMessage = ex.ToString();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    errorMessage);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            return (downloadedFile, errorMessage);
        }

        /// <summary>
        /// Returns the API url.
        /// </summary>
        private string BuildURL(
            string endpoint,
            string repository) => $"{BaseURL}/repos/{Options.Owner}/{repository}{endpoint}";
    }
}
