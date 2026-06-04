// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Orchestrators.GitHub;
using DeploymentManager.Services;
using DeploymentManager.Values;

namespace DeploymentManager.Orchestrators
{
    public class DeploymentOrchestrator
    {
        private readonly ILoggerService _Logger;
        private readonly IClock _Clock;
        private readonly IServiceProvider _ServiceProvider;
        private readonly DeploymentHistoryService _DeploymentHistoryService;
        private readonly AppSettingsModel AppSettings;

        // Sets the class's global variables.
        public DeploymentOrchestrator(
            ILoggerService _logger,
            IClock _clock,
            IServiceProvider _serviceProvider,
            DeploymentHistoryService _deploymentHistoryService,
            AppSettingsModel appSettings)
        {
            _Logger = _logger;
            _Clock = _clock;
            _ServiceProvider = _serviceProvider;
            _DeploymentHistoryService = _deploymentHistoryService;
            AppSettings = appSettings;
        }

        /// <summary>
        /// Sets up the deployment.
        /// </summary>
        public async Task<DeploymentHistoryModel<T>> SetUp<T>(DeploymentConfigurationModel<T> deploymentConfiguration)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Setting up deployment for {deploymentConfiguration.Project.Name} to {deploymentConfiguration.Environment}");

            List<DeploymentHistoryModel<object>> deploymentHistory = await _DeploymentHistoryService.GetDeploymentHistory(deploymentConfiguration.Project.Name);

            int lastId = deploymentHistory.Count == 0 ? 0 : deploymentHistory.OrderByDescending(dh => dh.Id)
                .First().Id;

            ArtefactType artefactType = typeof(T) switch
            {
                Type t when t == typeof(ArtefactModel) => ArtefactType.Artefact,
                Type t when t == typeof(AssetModel) => ArtefactType.ReleaseAsset,
                Type t when t == typeof(UploadFileModel) => ArtefactType.Upload,
                _ => throw new ArgumentException($"Unsupported artefact type: {typeof(T).Name}")
            };

            long artefactId = 0;
            string artefactName = string.Empty;
            long artefactSize = 0;
            string branchId = string.Empty;
            string branchName = string.Empty;

            if (artefactType == ArtefactType.Artefact)
            {
                ArtefactModel? artefact = deploymentConfiguration.Artefact as ArtefactModel;

                if (artefact != null)
                {
                    artefactId = artefact.Id;
                    artefactName = artefact.Name;
                    artefactSize = artefact.Size_in_Bytes;
                    branchId = artefact.Workflow_Run.Head_Sha;
                    branchName = artefact.Workflow_Run.Head_Branch;
                }
            }

            else if (artefactType == ArtefactType.ReleaseAsset)
            {
                AssetModel? asset = deploymentConfiguration.Artefact as AssetModel;

                if (asset != null)
                {
                    artefactId = asset.Id;
                    artefactName = asset.Name;
                    artefactSize = asset.Size;
                    branchId = "main";
                    branchName = "main";
                }
            }

            else
            {
                UploadFileModel? uploadFile = deploymentConfiguration.Artefact as UploadFileModel;

                if (uploadFile != null)
                {
                    artefactId = uploadFile.Id;
                    artefactName = uploadFile.Name;
                    artefactSize = uploadFile.Size;
                    branchId = uploadFile.BranchId;
                    branchName = uploadFile.BranchName;
                }
            }

            List<StageModel> stages = [];

            if (artefactType  == ArtefactType.Upload)
            {
                stages =
                [
                    new()
                    {
                        Name = DeploymentStage.ExtractArtefacts,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.FetchArtefactFiles,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.StopServices,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.MoveArtefacts,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.StartServices,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.CleanArtefacts,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    }
                ];
            }

