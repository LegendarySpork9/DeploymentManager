// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Components.Dialogs;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Models.Shared;
using DeploymentManager.Orchestrators;
using DeploymentManager.Services;
using DeploymentManager.Values;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Pages
{
    public partial class DeploymentConfiguration
    {
        [Inject]
        private ILoggerService _Logger { get; set; } = default!;
        [Inject]
        private IClock _Clock { get; set; } = default!;
        [Inject]
        private IServiceProvider _ServiceProvider { get; set; } = default!;
        [Inject]
        private DeploymentHistoryService _DeploymentHistoryService { get; set; } = default!;
        [Inject]
        private GitHubService _GitHubService { get; set; } = default!;
        [Inject]
        private AppSettingsModel AppSettings { get; set; } = default!;

        private DeploymentOrchestrator? _DeploymentOrchestrator;

        private ApprovalDialog ApprovalDialogForm = new();
        private FileUploadDialog FileUploadDialogForm = new();
        private DeploymentConfigurationModel<object>? DeploymentConfig;
        private DeploymentHistoryModel<object>? Deployment;

        private bool IsLoading;
        private bool PerformDeploy;
        private bool FileUploaded;
        private bool IsReadyForDeployment;
        private int FormKey;

        private string ErrorMessage = string.Empty;

        private string UploadButtonText = "Upload File";
        private ProjectModel? Project = null;
        private DeploymentEnvironment? DeployEnvironment = null;
        private EnvironmentModel? Environment = null;
        private DeploymentType? DeploymentType = null;
        private List<object> Artefacts = [];
        private List<string> ArtefactSelect = [];
        private object? Artefact = null;
        private DeploymentSettingsModel DeploymentSettings = new();
        private StageModel? SelectedStage;
        private StageModel? WarningStage;

        /// <summary>
        /// Logs opened page message.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Opened Deployment Configuration Page");

            IsLoading = true;

            _DeploymentOrchestrator = new(
                _Logger,
                _Clock,
                _ServiceProvider,
                _DeploymentHistoryService,
                AppSettings);

            if (!AppSettings.Projects.Any())
            {
                ErrorMessage = "No projects configured, please configure projects in app settings.";
            }

            if (!AppSettings.Environments.Any() && string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ErrorMessage = "No environments configured, please configure projects in app settings.";
            }

            IsLoading = false;
        }

        /// <summary>
        /// Sets the project from the input.
        /// </summary>
        private void SetProject(string project)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Selected Project: {project}");

            Project = AppSettings.Projects.First(p => p.Name == project);
        }

        /// <summary>
        /// Sets the environment from the input.
        /// </summary>
        private void SetEnvironment(string environment)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Selected Environment: {environment}");

            if (environment == "Live")
            {
                DeployEnvironment = DeploymentEnvironment.Live;
            }

            else if (environment == "QA")
            {
                DeployEnvironment = DeploymentEnvironment.QA;
            }

            else
            {
                DeployEnvironment = DeploymentEnvironment.Dev;
            }

            Environment = AppSettings.Environments.First(e => e.Name == DeployEnvironment);
        }

        /// <summary>
        /// Sets the deployment Type from the input.
        /// </summary>
        private async Task SetDeploymentType(string deploymentType)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Selected Deployment Type: {deploymentType}");

            IsLoading = true;

            if (deploymentType == "GitHub")
            {
                DeploymentType = Entities.DeploymentType.GitHub;

                if (Environment != null && Environment.ArtefactSource == ArtefactSource.Actions)
                {
                    ArtefactListModel? artefactList = await _GitHubService.GetArtefacts(Project.GitHub.Repository);

                    if (artefactList != null)
                    {
                        List<ArtefactModel> artefacts = artefactList.Artifacts.FindAll(a => a.Name.Contains(Project.GitHub.Artefact) && a.Archive_Download_Url.Contains("/zip"));

                        Artefacts = [.. artefacts];
                        ArtefactSelect = [.. artefacts.Select(a => a.Name)];
                    }
                }

                else if (Environment != null && Environment.ArtefactSource == ArtefactSource.Releases)
                {
                    List<ReleaseModel>? releases = await _GitHubService.GetReleases(Project.GitHub.Repository);

                    if (releases != null)
                    {
                        List<AssetModel> artefacts = [];

                        foreach (ReleaseModel release in releases)
                        {
                            artefacts.AddRange(release.Assets.FindAll(a => a.Name.Contains(Project.GitHub.Artefact) && a.Name.Contains(".zip")));
                        }

                        Artefacts = [.. artefacts];
                        ArtefactSelect = [.. artefacts.Select(a => a.Name)];
                    }
                }
            }

            else
            {
                DeploymentType = Entities.DeploymentType.FileUpload;
            }

            IsLoading = false;
        }

        /// <summary>
        /// Sets the artefact from the input.
        /// </summary>
        private void SetArtefact(string artefact)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Selected Artefact: {artefact}");

            if (DeploymentType == Entities.DeploymentType.GitHub)
            {
                if (Environment != null && Environment.ArtefactSource == ArtefactSource.Actions)
                {
                    List<ArtefactModel> artefacts = [.. Artefacts.Cast<ArtefactModel>()];

                    Artefact = artefacts.First(a => a.Name == artefact);
                }

                else if (Environment != null && Environment.ArtefactSource == ArtefactSource.Releases)
                {
                    List<AssetModel> artefacts = [.. Artefacts.Cast<AssetModel>()];

                    Artefact = artefacts.First(a => a.Name == artefact);
                }
            }

            IsReadyForDeployment = true;
        }

        /// <summary>
        /// Opens the file upload modal.
        /// </summary>
        private void OpenFileUploadModal()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Upload File Clicked");

            FileUploadDialogForm.Show();
        }

        /// <summary>
        /// Handles the uploaded file from the file upload dialog.
        /// </summary>
        private async Task HandleFileUploaded(UploadFileModel uploadFile)
        {
            Artefact = uploadFile;
            UploadButtonText = $"Uploaded file \"{uploadFile.Name}\"";
            FileUploaded = true;
            IsReadyForDeployment = true;
        }

        /// <summary>
        /// Triggers the deployment configuration generation and approval modal.
        /// </summary>
        private async Task StartDeployment()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Run Additional Deploys: {DeploymentSettings.RunAdditionalDeploys}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Restart Services: {DeploymentSettings.RestartService}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Deploy Clicked");

            IsLoading = true;

            DeploymentConfig = new()
            {
                Type = DeploymentType.Value,
                Environment = DeployEnvironment.Value,
                Project = Project,
                Artefact = Artefact,
                PrimaryDeploymentTarget = Environment,
                SecondaryDeploymentTargets = Project.AdditionalDeploy,
                DeploymentSettings = DeploymentSettings
            };

            if (DeploymentType == Entities.DeploymentType.GitHub)
            {
                if (Environment.ArtefactSource == ArtefactSource.Actions)
                {
                    DeploymentHistoryModel<ArtefactModel> deployment = await _DeploymentOrchestrator.SetUp<ArtefactModel>(DeploymentConfig.ToArtefactDeployment());
                    Deployment = deployment.ToObjectDeployment();
                }

                else if (Environment.ArtefactSource == ArtefactSource.Releases)
                {
                    DeploymentHistoryModel<AssetModel> deployment = await _DeploymentOrchestrator.SetUp<AssetModel>(DeploymentConfig.ToAssetDeployment());
                    Deployment = deployment.ToObjectDeployment();
                }
            }

            else
            {
                DeploymentHistoryModel<UploadFileModel> deployment = await _DeploymentOrchestrator.SetUp<UploadFileModel>(DeploymentConfig.ToUploadDeployment());
                Deployment = deployment.ToObjectDeployment();
            }

            ApprovalDialogForm.Show();
        }

        /// <summary>
        /// Sends the deployment to the deployment orchestrator.
        /// </summary>
        private async Task Deploy()
        {
            PerformDeploy = true;
            Deployment.Status = Status.NotStarted;
            await InvokeAsync(StateHasChanged);

            await Task.Delay(2000);

            Deployment = await _DeploymentOrchestrator.Deploy(
                Deployment,
                DeploymentConfig,
                async () => await InvokeAsync(StateHasChanged));
        }

        /// <summary>
        /// Clears the deployment configuration model and modal.
        /// </summary>
        private async Task Cancel()
        {
            UploadButtonText = "Upload File";
            Project = null;
            DeployEnvironment = null;
            Environment = null;
            DeploymentType = null;
            Artefacts = [];
            Artefact = null;
            DeploymentSettings = new();
            Deployment = null;
            DeploymentConfig = null;

            PerformDeploy = false;
            IsReadyForDeployment = false;
            IsLoading = false;
            FileUploaded = false;
            FormKey++;

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Triggers the clearing of the deployment configuration form.
        /// </summary>
        private async Task HandleDeploymentClose()
        {
            await Cancel();
        }

        /// <summary>
        /// Opens the error model for the given stage.
        /// </summary>
        private void ShowErrors(StageModel stage)
        {
            SelectedStage = stage;
        }

        /// <summary>
        /// Closes the error model for the previously selected stage.
        /// </summary>
        private void CloseErrors()
        {
            SelectedStage = null;
        }

        /// <summary>
        /// Opens the warnings model for the given stage.
        /// </summary>
        private void ShowWarnings(StageModel stage)
        {
            WarningStage = stage;
        }

        /// <summary>
        /// Closes the warnings model for the previously selected stage.
        /// </summary>
        private void CloseWarnings()
        {
            WarningStage = null;
        }
    }
}
