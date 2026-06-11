// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Models.Shared;
using DeploymentManager.Services;
using DeploymentManager.Values;

namespace DeploymentManager.Orchestrators.GitHub
{
    public class DeployReleasesOrchestrator
    {
        private readonly ILoggerService _Logger;
        private readonly IClock _Clock;
        private readonly GitHubService _GitHubService;
        private readonly DocumentService _DocumentService;
        private readonly IISService _IISService;
        private readonly TaskSchedulerService _TaskSchedulerService;

        // Sets the class's global variables.
        public DeployReleasesOrchestrator(
            ILoggerService _logger,
            IClock _clock,
            GitHubService _gitHubService,
            DocumentService _documentService,
            IISService _iisService,
            TaskSchedulerService _taskSchedulerService)
        {
            _Logger = _logger;
            _Clock = _clock;
            _GitHubService = _gitHubService;
            _DocumentService = _documentService;
            _IISService = _iisService;
            _TaskSchedulerService = _taskSchedulerService;
        }

        /// <summary>
        /// Runs a GitHub release asset deployment.
        /// </summary>
        public async Task<DeploymentHistoryModel<AssetModel>> Run(
            DeploymentHistoryModel<AssetModel> deployment,
            string artefactDownloadLocation,
            DeploymentConfigurationModel<AssetModel> deploymentConfiguration,
            Action? onStageUpdated = null)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Starting deployment {deployment.Id} for {deploymentConfiguration.Project.Name} to {deploymentConfiguration.Environment}");

            deployment.Status = Status.Running;
            deployment.StartDate = _Clock.UtcNow;

            onStageUpdated?.Invoke();

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Deployment Start Date: {deployment.StartDate:dd/MM/yyyy hh:mm:ss}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Starting fetch artefact stage for deployment {deployment.Id}");

            List<StageModel> finishedStages = [];
            string? errorMessage = null;
            List<string> errorMessages = [];

            StageModel fetch = deployment.Stages[0];
            fetch.Status = Status.Running;
            fetch.StartDate = _Clock.UtcNow;

            onStageUpdated?.Invoke();

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Fetch Artefact Stage Start Date: {fetch.StartDate:dd/MM/yyyy hh:mm:ss}");

            (string downloadedArtefactFile, errorMessage) = await _GitHubService.DownloadReleaseAsset(
                artefactDownloadLocation,
                deploymentConfiguration.Artefact,
                deploymentConfiguration.Project.GitHub.Repository);

            if (!string.IsNullOrWhiteSpace(downloadedArtefactFile))
            {
                fetch.Status = Status.Completed;
                fetch.EndDate = _Clock.UtcNow;
                fetch.RunTime = fetch.EndDate - fetch.StartDate;
            }

            else
            {
                deployment.FailedAtStage = DeploymentStage.FetchArtefacts;
                fetch.Status = Status.Failed;
                fetch.EndDate = _Clock.UtcNow;
                fetch.RunTime = fetch.EndDate - fetch.StartDate;
                fetch.FailMessages = [errorMessage ?? "No error message received from GitHub Service"];
            }

            onStageUpdated?.Invoke();

            finishedStages.Add(fetch);

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Fetch Artefact Stage Status: {fetch.Status}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Fetch Artefact Stage End Date: {fetch.EndDate:dd/MM/yyyy hh:mm:ss}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Fetch Artefact Stage Run Time: {fetch.RunTime:d\\.hh\\:mm\\:ss\\.fff}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Finished fetch artefact stage for deployment {deployment.Id}");

            string extractedArtefactFile = Path.Combine(
                artefactDownloadLocation,
                Path.GetFileNameWithoutExtension(deploymentConfiguration.Artefact.Name));

            if (finishedStages.All(fs => fs.Status == Status.Completed))
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Starting extract artefact stage for deployment {deployment.Id}");

                StageModel extract = deployment.Stages[1];
                extract.Status = Status.Running;
                extract.StartDate = _Clock.UtcNow;

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Extract Artefact Stage Start Date: {extract.StartDate:dd/MM/yyyy hh:mm:ss}");

