using DeploymentManager.Abstractions;
using DeploymentManager.Components.Shared;
using DeploymentManager.Models;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Responses;
using DeploymentManager.Models.Responses.Related;
using DeploymentManager.Services;
using Microsoft.AspNetCore.Components;

namespace DeploymentManager.Components.Pages
{
    public partial class Home
    {
        [Inject]
        private GitHubService _GitHubService { get; set; } = default!;
        [Inject]
        private AppSettingsModel AppSettings { get; set; } = default!;

        private ApprovalDialog approvalDialog;
        private string Text;

        private void StartDeployment()
        {
            approvalDialog.Show();
        }

        private async Task HandleApproved()
        {
            ProjectModel project = AppSettings.Projects.First();
            ArtefactListModel? artefactList = await _GitHubService.GetArtefacts(project.GitHub.Repository);

            if (artefactList != null)
            {
                List<ArtefactModel> artefacts = artefactList.Artifacts.FindAll(a => a.Name.Contains(project.GitHub.Artefact));

                if (artefacts.Count > 0)
                {
                    ArtefactModel artefact = artefacts.First();

                    string downloadedFile = await _GitHubService.DownloadArtefact(
                        artefact,
                        project);

                    Text = downloadedFile;
                }
            }
        }

        private async Task HandleCancelled()
        {
            Text = "Cancelled";
        }
    }
}
