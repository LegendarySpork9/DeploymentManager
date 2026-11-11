// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models;
using DeploymentManager.Models.Related;
using DeploymentManager.Values;
using Newtonsoft.Json;

namespace DeploymentManager.Services
{
    public class ProjectConfigurationService
    {
        private readonly ILoggerService _Logger;
        private readonly IFileSystem _FileSystem;
        private readonly AppSettingsModel AppSettings;
        private readonly SemaphoreSlim _ProjectsLock = new(1, 1);

        // Sets the class's global variables.
        public ProjectConfigurationService(
            ILoggerService _logger,
            IFileSystem _fileSystem,
            AppSettingsModel appSettings)
        {
            _Logger = _logger;
            _FileSystem = _fileSystem;
            AppSettings = appSettings;
        }

        /// <summary>
        /// Returns the file path for the projects configuration file.
        /// </summary>
        private string GetFilePath() => Path.Combine(
            AppSettings.ProjectConfigurationLocation,
            "projects.json");

        /// <summary>
        /// Initialises the project configuration from file, migrating from appsettings if needed.
        /// </summary>
        public async Task InitialiseAsync()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Initialising project configuration");

            try
            {
                string filePath = GetFilePath();

                if (!await _FileSystem.CheckFile(filePath))
                {
                    if (AppSettings.Projects != null && AppSettings.Projects.Any())
                    {
                        _Logger.LogMessage(
                            StandardValues.LoggerValues.Info,
                            "Migrating projects from appsettings to projects.json");

                        string projectsJSONString = JsonConvert.SerializeObject(AppSettings.Projects);
                        await _FileSystem.WriteAllText(
                            filePath,
                            projectsJSONString);

                        _Logger.LogMessage(
                            StandardValues.LoggerValues.Debug,
                            $"Migrated {AppSettings.Projects.Count} projects to projects.json");
                    }

                    else
                    {
                        AppSettings.Projects = [];

                        _Logger.LogMessage(
                            StandardValues.LoggerValues.Info,
                            "No existing projects to migrate");
                    }
                }

                else
                {
                    string projectsJSONString = await _FileSystem.ReadAllText(filePath);
                    List<ProjectModel> projects = JsonConvert.DeserializeObject<List<ProjectModel>>(projectsJSONString) ?? [];

                    AppSettings.Projects = projects;

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Loaded {projects.Count} projects from projects.json");
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Failed to initialise project configuration");
            }

            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Initialised project configuration");
        }

        /// <summary>
        /// Returns the list of configured projects.
        /// </summary>
        public async Task<List<ProjectModel>> GetProjectsAsync()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Fetching project configuration");

            List<ProjectModel> projects = [];

            await _ProjectsLock.WaitAsync();

            try
            {
                string filePath = GetFilePath();

                if (!await _FileSystem.CheckFile(filePath))
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        "No project configuration file found");
                }

                else
                {
                    string projectsJSONString = await _FileSystem.ReadAllText(filePath);
                    projects = JsonConvert.DeserializeObject<List<ProjectModel>>(projectsJSONString) ?? [];

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"Fetched {projects.Count} projects.");
                }
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Failed to fetch project configuration");
            }

            finally
            {
                _ProjectsLock.Release();
            }

            return projects;
        }

        /// <summary>
        /// Adds a new project to the configuration.
        /// </summary>
        public async Task AddProjectAsync(ProjectModel project)
        {
            await _ProjectsLock.WaitAsync();

            try
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Adding project, {project.Name}");

                List<ProjectModel> projects = await ReadProjectsFromFile();

                if (projects.Any(p => p.Name.Equals(
                    project.Name,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"A project with the name '{project.Name}' already exists.");
                }

                projects.Add(project);
                await WriteProjectsToFile(projects);
                AppSettings.Projects = projects;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Added project, {project.Name}.");
            }

            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to add project, {project.Name}");

                throw;
            }

            finally
            {
                _ProjectsLock.Release();
            }
        }

        /// <summary>
        /// Updates an existing project in the configuration.
        /// </summary>
        public async Task UpdateProjectAsync(
            string originalName,
            ProjectModel updatedProject)
        {
            await _ProjectsLock.WaitAsync();

            try
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Updating project, {originalName}");

                List<ProjectModel> projects = await ReadProjectsFromFile();

                int index = projects.FindIndex(p => p.Name.Equals(
                    originalName,
                    StringComparison.OrdinalIgnoreCase));

                if (index == -1)
                {
                    throw new InvalidOperationException($"No project with the name '{originalName}' was found.");
                }

                if (!updatedProject.Name.Equals(
                    originalName,
                    StringComparison.OrdinalIgnoreCase) &&
                    projects.Any(p => p.Name.Equals(
                        updatedProject.Name,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"A project with the name '{updatedProject.Name}' already exists.");
                }

                projects[index] = updatedProject;
                await WriteProjectsToFile(projects);
                AppSettings.Projects = projects;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Updated project, {originalName}.");
            }

            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to update project, {originalName}");

                throw;
            }

            finally
            {
                _ProjectsLock.Release();
            }
        }

        /// <summary>
        /// Deletes a project from the configuration.
        /// </summary>
        public async Task DeleteProjectAsync(string projectName)
        {
            await _ProjectsLock.WaitAsync();

            try
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Deleting project, {projectName}");

                List<ProjectModel> projects = await ReadProjectsFromFile();

                int removed = projects.RemoveAll(p => p.Name.Equals(
                    projectName,
                    StringComparison.OrdinalIgnoreCase));

                if (removed == 0)
                {
                    throw new InvalidOperationException($"No project with the name '{projectName}' was found.");
                }

                await WriteProjectsToFile(projects);
                AppSettings.Projects = projects;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Deleted project, {projectName}.");
            }

            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to delete project, {projectName}");

                throw;
            }

            finally
            {
                _ProjectsLock.Release();
            }
        }

        /// <summary>
        /// Reads the projects from the configuration file without acquiring the lock.
        /// </summary>
        private async Task<List<ProjectModel>> ReadProjectsFromFile()
        {
            string filePath = GetFilePath();

            if (!await _FileSystem.CheckFile(filePath))
            {
                return [];
            }

            string projectsJSONString = await _FileSystem.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<ProjectModel>>(projectsJSONString) ?? [];
        }

        /// <summary>
        /// Writes the projects to the configuration file without acquiring the lock.
        /// </summary>
        private async Task WriteProjectsToFile(List<ProjectModel> projects)
        {
            string filePath = GetFilePath();
            string projectsJSONString = JsonConvert.SerializeObject(projects);

            await _FileSystem.WriteAllText(filePath, projectsJSONString);
        }
    }
}
