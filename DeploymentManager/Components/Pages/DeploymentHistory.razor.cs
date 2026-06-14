// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Converters;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Data.Related;
using DeploymentManager.Models.Related;
using DeploymentManager.Services;
using DeploymentManager.Values;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Pages
{
    public partial class DeploymentHistory
    {
        [Inject]
        private ILoggerService _Logger { get; set; } = default!;
        [Inject]
        private IClock _Clock { get; set; } = default!;
        [Inject]
        private DeploymentHistoryService _DeploymentHistoryService { get; set; } = default!;
        [Inject]
        private AppSettingsModel AppSettings { get; set; } = default!;

        private PageState CurrentState = PageState.ProjectSelection;
        private ProjectModel? SelectedProject;
        private List<DeploymentHistoryModel<object>> Deployments = [];
        private List<DeploymentStatusModel> DeploymentStatuses = [];
        private DeploymentHistoryModel<object>? SelectedDeployment;
        private StageModel? SelectedStage;
        private StageModel? WarningStage;

        /// <summary>
        /// Logs the information page open message and loads deployment statuses.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Opened Deployment History Page");

            DeploymentStatuses = await _DeploymentHistoryService.GetDeploymentStatus();
        }

        /// <summary>
        /// Sets the project selected.
        /// </summary>
        private async Task SelectProject(ProjectModel project)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Selected Project: {project.Name}");

            SelectedProject = project;
            CurrentState = PageState.Loading;

            await InvokeAsync(StateHasChanged);

            Deployments = await _DeploymentHistoryService.GetDeploymentHistory(project.Name);

            CurrentState = PageState.HistoryList;
        }

        /// <summary>
        /// Resets the selected project.
        /// </summary>
        private void BackToProjects()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Back To Projects Clicked");

            SelectedProject = null;
            Deployments = [];
            SelectedDeployment = null;
            SelectedStage = null;
            WarningStage = null;
            CurrentState = PageState.ProjectSelection;
        }

        /// <summary>
        /// Opens the deployment modal for the given deployment.
        /// </summary>
        private void ViewDeployment(DeploymentHistoryModel<object> deployment)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Viewing Deployment: {deployment.Id}");

            SelectedDeployment = deployment;
        }

        /// <summary>
        /// Closes the deployment modal.
        /// </summary>
        private void CloseDeploymentDetail()
        {
            SelectedDeployment = null;
            SelectedStage = null;
            WarningStage = null;
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
        /// Returns the class name for the entry border.
        /// </summary>
        private static string GetEntryBorderClass(Status status)
        {
            return status switch
            {
                Status.Completed => "entry-completed",
                Status.CompletedWithWarnings => "entry-completed-warnings",
                Status.Failed => "entry-failed",
                _ => "entry-default"
            };
        }

        /// <summary>
        /// Returns the deployment status for the given project and environment.
        /// </summary>
        private DeploymentStatusModel? GetStatus(string projectName, DeploymentEnvironment environment)
        {
            return DeploymentStatuses.FirstOrDefault(ds =>
                ds.Project == projectName &&
                ds.Environment == environment);
        }

        /// <summary>
        /// Returns the CSS class for the status dot colour.
        /// </summary>
        private static string GetStatusDotClass(DeploymentStatusModel? status)
        {
            if (status == null) return "status-dot-none";

            return status.Status switch
            {
                Status.Completed => "status-dot-completed",
                Status.CompletedWithWarnings => "status-dot-warnings",
                _ => "status-dot-none"
            };
        }

        /// <summary>
        /// Enums for the various page states.
        /// </summary>
        private enum PageState
        {
            ProjectSelection,
            Loading,
            HistoryList
        }
    }
}
