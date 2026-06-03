// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Values;
using Newtonsoft.Json;

namespace DeploymentManager.Services
{
    public class DeploymentHistoryService
    {
        private readonly ILoggerService _Logger;
        private readonly IFileSystem _FileSystem;

        private readonly AppSettingsModel AppSettings;

        // Sets the class's global variables.
        public DeploymentHistoryService(
            ILoggerService _logger,
            IFileSystem _fileSystem,
            AppSettingsModel appSettings)
        {
            _Logger = _logger;
            _FileSystem = _fileSystem;
            AppSettings = appSettings;
        }

        /// <summary>
        /// Returns a list of deployment histories.
        /// </summary>
        public async Task<List<DeploymentHistoryModel>> GetDeploymentHistory(string project)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Fetching deployment history for project, {project}");

            List<DeploymentHistoryModel> deploymentHistory = [];

            try
            {
                string deploymentHistoryJSONString = await _FileSystem.ReadAllText(Path.Combine(AppSettings.DeploymentHistoryLocation, project));
                deploymentHistory = JsonConvert.DeserializeObject<List<DeploymentHistoryModel>>(deploymentHistoryJSONString) ?? [];

                foreach (DeploymentHistoryModel deploy in deploymentHistory)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Specifying date times as UTC for deploment {deploy.Id}");

                    deploy.StartDate = DateTime.SpecifyKind(
                        deploy.StartDate,
                        DateTimeKind.Utc);
                    deploy.EndDate = DateTime.SpecifyKind(
                        deploy.EndDate,
                        DateTimeKind.Utc);

                    foreach (StageModel stage in deploy.Stages)
                    {
                        deploy.StartDate = DateTime.SpecifyKind(
                            deploy.StartDate,
                            DateTimeKind.Utc);
                        deploy.EndDate = DateTime.SpecifyKind(
                            deploy.EndDate,
                            DateTimeKind.Utc);
                    }

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Specified date times as UTC for deploment {deploy.Id}");
                }

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Fetched {deploymentHistory.Count} history records for project, {project}.");
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
                    $"Failed to fetch deployment history for project, {project}");
            }

            return deploymentHistory;
        }

        /// <summary>
        /// Writes the deployment history.
        /// </summary>
        public async Task WriteDeploymentHistory(
            string project,
            DeploymentHistoryModel deployment)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Writing deployment history for project, {project}");

            try
            {
                string deploymentHistoryJSONString = await _FileSystem.ReadAllText(Path.Combine(AppSettings.DeploymentHistoryLocation, project));
                List<DeploymentHistoryModel> deploymentHistory = JsonConvert.DeserializeObject<List<DeploymentHistoryModel>>(deploymentHistoryJSONString) ?? [];

                deploymentHistory.Add(deployment);
                deploymentHistory.OrderByDescending(dh => dh.Id);

                deploymentHistoryJSONString = JsonConvert.SerializeObject(deploymentHistory);

                await _FileSystem.WriteAllText(Path.Combine(AppSettings.DeploymentHistoryLocation, project), deploymentHistoryJSONString);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Wrote deployment history for project, {project}.");
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
                    $"Failed to write deployment history for project, {project}");
            }
        }
    }
}
