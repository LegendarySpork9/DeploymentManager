using DeploymentManager.Abstractions;
using DeploymentManager.Components.Shared;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Orchestrators;
using DeploymentManager.Services;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Pages
{
    public partial class Home
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

        private ApprovalDialog approvalDialog;
        private DeploymentConfigurationModel<ArtefactModel> DeploymentConfiguration;
        private DeploymentHistoryModel<ArtefactModel> Deployment;
        private string Text;

        private ArtefactModel Artifact;

        private async Task StartDeployment()
        {
            DeploymentOrchestrator _deploymentOrchestrator = new(
            _Logger,
            _Clock,
            _ServiceProvider,
            _DeploymentHistoryService,
            AppSettings
            );

            ProjectModel project = AppSettings.Projects.First();
            ArtefactListModel? artefactList = await _GitHubService.GetArtefacts(project.GitHub.Repository);

            if (artefactList != null)
            {
                List<ArtefactModel> artefacts = artefactList.Artifacts.FindAll(a => a.Name.Contains(project.GitHub.Artefact));

                if (artefacts.Count > 0)
                {
                    ArtefactModel artefact = artefacts.First();
                    DeploymentConfiguration = new()
                    {
                        Type = DeploymentType.GitHub,
                        Environment = DeploymentEnvironment.Live,
                        Project = project,
                        Artefact = artefact,
                        PrimaryDeploymentTarget = AppSettings.Environments[0],
                        SecondaryDeploymentTargets = null,
                        DeploymentSettings = new()
                    };

                    Deployment = await _deploymentOrchestrator.SetUp<ArtefactModel>(DeploymentConfiguration);

                    approvalDialog.Show();
                }
            }
        }

        private async Task HandleApproved()
        {
            Deployment.Status = Status.NotStarted;
            DeploymentOrchestrator _deploymentOrchestrator = new(
            _Logger,
            _Clock,
            _ServiceProvider,
            _DeploymentHistoryService,
            AppSettings
            );
            await _deploymentOrchestrator.Deploy(
                Deployment,
                DeploymentConfiguration);
        }

        private async Task HandleCancelled()
        {
            Text = "Cancelled";
        }
    }
}
