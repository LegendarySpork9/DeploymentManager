// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;
using DeploymentManager.Models.Related;

namespace DeploymentManager.UnitTests.Models
{
    [TestClass]
    public class ProjectModelTest
    {
        private static ProjectModel CreateProject(Dictionary<DeploymentEnvironment, ProjectVariablesModel>? variables = null) => new()
        {
            Type = ProjectType.Website,
            Name = "TestProject",
            Directory = @"inetpub\wwwroot\TestProject",
            GitHub = new() { Repository = "test-repo", Artefact = "test-artefact" },
            Variables = variables
        };

        /// <summary>
        /// Tests whether GetServiceName returns the project name when Variables is null.
        /// </summary>
        [TestMethod]
        public void TestGetServiceNameReturnsNameWhenVariablesNull()
        {
            ProjectModel project = CreateProject();

            string result = project.GetServiceName(DeploymentEnvironment.QA);

            Assert.AreEqual(
                "TestProject",
                result);
        }

        /// <summary>
        /// Tests whether GetServiceName returns the project name when the environment is not in Variables.
        /// </summary>
        [TestMethod]
        public void TestGetServiceNameReturnsNameWhenEnvironmentNotInVariables()
        {
            ProjectModel project = CreateProject(new()
            {
                [DeploymentEnvironment.Dev] = new() { ServiceName = "TestProject Dev" }
            });

            string result = project.GetServiceName(DeploymentEnvironment.QA);

            Assert.AreEqual(
                "TestProject",
                result);
        }

        /// <summary>
        /// Tests whether GetServiceName returns the project name when the ServiceName override is null.
        /// </summary>
        [TestMethod]
        public void TestGetServiceNameReturnsNameWhenServiceNameNull()
        {
            ProjectModel project = CreateProject(new()
            {
                [DeploymentEnvironment.QA] = new() { ServiceName = null }
            });

            string result = project.GetServiceName(DeploymentEnvironment.QA);

            Assert.AreEqual(
                "TestProject",
                result);
        }

        /// <summary>
        /// Tests whether GetServiceName returns the override when it is set for the matching environment.
        /// </summary>
        [TestMethod]
        public void TestGetServiceNameReturnsOverrideWhenSet()
        {
            ProjectModel project = CreateProject(new()
            {
                [DeploymentEnvironment.QA] = new() { ServiceName = "TestProject QA" }
            });

            string result = project.GetServiceName(DeploymentEnvironment.QA);

            Assert.AreEqual(
                "TestProject QA",
                result);
        }

        /// <summary>
        /// Tests whether GetDirectory returns the project directory when Variables is null.
        /// </summary>
        [TestMethod]
        public void TestGetDirectoryReturnsDirectoryWhenVariablesNull()
        {
            ProjectModel project = CreateProject();

            string result = project.GetDirectory(DeploymentEnvironment.QA);

            Assert.AreEqual(
                @"inetpub\wwwroot\TestProject",
                result);
        }

        /// <summary>
        /// Tests whether GetDirectory returns the project directory when the environment is not in Variables.
        /// </summary>
        [TestMethod]
        public void TestGetDirectoryReturnsDirectoryWhenEnvironmentNotInVariables()
        {
            ProjectModel project = CreateProject(new()
            {
                [DeploymentEnvironment.Dev] = new() { Directory = @"inetpub\wwwroot\TestProject Dev" }
            });

            string result = project.GetDirectory(DeploymentEnvironment.QA);

            Assert.AreEqual(
                @"inetpub\wwwroot\TestProject",
                result);
        }

        /// <summary>
        /// Tests whether GetDirectory returns the project directory when the Directory override is null.
        /// </summary>
        [TestMethod]
        public void TestGetDirectoryReturnsDirectoryWhenDirectoryNull()
        {
            ProjectModel project = CreateProject(new()
            {
                [DeploymentEnvironment.QA] = new() { Directory = null }
            });

            string result = project.GetDirectory(DeploymentEnvironment.QA);

            Assert.AreEqual(
                @"inetpub\wwwroot\TestProject",
                result);
        }

        /// <summary>
        /// Tests whether GetDirectory returns the override when it is set for the matching environment.
        /// </summary>
        [TestMethod]
        public void TestGetDirectoryReturnsOverrideWhenSet()
        {
            ProjectModel project = CreateProject(new()
            {
                [DeploymentEnvironment.QA] = new() { Directory = @"inetpub\wwwroot\TestProject QA" }
            });

            string result = project.GetDirectory(DeploymentEnvironment.QA);

            Assert.AreEqual(
                @"inetpub\wwwroot\TestProject QA",
                result);
        }
    }
}
