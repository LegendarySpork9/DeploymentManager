// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Entities;
using DeploymentManager.Models;
using DeploymentManager.Models.Related;
using DeploymentManager.Services;
using Moq;
using Newtonsoft.Json;

namespace DeploymentManager.PersistenceTests.Services
{
    [TestClass]
    public class ProjectConfigurationServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();

        private AppSettingsModel CreateAppSettings() => new()
        {
            SiteAuth = string.Empty,
            DeploymentHistoryLocation = @"C:\DeployHistory",
            ApprovalCredentialLocation = string.Empty,
            ArtefactDownloadLocation = @"C:\Deploy",
            ProjectConfigurationLocation = @"C:\ProjectConfig",
            Environments =
            [
                new()
                {
                    Device = "TestDevice",
                    Drive = "C",
                    Name = DeploymentEnvironment.Live,
                    ArtefactSource = ArtefactSource.Actions
                }
            ],
            GitHubOptions = new()
            {
                Auth = string.Empty,
                Owner = string.Empty
            },
            Projects =
            [
                new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = "Project",
                    GitHub = new()
                    {
                        Repository = "test-repo",
                        Artefact = "test-artefact"
                    },
                    AdditionalDeploy = null,
                    Ignore = null
                }
            ]
        };

        private static ProjectModel CreateTestProject(string name = "NewProject") => new()
        {
            Type = ProjectType.API,
            Name = name,
            Directory = "NewDirectory",
            GitHub = new()
            {
                Repository = "new-repo",
                Artefact = "new-artefact"
            },
            AdditionalDeploy = null,
            Ignore = null
        };

        /// <summary>
        /// Tests whether GetProjectsAsync returns a list when the file exists.
        /// </summary>
        [TestMethod]
        public async Task TestGetProjectsReturnsListWhenFileExists()
        {
            AppSettingsModel appSettings = CreateAppSettings();

            List<ProjectModel> expected =
            [
                new()
                {
                    Type = ProjectType.Website,
                    Name = "TestProject",
                    Directory = "Project",
                    GitHub = new()
                    {
                        Repository = "test-repo",
                        Artefact = "test-artefact"
                    }
                }
            ];

            string projectsJson = JsonConvert.SerializeObject(expected);

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(projectsJson);

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            List<ProjectModel> actual = await service.GetProjectsAsync();

            Assert.HasCount(
                1,
                actual);
            Assert.AreEqual(
                "TestProject",
                actual[0].Name);
            Assert.AreEqual(
                ProjectType.Website,
                actual[0].Type);
        }

        /// <summary>
        /// Tests whether GetProjectsAsync returns an empty list when the file does not exist.
        /// </summary>
        [TestMethod]
        public async Task TestGetProjectsReturnsEmptyListWhenFileNotFound()
        {
            AppSettingsModel appSettings = CreateAppSettings();

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(false);

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            List<ProjectModel> actual = await service.GetProjectsAsync();

            Assert.HasCount(
                0,
                actual);
        }

        /// <summary>
        /// Tests whether AddProjectAsync writes to the file and updates AppSettings.Projects.
        /// </summary>
        [TestMethod]
        public async Task TestAddProjectWritesToFileAndUpdatesAppSettings()
        {
            AppSettingsModel appSettings = CreateAppSettings();
            ProjectModel newProject = CreateTestProject();

            string existingJson = JsonConvert.SerializeObject(appSettings.Projects);

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(existingJson);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            await service.AddProjectAsync(newProject);

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    Path.Combine(
                        appSettings.ProjectConfigurationLocation,
                        "projects.json"),
                    It.IsAny<string>()),
                Times.Once);

            Assert.HasCount(
                2,
                appSettings.Projects);
            Assert.AreEqual(
                "NewProject",
                appSettings.Projects[1].Name);
        }

        /// <summary>
        /// Tests whether AddProjectAsync rejects a duplicate project name.
        /// </summary>
        [TestMethod]
        public async Task TestAddProjectRejectsDuplicateName()
        {
            AppSettingsModel appSettings = CreateAppSettings();
            ProjectModel duplicateProject = CreateTestProject("TestProject");

            string existingJson = JsonConvert.SerializeObject(appSettings.Projects);

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(existingJson);

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => service.AddProjectAsync(duplicateProject));
        }

        /// <summary>
        /// Tests whether UpdateProjectAsync writes to the file and updates AppSettings.Projects.
        /// </summary>
        [TestMethod]
        public async Task TestUpdateProjectWritesToFileAndUpdatesAppSettings()
        {
            AppSettingsModel appSettings = CreateAppSettings();

            ProjectModel updatedProject = new()
            {
                Type = ProjectType.API,
                Name = "TestProject",
                Directory = "UpdatedDirectory",
                GitHub = new()
                {
                    Repository = "updated-repo",
                    Artefact = "updated-artefact"
                }
            };

            string existingJson = JsonConvert.SerializeObject(appSettings.Projects);

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(existingJson);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            await service.UpdateProjectAsync(
                "TestProject",
                updatedProject);

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    Path.Combine(
                        appSettings.ProjectConfigurationLocation,
                        "projects.json"),
                    It.IsAny<string>()),
                Times.Once);

            Assert.AreEqual(
                "UpdatedDirectory",
                appSettings.Projects[0].Directory);
            Assert.AreEqual(
                ProjectType.API,
                appSettings.Projects[0].Type);
        }

        /// <summary>
        /// Tests whether UpdateProjectAsync rejects a duplicate name when renaming.
        /// </summary>
        [TestMethod]
        public async Task TestUpdateProjectRejectsDuplicateNameWhenRenaming()
        {
            AppSettingsModel appSettings = CreateAppSettings();
            appSettings.Projects.Add(CreateTestProject("SecondProject"));

            string existingJson = JsonConvert.SerializeObject(appSettings.Projects);

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(existingJson);

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            ProjectModel renamedProject = CreateTestProject("SecondProject");

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => service.UpdateProjectAsync(
                    "TestProject",
                    renamedProject));
        }

        /// <summary>
        /// Tests whether DeleteProjectAsync writes to the file and updates AppSettings.Projects.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteProjectWritesToFileAndUpdatesAppSettings()
        {
            AppSettingsModel appSettings = CreateAppSettings();

            string existingJson = JsonConvert.SerializeObject(appSettings.Projects);

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(existingJson);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            await service.DeleteProjectAsync("TestProject");

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    Path.Combine(
                        appSettings.ProjectConfigurationLocation,
                        "projects.json"),
                    It.IsAny<string>()),
                Times.Once);

            Assert.HasCount(
                0,
                appSettings.Projects);
        }

        /// <summary>
        /// Tests whether InitialiseAsync migrates projects from AppSettings when the file does not exist.
        /// </summary>
        [TestMethod]
        public async Task TestInitialiseAsyncMigratesFromAppSettings()
        {
            AppSettingsModel appSettings = CreateAppSettings();

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(false);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            await service.InitialiseAsync();

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    Path.Combine(
                        appSettings.ProjectConfigurationLocation,
                        "projects.json"),
                    It.IsAny<string>()),
                Times.Once);

            Assert.HasCount(
                1,
                appSettings.Projects);
        }

        /// <summary>
        /// Tests whether InitialiseAsync loads from the existing file.
        /// </summary>
        [TestMethod]
        public async Task TestInitialiseAsyncLoadsFromExistingFile()
        {
            AppSettingsModel appSettings = CreateAppSettings();

            List<ProjectModel> fileProjects =
            [
                new()
                {
                    Type = ProjectType.API,
                    Name = "FileProject",
                    Directory = "FileDir",
                    GitHub = new()
                    {
                        Repository = "file-repo",
                        Artefact = "file-artefact"
                    }
                },
                new()
                {
                    Type = ProjectType.ConsoleApplication,
                    Name = "SecondFileProject",
                    Directory = "SecondDir",
                    GitHub = new()
                    {
                        Repository = "second-repo",
                        Artefact = "second-artefact"
                    }
                }
            ];

            string fileJson = JsonConvert.SerializeObject(fileProjects);

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(true);
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(fileJson);

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            await service.InitialiseAsync();

            Assert.HasCount(
                2,
                appSettings.Projects);
            Assert.AreEqual(
                "FileProject",
                appSettings.Projects[0].Name);
            Assert.AreEqual(
                "SecondFileProject",
                appSettings.Projects[1].Name);
        }

        /// <summary>
        /// Tests whether concurrent AddProjectAsync calls produce correct results.
        /// </summary>
        [TestMethod]
        public async Task TestConcurrentAddProjectsProduceCorrectResults()
        {
            AppSettingsModel appSettings = CreateAppSettings();
            appSettings.Projects = [];

            _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                .ReturnsAsync(false);
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Callback<string, string>((path, content) =>
                {
                    _MockFileSystem.Setup(fs => fs.CheckFile(It.IsAny<string>()))
                        .ReturnsAsync(true);
                    _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                        .ReturnsAsync(content);
                });

            ProjectConfigurationService service = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                appSettings);

            int concurrentCalls = 5;

            await Task.WhenAll(
                Enumerable.Range(0, concurrentCalls)
                    .Select(i => service.AddProjectAsync(CreateTestProject($"Project{i}"))));

            Assert.HasCount(
                concurrentCalls,
                appSettings.Projects);
            CollectionAssert.AllItemsAreUnique(
                appSettings.Projects.Select(p => p.Name).ToList());
        }
    }
}
