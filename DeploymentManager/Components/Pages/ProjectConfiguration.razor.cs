// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Related;
using DeploymentManager.Models.Shared;
using DeploymentManager.Services;
using DeploymentManager.Values;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace DeploymentManager.Components.Pages
{
    public partial class ProjectConfiguration
    {
        [Inject]
        private ILoggerService _Logger { get; set; } = default!;
        [Inject]
        private ProjectConfigurationService ProjectConfigService { get; set; } = default!;
        [Inject]
        private AppSettingsModel AppSettings { get; set; } = default!;
        [Inject]
        private ProtectedSessionStorage SessionStorage { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        private List<ProjectModel> Projects = [];

        private bool IsLoading;
        private bool ShowForm;
        private bool IsAdding;
        private bool ShowDeleteConfirmation;

        private string ErrorMessage = string.Empty;
        private string SuccessMessage = string.Empty;

        private string? OriginalProjectName;
        private string? DeleteTargetName;
        private string FormName = string.Empty;
        private ProjectType FormType = ProjectType.Website;
        private string FormDirectory = string.Empty;
        private string FormGitHubRepository = string.Empty;
        private string FormGitHubArtefact = string.Empty;
        private List<AdditionalDeployModel> FormAdditionalDeploys = [];
        private List<IgnoreModel> FormIgnoreRules = [];
        private Dictionary<DeploymentEnvironment, ProjectVariablesModel> FormVariables = new()
        {
            { DeploymentEnvironment.Live, new() },
            { DeploymentEnvironment.QA, new() },
            { DeploymentEnvironment.Dev, new() }
        };

        /// <summary>
        /// Checks authentication and loads projects.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            ProtectedBrowserStorageResult<bool> loggedInResult = await SessionStorage.GetAsync<bool>("userLoggedIn");

            if (!loggedInResult.Success || !loggedInResult.Value)
            {
                Navigation.NavigateTo(
                    "/login",
                    true);

                return;
            }

            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Opened Project Configuration Page");

            IsLoading = true;
            Projects = [.. AppSettings.Projects];
            IsLoading = false;
        }

        /// <summary>
        /// Opens the form for adding a new project.
        /// </summary>
        private void StartAdd()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Add Project Clicked");

            IsAdding = true;
            OriginalProjectName = null;
            ClearForm();
            ShowForm = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        /// <summary>
        /// Opens the form for editing an existing project.
        /// </summary>
        private void StartEdit(ProjectModel project)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Edit Project Clicked: {project.Name}");

            IsAdding = false;
            OriginalProjectName = project.Name;

            FormName = project.Name;
            FormType = project.Type;
            FormDirectory = project.Directory;
            FormGitHubRepository = project.GitHub.Repository;
            FormGitHubArtefact = project.GitHub.Artefact;
            FormAdditionalDeploys = project.AdditionalDeploy != null ? [.. project.AdditionalDeploy.Select(a => new AdditionalDeployModel
            {
                Device = a.Device,
                Drive = a.Drive,
                Directory = a.Directory,
                Auth = a.Auth != null ? new DeviceAuthModel
                {
                    Domain = a.Auth.Domain,
                    Username = a.Auth.Username,
                    Password = a.Auth.Password
                } : null
            })] : [];
            FormIgnoreRules = project.Ignore != null ? [.. project.Ignore.Select(i => new IgnoreModel
            {
                Type = i.Type,
                Name = i.Name
            })] : [];
            FormVariables = new()
            {
                { DeploymentEnvironment.Live, new() },
                { DeploymentEnvironment.QA, new() },
                { DeploymentEnvironment.Dev, new() }
            };

            if (project.Variables != null)
            {
                foreach (KeyValuePair<DeploymentEnvironment, ProjectVariablesModel> kvp in project.Variables)
                {
                    FormVariables[kvp.Key] = new ProjectVariablesModel
                    {
                        ServiceName = kvp.Value.ServiceName,
                        Directory = kvp.Value.Directory
                    };
                }
            }

            ShowForm = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        /// <summary>
        /// Closes the add/edit form.
        /// </summary>
        private void CancelEdit()
        {
            ShowForm = false;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Validates and saves the project.
        /// </summary>
        private async Task HandleSave()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Save Project Clicked");

            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FormName))
            {
                ErrorMessage = "Project name is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(FormDirectory))
            {
                ErrorMessage = "Directory is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(FormGitHubRepository))
            {
                ErrorMessage = "GitHub repository is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(FormGitHubArtefact))
            {
                ErrorMessage = "GitHub artefact name is required.";
                return;
            }

            IsLoading = true;

            try
            {
                Dictionary<DeploymentEnvironment, ProjectVariablesModel>? variables = FormVariables.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value.ServiceName) || !string.IsNullOrWhiteSpace(kvp.Value.Directory))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (variables.Count == 0)
                {
                    variables = null;
                }

                ProjectModel project = new()
                {
                    Type = FormType,
                    Name = FormName.Trim(),
                    Directory = FormDirectory.Trim(),
                    GitHub = new GitHubModel
                    {
                        Repository = FormGitHubRepository.Trim(),
                        Artefact = FormGitHubArtefact.Trim()
                    },
                    AdditionalDeploy = FormAdditionalDeploys.Count > 0 ? FormAdditionalDeploys : null,
                    Ignore = FormIgnoreRules.Count > 0 ? FormIgnoreRules : null,
                    Variables = variables
                };

                if (IsAdding)
                {
                    await ProjectConfigService.AddProjectAsync(project);

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Project '{project.Name}' added successfully.");

                    Projects = [.. AppSettings.Projects];
                    ShowForm = false;

                    await ShowSuccessMessage($"Project '{project.Name}' added successfully.");
                }

                else
                {
                    await ProjectConfigService.UpdateProjectAsync(OriginalProjectName!, project);

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Project '{project.Name}' updated successfully.");

                    Projects = [.. AppSettings.Projects];
                    ShowForm = false;

                    await ShowSuccessMessage($"Project '{project.Name}' updated successfully.");
                }
            }

            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
            }

            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while saving the project. Please try again.";
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            IsLoading = false;
        }

        /// <summary>
        /// Shows the delete confirmation for the given project.
        /// </summary>
        private void ConfirmDelete(string projectName)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Delete Project Clicked: {projectName}");

            DeleteTargetName = projectName;
            ShowDeleteConfirmation = true;
            SuccessMessage = string.Empty;
        }

        /// <summary>
        /// Deletes the project.
        /// </summary>
        private async Task HandleDelete()
        {
            if (string.IsNullOrEmpty(DeleteTargetName))
            {
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                await ProjectConfigService.DeleteProjectAsync(DeleteTargetName);

                string deletedName = DeleteTargetName;
                Projects = [.. AppSettings.Projects];

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Project '{deletedName}' deleted successfully.");

                ShowDeleteConfirmation = false;
                DeleteTargetName = null;
                IsLoading = false;

                await ShowSuccessMessage($"Project '{deletedName}' deleted successfully.");

                return;
            }

            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while deleting the project. Please try again.";
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            ShowDeleteConfirmation = false;
            DeleteTargetName = null;
            IsLoading = false;
        }

        /// <summary>
        /// Cancels the delete confirmation.
        /// </summary>
        private void CancelDelete()
        {
            ShowDeleteConfirmation = false;
            DeleteTargetName = null;
        }

        /// <summary>
        /// Adds a new additional deploy entry to the form.
        /// </summary>
        private void AddAdditionalDeploy()
        {
            FormAdditionalDeploys.Add(new AdditionalDeployModel
            {
                Device = string.Empty,
                Drive = string.Empty,
                Directory = string.Empty
            });
        }

        /// <summary>
        /// Removes an additional deploy entry from the form.
        /// </summary>
        private void RemoveAdditionalDeploy(int index)
        {
            if (index >= 0 && index < FormAdditionalDeploys.Count)
            {
                FormAdditionalDeploys.RemoveAt(index);
            }
        }

        /// <summary>
        /// Adds a new ignore rule entry to the form.
        /// </summary>
        private void AddIgnoreRule()
        {
            FormIgnoreRules.Add(new IgnoreModel
            {
                Type = IgnoreType.File,
                Name = string.Empty
            });
        }

        /// <summary>
        /// Removes an ignore rule entry from the form.
        /// </summary>
        private void RemoveIgnoreRule(int index)
        {
            if (index >= 0 && index < FormIgnoreRules.Count)
            {
                FormIgnoreRules.RemoveAt(index);
            }
        }

        /// <summary>
        /// Shows a success message that auto-dismisses after a delay.
        /// </summary>
        private async Task ShowSuccessMessage(string message)
        {
            SuccessMessage = message;
            await InvokeAsync(StateHasChanged);

            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                SuccessMessage = string.Empty;
                await InvokeAsync(StateHasChanged);
            });
        }

        /// <summary>
        /// Clears the form fields.
        /// </summary>
        private void ClearForm()
        {
            FormName = string.Empty;
            FormType = ProjectType.Website;
            FormDirectory = string.Empty;
            FormGitHubRepository = string.Empty;
            FormGitHubArtefact = string.Empty;
            FormAdditionalDeploys = [];
            FormIgnoreRules = [];
            FormVariables = new()
            {
                { DeploymentEnvironment.Live, new() },
                { DeploymentEnvironment.QA, new() },
                { DeploymentEnvironment.Dev, new() }
            };
        }
    }
}
