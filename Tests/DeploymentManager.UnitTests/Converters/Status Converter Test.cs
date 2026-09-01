// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Converters;
using DeploymentManager.Entities;

namespace DeploymentManager.UnitTests.Converters
{
    [TestClass]
    public class StatusConverterTest
    {
        /// <summary>
        /// Tests whether GetStatusBadgeClass returns "bg-secondary" for PendingApproval.
        /// </summary>
        [TestMethod]
        public void TestGetStatusBadgeClassPendingApproval()
        {
            string actual = StatusConverter.GetStatusBadgeClass(Status.PendingApproval);

            Assert.AreEqual(
                "bg-secondary",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusBadgeClass returns "bg-secondary" for NotStarted.
        /// </summary>
        [TestMethod]
        public void TestGetStatusBadgeClassNotStarted()
        {
            string actual = StatusConverter.GetStatusBadgeClass(Status.NotStarted);

            Assert.AreEqual(
                "bg-secondary",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusBadgeClass returns "bg-primary" for Running.
        /// </summary>
        [TestMethod]
        public void TestGetStatusBadgeClassRunning()
        {
            string actual = StatusConverter.GetStatusBadgeClass(Status.Running);

            Assert.AreEqual(
                "bg-primary",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusBadgeClass returns "bg-success" for Completed.
        /// </summary>
        [TestMethod]
        public void TestGetStatusBadgeClassCompleted()
        {
            string actual = StatusConverter.GetStatusBadgeClass(Status.Completed);

            Assert.AreEqual(
                "bg-success",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusBadgeClass returns "bg-warning text-dark" for CompletedWithWarnings.
        /// </summary>
        [TestMethod]
        public void TestGetStatusBadgeClassCompletedWithWarnings()
        {
            string actual = StatusConverter.GetStatusBadgeClass(Status.CompletedWithWarnings);

            Assert.AreEqual(
                "bg-warning text-dark",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusBadgeClass returns "bg-danger" for Failed.
        /// </summary>
        [TestMethod]
        public void TestGetStatusBadgeClassFailed()
        {
            string actual = StatusConverter.GetStatusBadgeClass(Status.Failed);

            Assert.AreEqual(
                "bg-danger",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusBadgeClass returns "badge-skipped" for Skipped.
        /// </summary>
        [TestMethod]
        public void TestGetStatusBadgeClassSkipped()
        {
            string actual = StatusConverter.GetStatusBadgeClass(Status.Skipped);

            Assert.AreEqual(
                "badge-skipped",
                actual);
        }

        /// <summary>
        /// Tests whether GetCardClass returns "card-not-started" for NotStarted.
        /// </summary>
        [TestMethod]
        public void TestGetCardClassNotStarted()
        {
            string actual = StatusConverter.GetCardClass(Status.NotStarted);

            Assert.AreEqual(
                "card-not-started",
                actual);
        }

        /// <summary>
        /// Tests whether GetCardClass returns "card-running" for Running.
        /// </summary>
        [TestMethod]
        public void TestGetCardClassRunning()
        {
            string actual = StatusConverter.GetCardClass(Status.Running);

            Assert.AreEqual(
                "card-running",
                actual);
        }

        /// <summary>
        /// Tests whether GetCardClass returns "card-completed" for Completed.
        /// </summary>
        [TestMethod]
        public void TestGetCardClassCompleted()
        {
            string actual = StatusConverter.GetCardClass(Status.Completed);

            Assert.AreEqual(
                "card-completed",
                actual);
        }

        /// <summary>
        /// Tests whether GetCardClass returns "card-completed-warnings" for CompletedWithWarnings.
        /// </summary>
        [TestMethod]
        public void TestGetCardClassCompletedWithWarnings()
        {
            string actual = StatusConverter.GetCardClass(Status.CompletedWithWarnings);

            Assert.AreEqual(
                "card-completed-warnings",
                actual);
        }

        /// <summary>
        /// Tests whether GetCardClass returns "card-failed" for Failed.
        /// </summary>
        [TestMethod]
        public void TestGetCardClassFailed()
        {
            string actual = StatusConverter.GetCardClass(Status.Failed);

            Assert.AreEqual(
                "card-failed",
                actual);
        }

        /// <summary>
        /// Tests whether GetCardClass returns "card-skipped" for Skipped.
        /// </summary>
        [TestMethod]
        public void TestGetCardClassSkipped()
        {
            string actual = StatusConverter.GetCardClass(Status.Skipped);

            Assert.AreEqual(
                "card-skipped",
                actual);
        }

        /// <summary>
        /// Tests whether GetCardClass returns "card-not-started" for PendingApproval (default case).
        /// </summary>
        [TestMethod]
        public void TestGetCardClassDefault()
        {
            string actual = StatusConverter.GetCardClass(Status.PendingApproval);

            Assert.AreEqual(
                "card-not-started",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusDisplayText returns "Complete" for Completed.
        /// </summary>
        [TestMethod]
        public void TestGetStatusDisplayTextCompleted()
        {
            string actual = StatusConverter.GetStatusDisplayText(Status.Completed);

            Assert.AreEqual(
                "Complete",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusDisplayText returns "Pending Approval" for PendingApproval.
        /// </summary>
        [TestMethod]
        public void TestGetStatusDisplayTextPendingApproval()
        {
            string actual = StatusConverter.GetStatusDisplayText(Status.PendingApproval);

            Assert.AreEqual(
                "Pending Approval",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusDisplayText returns "Not Started" for NotStarted.
        /// </summary>
        [TestMethod]
        public void TestGetStatusDisplayTextNotStarted()
        {
            string actual = StatusConverter.GetStatusDisplayText(Status.NotStarted);

            Assert.AreEqual(
                "Not Started",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusDisplayText returns "Running" for Running.
        /// </summary>
        [TestMethod]
        public void TestGetStatusDisplayTextRunning()
        {
            string actual = StatusConverter.GetStatusDisplayText(Status.Running);

            Assert.AreEqual(
                "Running",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusDisplayText returns "Completed With Warnings" for CompletedWithWarnings.
        /// </summary>
        [TestMethod]
        public void TestGetStatusDisplayTextCompletedWithWarnings()
        {
            string actual = StatusConverter.GetStatusDisplayText(Status.CompletedWithWarnings);

            Assert.AreEqual(
                "Completed With Warnings",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusDisplayText returns "Failed" for Failed.
        /// </summary>
        [TestMethod]
        public void TestGetStatusDisplayTextFailed()
        {
            string actual = StatusConverter.GetStatusDisplayText(Status.Failed);

            Assert.AreEqual(
                "Failed",
                actual);
        }

        /// <summary>
        /// Tests whether GetStatusDisplayText returns "Skipped" for Skipped.
        /// </summary>
        [TestMethod]
        public void TestGetStatusDisplayTextSkipped()
        {
            string actual = StatusConverter.GetStatusDisplayText(Status.Skipped);

            Assert.AreEqual(
                "Skipped",
                actual);
        }

        /// <summary>
        /// Tests whether GetDeploymentTypeBadgeClass returns "bg-info text-dark" for GitHub.
        /// </summary>
        [TestMethod]
        public void TestGetDeploymentTypeBadgeClassGitHub()
        {
            string actual = StatusConverter.GetDeploymentTypeBadgeClass(DeploymentType.GitHub);

            Assert.AreEqual(
                "bg-info text-dark",
                actual);
        }

        /// <summary>
        /// Tests whether GetDeploymentTypeBadgeClass returns "bg-success" for FileUpload.
        /// </summary>
        [TestMethod]
        public void TestGetDeploymentTypeBadgeClassFileUpload()
        {
            string actual = StatusConverter.GetDeploymentTypeBadgeClass(DeploymentType.FileUpload);

            Assert.AreEqual(
                "bg-success",
                actual);
        }

        /// <summary>
        /// Tests whether GetDeploymentTypeBadgeClass returns "bg-secondary" for an unknown value.
        /// </summary>
        [TestMethod]
        public void TestGetDeploymentTypeBadgeClassDefault()
        {
            string actual = StatusConverter.GetDeploymentTypeBadgeClass((DeploymentType)999);

            Assert.AreEqual(
                "bg-secondary",
                actual);
        }
    }
}
