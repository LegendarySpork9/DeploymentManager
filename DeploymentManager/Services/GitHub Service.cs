// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Values;

namespace DeploymentManager.Services
{
    public class GitHubService
    {
        private readonly ILoggerService _Logger;
        private readonly IGitHubClient _GitHubClient;

        // Sets the class's global variables.
        public GitHubService(
            ILoggerService _logger,
            IGitHubClient gitHubClient)
        {
            _Logger = _logger;
            _GitHubClient = gitHubClient;
        }

        /// <summary>
        /// Returns a list of the artefacts from the API.
        /// </summary>
        public async Task<ArtefactListModel?> GetArtefacts(string repository)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Fetching artefacts from GitHub for {repository} repository");

            ArtefactListModel? artefactList = null;

            try
            {
                artefactList = await _GitHubClient.GetArtefacts(repository);

                if (artefactList != null)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Fetched {artefactList.Artifacts.Count} artefact(s) from Github for {repository} repository");
                }

                else
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Failed to fetch artefacts from GitHub for {repository} repository");
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
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to fetch artefacts from GitHub for {repository} repository");
            }

            return artefactList;
        }

        /// <summary>
        /// Returns the downloaded artefact from the API.
        /// </summary>
        public async Task<string> DownloadArtefact(
            ArtefactModel artefact,
            ProjectModel project)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Downloading artefact, {artefact.Name}, from GitHub for {project.GitHub.Repository} repository");

            string downloadedFile = string.Empty;

            try
            {
                downloadedFile = await _GitHubClient.DownloadArtefact(
                    artefact.Archive_Download_Url,
                    project.Directory,
                    $"{artefact.Name}.zip");

                if (!string.IsNullOrWhiteSpace(downloadedFile))
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Downloaded artefact, {artefact.Name}, from GitHub for {project.GitHub.Repository} repository");
                }

                else
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Failed to download artefact, {artefact.Name}, from GitHub for {project.GitHub.Repository} repository");
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
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to download artefact, {artefact.Name}, from GitHub for {project.GitHub.Repository} repository");
            }

            return downloadedFile;
        }

        /// <summary>
        /// Returns a list of the releases from the API.
        /// </summary>
        public async Task<List<ReleaseModel>?> GetReleases(string repository)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Fetching releases from GitHub for {repository} repository");

            List<ReleaseModel>? releases = null;

            try
            {
                releases = await _GitHubClient.GetReleases(repository);

                if (releases != null)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Fetched releases from GitHub for {repository} repository");
                }

                else
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Failed to fetch releases from GitHub for {repository} repository");
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
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to fetch artefacts from GitHub for {repository} repository");
            }

            return releases;
        }

        /// <summary>
        /// Returns the downloaded release asset from the API.
        /// </summary>
        public async Task<string> DownloadReleaseAsset(
            AssetModel asset,
            ProjectModel project)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Downloading release asset, {asset.Name}, from GitHub for {project.GitHub.Repository} repository");

            string downloadedFile = string.Empty;

            try
            {
                downloadedFile = await _GitHubClient.DownloadReleaseAsset(
                    asset.Browser_Download_Url,
                    project.Directory,
                    asset.Name);

                if (!string.IsNullOrWhiteSpace(downloadedFile))
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Downloaded release asset, {asset.Name}, from GitHub for {project.GitHub.Repository} repository");
                }

                else
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Failed to download release asset, {asset.Name}, from GitHub for {project.GitHub.Repository} repository");
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
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to download release asset, {asset.Name}, from GitHub for {project.GitHub.Repository} repository");
            }

            return downloadedFile;
        }
    }
}