            else
            {
                stages =
                [
                    new()
                    {
                        Name = DeploymentStage.FetchArtefacts,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.ExtractArtefacts,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.FetchArtefactFiles,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.StopServices,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.MoveArtefacts,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.StartServices,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    },
                    new()
                    {
                        Name = DeploymentStage.CleanArtefacts,
                        Status = Status.NotStarted,
                        StartDate = _Clock.DefaultDate,
                        EndDate = _Clock.DefaultDate,
                        RunTime = _Clock.DefaultTimeSpan
                    }
                ];
            }

            DeploymentHistoryModel<T> deployment = new()
            {
                Id = ++lastId,
                Type = deploymentConfiguration.Type,
                ArtefactType = artefactType,
                Status = Status.PendingApproval,
                StartDate = _Clock.DefaultDate,
                EndDate = _Clock.DefaultDate,
                RunTime = _Clock.DefaultTimeSpan,
                ArtefactId = artefactId,
                ArtefactName = artefactName,
                ArtefactSize = artefactSize,
                BranchId = branchId,
                BranchName = branchName,
                DeploymentConfiguration = deploymentConfiguration,
                Stages = stages
            };

            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Set up deployment for {deploymentConfiguration.Project.Name} to {deploymentConfiguration.Environment}");
            return deployment;
        }

        /// <summary>
        /// Manages the deployment.
        /// </summary>
        public async Task<DeploymentHistoryModel<T>> Deploy<T>(
            DeploymentHistoryModel<T> deployment,
            DeploymentConfigurationModel<T> deploymentConfiguration)
        {
            if (deployment.Type == DeploymentType.GitHub)
            {
                if (deploymentConfiguration.PrimaryDeploymentTarget.ArtefactSource == ArtefactSource.Actions)
                {
                    DeployActionsOrchestrator _deployActionsOrchestrator = ActivatorUtilities.CreateInstance<DeployActionsOrchestrator>(_ServiceProvider);

                    DeploymentHistoryModel<ArtefactModel> history = (DeploymentHistoryModel<ArtefactModel>)(object)deployment;
                    DeploymentConfigurationModel<ArtefactModel> config = (DeploymentConfigurationModel<ArtefactModel>)(object)deploymentConfiguration;

                    deployment = (DeploymentHistoryModel<T>)(object)await _deployActionsOrchestrator.Run(
                        history,
                        AppSettings.ArtefactDownloadLocation,
                        config);
                }

                else if (deploymentConfiguration.PrimaryDeploymentTarget.ArtefactSource == ArtefactSource.Releases)
                {
                    DeployReleasesOrchestrator _deployReleaseOrchestrator = ActivatorUtilities.CreateInstance<DeployReleasesOrchestrator>(_ServiceProvider);

                    DeploymentHistoryModel<AssetModel> history = (DeploymentHistoryModel<AssetModel>)(object)deployment;
                    DeploymentConfigurationModel<AssetModel> config = (DeploymentConfigurationModel<AssetModel>)(object)deploymentConfiguration;

                    deployment = (DeploymentHistoryModel<T>)(object)await _deployReleaseOrchestrator.Run(
                        history,
                        AppSettings.ArtefactDownloadLocation,
                        config);
                }
            }

            else
            {
                DeployUploadOrchestrator _deployUploadOrchestrator = ActivatorUtilities.CreateInstance<DeployUploadOrchestrator>(_ServiceProvider);

                DeploymentHistoryModel<UploadFileModel> history = (DeploymentHistoryModel<UploadFileModel>)(object)deployment;
                DeploymentConfigurationModel<UploadFileModel> config = (DeploymentConfigurationModel<UploadFileModel>)(object)deploymentConfiguration;

                deployment = (DeploymentHistoryModel<T>)(object)await _deployUploadOrchestrator.Run(
                    history,
                    AppSettings.ArtefactDownloadLocation,
                    config);
            }

            await _DeploymentHistoryService.WriteDeploymentHistory(
                deploymentConfiguration.Project.Name,
                deployment);

            return deployment;
        }
    }
}
