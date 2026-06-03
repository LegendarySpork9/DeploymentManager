// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Services
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
            _MockIISClient.Setup(iis => iis.StopSite(It.IsAny<string>()));

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            bool actual = await iisService.StopSite("Test Site");

            Assert.IsTrue(actual);

            _MockIISClient.Verify(
                iis => iis.StopSite("Test Site"),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the StopSite method returns false when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestStopSiteException()
        {
            _MockIISClient.Setup(iis => iis.StopSite(It.IsAny<string>()))
                .Throws(new Exception("Site not found"));

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            bool actual = await iisService.StopSite("Test Site");

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether the StartSite method returns true when the site is started successfully.
        /// </summary>
        [TestMethod]
        public async Task TestStartSiteReturnsTrue()
        {
            _MockIISClient.Setup(iis => iis.StartSite(It.IsAny<string>()));

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            bool actual = await iisService.StartSite("Test Site");

            Assert.IsTrue(actual);

            _MockIISClient.Verify(
                iis => iis.StartSite("Test Site"),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the StartSite method returns false when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestStartSiteException()
        {
            _MockIISClient.Setup(iis => iis.StartSite(It.IsAny<string>()))
                .Throws(new Exception("Site not found"));

            IISService iisService = new(
                _MockLogger.Object,
                _MockIISClient.Object);

            bool actual = await iisService.StartSite("Test Site");

            Assert.IsFalse(actual);
        }
    }
}
