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
        private StageModel? SelectedStage;
        private StageModel? WarningStage;

        private string UploadButtonText = "Upload File";
        private ProjectModel? Project = null;
        private DeploymentEnvironment? DeployEnvironment = null;
        private EnvironmentModel? Environment = null;
        private DeploymentType? DeploymentType = null;
        private List<ArtefactModel> Artefacts = [];
        private object? Artefact = null;
        private DeploymentSettingsModel DeploymentSettings = new();

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

                ArtefactListModel? artefactList = await _GitHubService.GetArtefacts(Project.GitHub.Repository);

                if (artefactList != null)
                {
                    Artefacts = artefactList.Artifacts.FindAll(a => a.Name.Contains(Project.GitHub.Artefact));
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

            Artefact = Artefacts.First(a => a.Name == artefact);

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
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"File Uploaded: {uploadFile.Name}");

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

            //ApprovalDialogForm.Show();
            await Deploy();
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

        /// <summary>
        /// Returns the UI friendly stage name for the deployment stages.
        /// </summary>
        private static string FormatStageName(DeploymentStage stage)
        {
            return stage switch
            {
                DeploymentStage.FetchArtefacts => "Fetch Artefacts",
                DeploymentStage.ExtractArtefacts => "Extract Artefacts",
                DeploymentStage.FetchArtefactFiles => "Fetch Artefact Files",
                DeploymentStage.StopServices => "Stop Services",
                DeploymentStage.MoveArtefacts => "Move Artefacts",
                DeploymentStage.StartServices => "Start Services",
                DeploymentStage.CleanArtefacts => "Clean Artefacts",
                _ => stage.ToString()
            };
        }

        /// <summary>
        /// Returns the UI friendly format for the deployment time.
        /// </summary>
        private string FormatTime(DateTime dateTime)
        {
            if (dateTime == _Clock.DefaultDate)
            {
                return "-";
            }

            return dateTime.ToString("HH:mm:ss");
        }

        /// <summary>
        /// Returns the UI friendly format for the run time.
        /// </summary>
        private string FormatRunTime(TimeSpan runTime)
        {
            if (runTime == _Clock.DefaultTimeSpan)
            {
                return "-";
            }

            if (runTime.TotalMinutes >= 1)
            {
                return runTime.ToString(@"m\:ss\.fff");
            }

            return runTime.ToString(@"s\.fff") + "s";
        }

        /// <summary>
        /// Returns the status class for the given status.
        /// </summary>
        private static string GetStatusBadgeClass(Status status)
        {
            return status switch
            {
                Status.PendingApproval => "bg-secondary",
                Status.NotStarted => "bg-secondary",
                Status.Running => "bg-primary",
                Status.Completed => "bg-success",
                Status.CompletedWithWarnings => "bg-warning text-dark",
                Status.Failed => "bg-danger",
                Status.Skipped => "badge-skipped",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Returns the card status class for the given status.
        /// </summary>
        private static string GetCardClass(Status status)
        {
            return status switch
            {
                Status.NotStarted => "card-not-started",
                Status.Running => "card-running",
                Status.Completed => "card-completed",
                Status.CompletedWithWarnings => "card-completed-warnings",
                Status.Failed => "card-failed",
                Status.Skipped => "card-skipped",
                _ => "card-not-started"
            };
        }

        /// <summary>
        /// Returns the status class for the card.
        /// </summary>
        private string GetOverallStatusClass()
        {
            if (Deployment == null)
            {
                return "bg-secondary";
            }

            return GetStatusBadgeClass(Deployment.Status);
        }
    }
}