                (bool extracted, errorMessage) = await _DocumentService.ExtractArtefact(
                    downloadedArtefactFile,
                    extractedArtefactFile);

                if (extracted)
                {
                    extract.Status = Status.Completed;
                    extract.EndDate = _Clock.UtcNow;
                    extract.RunTime = extract.EndDate - extract.StartDate;
                }

                else
                {
                    deployment.FailedAtStage = DeploymentStage.ExtractArtefacts;
                    extract.Status = Status.Failed;
                    extract.EndDate = _Clock.UtcNow;
                    extract.RunTime = extract.EndDate - extract.StartDate;
                    extract.FailMessages = [errorMessage ?? "No error message received from Document Service"];
                }

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Extract Artefact Stage Status: {extract.Status}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Extract Artefact Stage End Date: {extract.EndDate:dd/MM/yyyy hh:mm:ss}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Extract Artefact Stage Run Time: {extract.RunTime:d\\.hh\\:mm\\:ss\\.fff}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Finished extract artefact stage for deployment {deployment.Id}");

                finishedStages.Add(extract);
            }

            List<DeploymentModel> deploymentsToPerform = deploymentConfiguration.ToDeploymentModel();

            if (!deploymentConfiguration.DeploymentSettings.RunAdditionalDeploys)
            {
                deploymentsToPerform.RemoveRange(
                    1,
                    deploymentsToPerform.Count - 1);
            }

            if (finishedStages.All(fs => fs.Status == Status.Completed))
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Starting fetch artefact files stage for deployment {deployment.Id}");

                StageModel fetchFiles = deployment.Stages[2];
                fetchFiles.Status = Status.Running;
                fetchFiles.StartDate = _Clock.UtcNow;

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Fetch Artefact Files Stage Start Date: {fetchFiles.StartDate:dd/MM/yyyy hh:mm:ss}");

                (string[] files, errorMessage) = await _DocumentService.GetExtractedArtefactFiles(
                        Path.GetFileNameWithoutExtension(deploymentConfiguration.Artefact.Name),
                        extractedArtefactFile);

                List<IgnoreModel> ignore = deploymentConfiguration.Project.Ignore ?? [];

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string? directory = Path.GetDirectoryName(file);

