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
        private DeploymentHistoryModel<object>? SelectedDeployment;
        private StageModel? SelectedStage;
        private StageModel? WarningStage;

        protected override void OnInitialized()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Opened Deployment History Page");
        }

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

        private void BackToProjects()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Back to projects clicked");

            SelectedProject = null;
            Deployments = [];
            SelectedDeployment = null;
            SelectedStage = null;
            WarningStage = null;
            CurrentState = PageState.ProjectSelection;
        }

        private void ViewDeploymentDetail(DeploymentHistoryModel<object> deployment)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Viewing deployment detail: {deployment.Id}");

            SelectedDeployment = deployment;
        }

        private void CloseDeploymentDetail()
        {
            SelectedDeployment = null;
            SelectedStage = null;
            WarningStage = null;
        }

        private void ShowErrors(StageModel stage)
        {
            SelectedStage = stage;
        }

        private void CloseErrors()
        {
            SelectedStage = null;
        }

        private void ShowWarnings(StageModel stage)
        {
            WarningStage = stage;
        }

        private void CloseWarnings()
        {
            WarningStage = null;
        }

        private string FormatDateTime(DateTime dateTime)
        {
            return DateTimeConverter.FormatDateTime(dateTime, _Clock.DefaultDate);
        }

        private string FormatRunTimeFriendly(TimeSpan runTime)
        {
            return DateTimeConverter.FormatRunTimeFriendly(runTime, _Clock.DefaultTimeSpan);
        }

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

        private enum PageState
        {
            ProjectSelection,
            Loading,
            HistoryList
        }
    }
}
