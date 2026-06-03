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
        private DocumentService _DocumentService { get; set; } = default!;
        [Inject]
        private IISService _IISService { get; set; } = default!;
        [Inject]
        private AppSettingsModel AppSettings { get; set; } = default!;

        private ApprovalDialog approvalDialog;
        private string Text;
        private string Text2;

        private ArtefactModel Artifact;

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
                    Artifact = artefact;

                    string downloadedFile = await _GitHubService.DownloadArtefact(
                        AppSettings.ArtefactDownloadLocation,
                        artefact,
                        project.GitHub.Repository);

                    Text = downloadedFile;
                }
            }
        }

        private async Task StartExtract()
        {
            if (!string.IsNullOrWhiteSpace(Text))
            {
                ProjectModel project = AppSettings.Projects.First(p => Text.Contains(p.GitHub.Artefact));

                string artefactFile = Path.Combine(AppSettings.ArtefactDownloadLocation, $"{Artifact.Name}.zip");
                string extractedArtefactFile = Path.Combine(AppSettings.ArtefactDownloadLocation, Artifact.Name);

                if (await _DocumentService.ExtractArtefact(
                    artefactFile,
                    extractedArtefactFile))
                {
                    string[] files = await _DocumentService.GetExtractedArtefactFiles(
                        Artifact.Name,
                        extractedArtefactFile);

                    List<(string, KeyValuePair<string, string>)> filesToMove = [];

                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);

                        if (project.Ignore != null)
                        {
                            string? directory = Path.GetDirectoryName(file);

                            if (!project.Ignore.Select(i => i.Name)
                                    .Contains(fileName) && (directory != null && !project.Ignore.Select(i => i.Name)
                                        .Contains(directory)))
                            {
                                string relativePath = Path.GetRelativePath(
                                    extractedArtefactFile,
                                    file);
                                filesToMove.Add(new(
                                    fileName,
                                    new(
                                        file,
                                        $@"C:\{project.Directory}\{relativePath}")));
                            }
                        }

                        else
                        {
                            string relativePath = Path.GetRelativePath(
                                extractedArtefactFile,
                                file);
                            filesToMove.Add(new(
                                fileName,
                                new(
                                    file,
                                    $@"C:\{project.Directory}{relativePath}")));
                        }
                    }

                    if (await _IISService.StopSite(project.Name))
                    {
                        if (await _DocumentService.MoveArtefactFiles(
                            Artifact.Name,
                            $@"C:\{project.Directory}",
                            filesToMove))
                        {
                            if (await _DocumentService.DeleteArtefact(
                                Artifact.Name,
                                artefactFile,
                                extractedArtefactFile))
                            {
                                if (await _IISService.StartSite(project.Name))
                                {
                                    Text2 = "Deployment Complete";
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task HandleCancelled()
        {
            Text = "Cancelled";
        }
    }
}