                    if (!ignore.Select(i => i.Name)
                            .Contains(fileName) && (directory != null && !ignore.Select(i => i.Name)
                                .Contains(directory)))
                    {
                        string relativePath = Path.GetRelativePath(
                            extractedArtefactFile,
                            file);

                        foreach (DeploymentModel deploymentToPerform in deploymentsToPerform)
                        {
                            string drive = "C";

                            if (deploymentToPerform.ArtefactDeploymentType == ArtefactFileDeploymentType.Primary)
                            {
                                drive = deploymentConfiguration.PrimaryDeploymentTarget.Drive;
                            }

                            else
                            {
                                drive = deploymentConfiguration.SecondaryDeploymentTargets?.FirstOrDefault(sdt => sdt.Device == deploymentToPerform.Device)?.Drive ?? "C";
                            }

                            deploymentToPerform.ArtefactFiles.Add(new()
                            {
                                Name = fileName,
                                Paths = new(
                                    file,
                                    Path.Combine(
                                        $"{drive}:",
                                        deploymentToPerform.Directory,
                                        relativePath))
                            });
                        }
                    }
                }

                if (deploymentsToPerform.All(df => df.ArtefactFiles.Count > 0))
                {
                    fetchFiles.Status = Status.Completed;
                    fetchFiles.EndDate = _Clock.UtcNow;
                    fetchFiles.RunTime = fetchFiles.EndDate - fetchFiles.StartDate;
                }

                else
                {
                    deployment.FailedAtStage = DeploymentStage.FetchArtefactFiles;
                    fetchFiles.Status = Status.Failed;
                    fetchFiles.EndDate = _Clock.UtcNow;
                    fetchFiles.RunTime = fetchFiles.EndDate - fetchFiles.StartDate;
                    fetchFiles.FailMessages = [errorMessage ?? "No error message received from Document Service"];
                }

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Fetch Artefact Files Stage Status: {fetchFiles.Status}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Fetch Artefact Files Stage End Date: {fetchFiles.EndDate:dd/MM/yyyy hh:mm:ss}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Fetch Artefact Files Stage Run Time: {fetchFiles.RunTime:d\\.hh\\:mm\\:ss\\.fff}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Finished fetch artefact files stage for deployment {deployment.Id}");

                finishedStages.Add(fetchFiles);
            }

            if (finishedStages.All(fs => fs.Status == Status.Completed))
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Starting stop services stage for deployment {deployment.Id}");

                StageModel stopServices = deployment.Stages[3];
                stopServices.Status = Status.Running;
                stopServices.StartDate = _Clock.UtcNow;

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Stop Services Stage Start Date: {stopServices.StartDate:dd/MM/yyyy hh:mm:ss}");

                List<AdditionalDeployModel> secondaryDeploymentTargets = deploymentConfiguration.SecondaryDeploymentTargets ?? [];
                List<bool> success = [];
                List<string> warningMessages = [];

                if (deploymentConfiguration.Project.Type == ProjectType.API || deploymentConfiguration.Project.Type == ProjectType.Website)
                {
                    string device = deploymentConfiguration.PrimaryDeploymentTarget.Device;

                    (bool stopped, string? tempErrorMessage) = await _IISService.StopSite(
                        deploymentConfiguration.Project.Name,
                        device,
                        deploymentConfiguration.PrimaryDeploymentTarget.Auth);

                    if (stopped)
                    {
                        success.Add(true);

                        if (tempErrorMessage != null)
                        {
                            warningMessages.Add($"{device} - {tempErrorMessage}");
                        }
                    }

                    else
                    {
                        success.Add(false);
                        errorMessages.Add(tempErrorMessage == null ? "No error message received from IIS Service" : $"{device} - {tempErrorMessage}");
                    }

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Stopped Service on {device}: {stopped}");

                    foreach (AdditionalDeployModel secondaryDeploymentTarget in secondaryDeploymentTargets)
                    {
                        device = secondaryDeploymentTarget.Device;

                        (stopped, tempErrorMessage) = await _IISService.StopSite(
                            deploymentConfiguration.Project.Name,
                            device,
                            secondaryDeploymentTarget.Auth);

                        if (stopped)
                        {
                            success.Add(true);

                            if (tempErrorMessage != null)
                            {
                                warningMessages.Add($"{device} - {tempErrorMessage}");
                            }
                        }

                        else
                        {
                            success.Add(false);
                            errorMessages.Add(tempErrorMessage == null ? "No error message received from IIS Service" : $"{device} - {tempErrorMessage}");
                        }

                        _Logger.LogMessage(
                            StandardValues.LoggerValues.Debug,
                            $"Stopped Service on {device}: {stopped}");
                    }
                }

                else if (deploymentConfiguration.Project.Type == ProjectType.ConsoleApplication)
                {
                    string device = deploymentConfiguration.PrimaryDeploymentTarget.Device;

                    (bool stopped, string? tempErrorMessage) = await _TaskSchedulerService.StopTask(
                        deploymentConfiguration.Project.Name,
                        device,
                        deploymentConfiguration.PrimaryDeploymentTarget.Auth);

                    if (stopped)
                    {
                        success.Add(true);

                        if (tempErrorMessage != null)
                        {
                            warningMessages.Add($"{device} - {tempErrorMessage}");
                        }
                    }

                    else
                    {
                        success.Add(false);
                        errorMessages.Add(tempErrorMessage == null ? "No error message received from Task Scheduler Service" : $"{device} - {tempErrorMessage}");
                    }

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Stopped Service on {device}: {stopped}");

                    foreach (AdditionalDeployModel secondaryDeploymentTarget in secondaryDeploymentTargets)
                    {
                        device = secondaryDeploymentTarget.Device;

                        (stopped, tempErrorMessage) = await _TaskSchedulerService.StopTask(
                            deploymentConfiguration.Project.Name,
                            device,
                            secondaryDeploymentTarget.Auth);

                        if (stopped)
                        {
                            success.Add(true);

                            if (tempErrorMessage != null)
                            {
                                warningMessages.Add($"{device} - {tempErrorMessage}");
                            }
                        }

                        else
                        {
                            success.Add(false);
                            errorMessages.Add(tempErrorMessage == null ? "No error message received from Task Scheduler Service" : $"{device} - {tempErrorMessage}");
                        }

                        _Logger.LogMessage(
                            StandardValues.LoggerValues.Debug,
                            $"Stopped Service on {device}: {stopped}");
                    }
                }

                if (success.All(s => s))
                {
                    stopServices.Status = warningMessages.Count > 0 ? Status.CompletedWithWarnings : Status.Completed;
                    stopServices.EndDate = _Clock.UtcNow;
                    stopServices.RunTime = stopServices.EndDate - stopServices.StartDate;

                    if (warningMessages.Count > 0)
                    {
                        stopServices.WarningMessages = warningMessages;
                    }
                }

                else
                {
                    deployment.FailedAtStage = DeploymentStage.StopServices;
                    stopServices.Status = Status.Failed;
                    stopServices.EndDate = _Clock.UtcNow;
                    stopServices.RunTime = stopServices.EndDate - stopServices.StartDate;
                    stopServices.FailMessages = errorMessages;
                    errorMessages = [];
                }

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Stop Services Stage Status: {stopServices.Status}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Stop Services Stage End Date: {stopServices.EndDate:dd/MM/yyyy hh:mm:ss}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Stop Services Stage Run Time: {stopServices.RunTime:d\\.hh\\:mm\\:ss\\.fff}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Finished stop services stage for deployment {deployment.Id}");

                finishedStages.Add(stopServices);
            }

            if (finishedStages.All(fs => fs.Status == Status.Completed || fs.Status == Status.CompletedWithWarnings))
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Starting move artefact stage for deployment {deployment.Id}");

                StageModel move = deployment.Stages[4];
                move.Status = Status.Running;
                move.StartDate = _Clock.UtcNow;

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Move Artefact Stage Start Date: {move.StartDate:dd/MM/yyyy hh:mm:ss}");

                List<bool> success = [];

                foreach (DeploymentModel deploymentToPerform in deploymentsToPerform)
                {
                    string drive;

                    if (deploymentToPerform.ArtefactDeploymentType == ArtefactFileDeploymentType.Primary)
                    {
                        drive = deploymentConfiguration.PrimaryDeploymentTarget.Drive;
                    }

                    else
                    {
                        drive = deploymentConfiguration.SecondaryDeploymentTargets?.FirstOrDefault(sdt => sdt.Device == deploymentToPerform.Device)?.Drive ?? "C";
                    }

                    (bool moved, List<string> tempErrorMessages) = await _DocumentService.MoveArtefactFiles(
                        Path.GetFileNameWithoutExtension(deploymentConfiguration.Artefact.Name),
                        Path.Combine(
                            $"{drive}:",
                            deploymentToPerform.Directory),
                        deploymentToPerform.Device ?? deploymentConfiguration.PrimaryDeploymentTarget.Device,
                        deploymentToPerform.ArtefactFiles);

                    success.Add(moved);
                    errorMessages.AddRange(tempErrorMessages);

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Moved files on {deploymentToPerform.Device ?? deploymentConfiguration.PrimaryDeploymentTarget.Device}: {moved}");
                }

                if (success.All(s => s))
                {
                    move.Status = Status.Completed;
                    move.EndDate = _Clock.UtcNow;
                    move.RunTime = move.EndDate - move.StartDate;
                }

                else
                {
                    deployment.FailedAtStage = DeploymentStage.MoveArtefacts;
                    move.Status = Status.Failed;
                    move.EndDate = _Clock.UtcNow;
                    move.RunTime = move.EndDate - move.StartDate;
                    move.FailMessages = errorMessages;
                    errorMessages = [];
                }

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Move Artefact Stage Status: {move.Status}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Move Artefact Stage End Date: {move.EndDate:dd/MM/yyyy hh:mm:ss}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Move Artefact Stage Run Time: {move.RunTime:d\\.hh\\:mm\\:ss\\.fff}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Finished move artefact stage for deployment {deployment.Id}");

                finishedStages.Add(move);
            }

            if (deploymentConfiguration.DeploymentSettings.RestartService && finishedStages.All(fs => fs.Status == Status.Completed || fs.Status == Status.CompletedWithWarnings))
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Starting start services stage for deployment {deployment.Id}");

                StageModel startServices = deployment.Stages[5];
                startServices.Status = Status.Running;
                startServices.StartDate = _Clock.UtcNow;

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Start Services Stage Start Date: {startServices.StartDate:dd/MM/yyyy hh:mm:ss}");

                List<AdditionalDeployModel> secondaryDeploymentTargets = deploymentConfiguration.SecondaryDeploymentTargets ?? [];
                List<bool> success = [];

                if (deploymentConfiguration.Project.Type == ProjectType.API || deploymentConfiguration.Project.Type == ProjectType.Website)
                {
                    string device = deploymentConfiguration.PrimaryDeploymentTarget.Device;

                    (bool started, string? tempErrorMessage) = await _IISService.StartSite(
                        deploymentConfiguration.Project.Name,
                        device,
                        deploymentConfiguration.PrimaryDeploymentTarget.Auth);

                    if (started)
                    {
                        success.Add(true);
                    }

                    else
                    {
                        success.Add(false);
                        errorMessages.Add(tempErrorMessage == null ? "No error message received from IIS Service" : $"{device} - {tempErrorMessage}");
                    }

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Started Service on {device}: {started}");

                    foreach (AdditionalDeployModel secondaryDeploymentTarget in secondaryDeploymentTargets)
                    {
                        device = secondaryDeploymentTarget.Device;

                        (started, tempErrorMessage) = await _IISService.StartSite(
                            deploymentConfiguration.Project.Name,
                            device,
                            secondaryDeploymentTarget.Auth);

                        if (started)
                        {
                            success.Add(true);
                        }

                        else
                        {
                            success.Add(false);
                            errorMessages.Add(tempErrorMessage == null ? "No error message received from IIS Service" : $"{device} - {tempErrorMessage}");
                        }

                        _Logger.LogMessage(
                            StandardValues.LoggerValues.Debug,
                            $"Started Service on {device}: {started}");
                    }
                }

                else if (deploymentConfiguration.Project.Type == ProjectType.ConsoleApplication)
                {
                    string device = deploymentConfiguration.PrimaryDeploymentTarget.Device;

                    (bool started, string? tempErrorMessage) = await _TaskSchedulerService.StartTask(
                        deploymentConfiguration.Project.Name,
                        device,
                        deploymentConfiguration.PrimaryDeploymentTarget.Auth);

                    if (started)
                    {
                        success.Add(true);
                    }

                    else
                    {
                        success.Add(false);
                        errorMessages.Add(tempErrorMessage == null ? "No error message received from Task Scheduler Service" : $"{device} - {tempErrorMessage}");
                    }

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Started Service on {device}: {started}");

                    foreach (AdditionalDeployModel secondaryDeploymentTarget in secondaryDeploymentTargets)
                    {
                        device = secondaryDeploymentTarget.Device;

                        (started, tempErrorMessage) = await _TaskSchedulerService.StartTask(
                            deploymentConfiguration.Project.Name,
                            device,
                            secondaryDeploymentTarget.Auth);

                        if (started)
                        {
                            success.Add(true);
                        }

                        else
                        {
                            success.Add(false);
                            errorMessages.Add(tempErrorMessage == null ? "No error message received from Task Scheduler Service" : $"{device} - {tempErrorMessage}");
                        }

                        _Logger.LogMessage(
                            StandardValues.LoggerValues.Debug,
                            $"Started Service on {device}: {started}");
                    }
                }


                if (success.All(s => s))
                {
                    startServices.Status = Status.Completed;
                    startServices.EndDate = _Clock.UtcNow;
                    startServices.RunTime = startServices.EndDate - startServices.StartDate;
                }

                else
                {
                    deployment.FailedAtStage = DeploymentStage.StartServices;
                    startServices.Status = Status.Failed;
                    startServices.EndDate = _Clock.UtcNow;
                    startServices.RunTime = startServices.EndDate - startServices.StartDate;
                    startServices.FailMessages = errorMessages;
                    errorMessages = [];
                }

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Start Services Stage Status: {startServices.Status}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Start Services Stage End Date: {startServices.EndDate:dd/MM/yyyy hh:mm:ss}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Start Services Stage Run Time: {startServices.RunTime:d\\.hh\\:mm\\:ss\\.fff}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Finished start services stage for deployment {deployment.Id}");

                finishedStages.Add(startServices);
            }

            if (!deploymentConfiguration.DeploymentSettings.RestartService)
            {
                StageModel startServices = deployment.Stages[5];
                startServices.Status = Status.Skipped;

                onStageUpdated?.Invoke();

                finishedStages.Add(startServices);
            }

            _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Starting clean artefacts stage for deployment {deployment.Id}");

            StageModel cleanArtefacts = deployment.Stages[6];
            cleanArtefacts.Status = Status.Running;
            cleanArtefacts.StartDate = _Clock.UtcNow;

            onStageUpdated?.Invoke();

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Clean Artefacts Stage Start Date: {cleanArtefacts.StartDate:dd/MM/yyyy hh:mm:ss}");

            (bool deleted, errorMessage) = await _DocumentService.DeleteArtefact(
                Path.GetFileNameWithoutExtension(deploymentConfiguration.Artefact.Name),
                downloadedArtefactFile,
                extractedArtefactFile);

            if (deleted)
            {
                cleanArtefacts.Status = Status.Completed;
                cleanArtefacts.EndDate = _Clock.UtcNow;
                cleanArtefacts.RunTime = cleanArtefacts.EndDate - cleanArtefacts.StartDate;
            }

            else
            {
                deployment.FailedAtStage = DeploymentStage.CleanArtefacts;
                cleanArtefacts.Status = Status.Failed;
                cleanArtefacts.EndDate = _Clock.UtcNow;
                cleanArtefacts.RunTime = cleanArtefacts.EndDate - cleanArtefacts.StartDate;
                cleanArtefacts.FailMessages = [errorMessage ?? "No error message received from Document Service"];
            }

            onStageUpdated?.Invoke();

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Clean Artefacts Stage Status: {cleanArtefacts.Status}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Clean Artefacts Stage End Date: {cleanArtefacts.EndDate:dd/MM/yyyy hh:mm:ss}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Clean Artefacts Stage Run Time: {cleanArtefacts.RunTime:d\\.hh\\:mm\\:ss\\.fff}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Finished clean artefacts stage for deployment {deployment.Id}");

            finishedStages.Add(cleanArtefacts);

            if (finishedStages.Count == deployment.Stages.Count && finishedStages.All(fs => fs.Status == Status.Completed || fs.Status == Status.CompletedWithWarnings || fs.Status == Status.Skipped))
            {
                deployment.Status = finishedStages.Any(fs => fs.Status == Status.CompletedWithWarnings) ? Status.CompletedWithWarnings : Status.Completed;
                deployment.EndDate = _Clock.UtcNow;
                deployment.RunTime = deployment.EndDate - deployment.StartDate;

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Completed deployment {deployment.Id} for {deploymentConfiguration.Project.Name} to {deploymentConfiguration.Environment}");
            }

            else
            {
                deployment.Status = Status.Failed;
                deployment.EndDate = _Clock.UtcNow;
                deployment.RunTime = deployment.EndDate - deployment.StartDate;

                onStageUpdated?.Invoke();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed deployment {deployment.Id} for {deploymentConfiguration.Project.Name} to {deploymentConfiguration.Environment}");
            }

            return deployment;
        }
    }
}
