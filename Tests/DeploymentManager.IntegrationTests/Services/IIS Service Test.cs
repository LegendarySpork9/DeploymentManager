// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Shared;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.IntegrationTests.Services
{
    [TestClass]
    public class IISServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IIISClient> _MockIISClient = new();

        /// <summary>
        /// Tests whether the StopSite method returns true when the site is stopped successfully.
        /// </summary>
        [TestMethod]
        public async Task TestStopSiteReturnsTrue()
        {
            _MockIISClient.Setup(iis => iis.StopSite(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DeviceAuthModel?>()))
                .Returns((string?)null);

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            (bool actual, string? errorMessage) = await iisService.StopSite(
                "Test Site",
                "TestDevice");

            Assert.IsTrue(actual);
            Assert.IsNull(errorMessage);

            _MockIISClient.Verify(
                iis => iis.StopSite(
                    "Test Site",
                    "TestDevice",
                    null),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the StopSite method returns true with a warning message when the site was already stopped.
        /// </summary>
        [TestMethod]
        public async Task TestStopSiteReturnsTrueWithWarning()
        {
            _MockIISClient.Setup(iis => iis.StopSite(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DeviceAuthModel?>()))
                .Returns("IIS site 'Test Site' was already stopped");

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            (bool actual, string? errorMessage) = await iisService.StopSite(
                "Test Site",
                "TestDevice");

            Assert.IsTrue(actual);
            Assert.AreEqual(
                "IIS site 'Test Site' was already stopped",
                errorMessage);
        }

        /// <summary>
        /// Tests whether the StopSite method returns false when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestStopSiteException()
        {
            _MockIISClient.Setup(iis => iis.StopSite(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DeviceAuthModel?>()))
                .Throws(new Exception("Site not found"));

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            (bool actual, string? errorMessage) = await iisService.StopSite(
                "Test Site",
                "TestDevice");

            Assert.IsFalse(actual);
            Assert.AreEqual(
                "Site not found",
                errorMessage);
        }

        /// <summary>
        /// Tests whether the StartSite method returns true when the site is started successfully.
        /// </summary>
        [TestMethod]
        public async Task TestStartSiteReturnsTrue()
        {
            _MockIISClient.Setup(iis => iis.StartSite(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DeviceAuthModel?>()));

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            (bool actual, string? errorMessage) = await iisService.StartSite(
                "Test Site",
                "TestDevice");

            Assert.IsTrue(actual);
            Assert.IsNull(errorMessage);

            _MockIISClient.Verify(
                iis => iis.StartSite(
                    "Test Site",
                    "TestDevice",
                    null),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the StartSite method returns false when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestStartSiteException()
        {
            _MockIISClient.Setup(iis => iis.StartSite(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DeviceAuthModel?>()))
                .Throws(new Exception("Site not found"));

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            (bool actual, string? errorMessage) = await iisService.StartSite(
                "Test Site",
                "TestDevice");

            Assert.IsFalse(actual);
            Assert.AreEqual(
                "Site not found",
                errorMessage);
        }
    }
}
