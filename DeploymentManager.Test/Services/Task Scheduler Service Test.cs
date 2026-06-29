// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Services
{
    [TestClass]
    public class TaskSchedulerServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<ITaskScheduler> _MockTaskScheduler = new();

        /// <summary>
        /// Tests whether the StopTask method returns true when the task is stopped successfully.
        /// </summary>
        [TestMethod]
        public async Task TestStopTaskReturnsTrue()
        {
            _MockTaskScheduler.Setup(ts => ts.StopTask(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null))
                .Returns((string?)null);

            TaskSchedulerService taskSchedulerService = new(
                _MockLogger.Object,
                _MockTaskScheduler.Object);

            (bool actual, string? errorMessage) = await taskSchedulerService.StopTask(
                "Test Task",
                "Test Device");

            Assert.IsTrue(actual);
            Assert.IsNull(errorMessage);

            _MockTaskScheduler.Verify(
                ts => ts.StopTask(
                    "Test Task",
                    "Test Device",
                    null),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the StopTask method returns true with a warning message when the task was already stopped.
        /// </summary>
        [TestMethod]
        public async Task TestStopTaskReturnsTrueWithWarning()
        {
            _MockTaskScheduler.Setup(ts => ts.StopTask(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null))
                .Returns("Task 'Test Task' was already stopped");

            TaskSchedulerService taskSchedulerService = new(
                _MockLogger.Object,
                _MockTaskScheduler.Object);

            (bool actual, string? errorMessage) = await taskSchedulerService.StopTask(
                "Test Task",
                "Test Device");

            Assert.IsTrue(actual);
            Assert.AreEqual(
                "Task 'Test Task' was already stopped",
                errorMessage);
        }

        /// <summary>
        /// Tests whether the StopTask method returns false when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestStopTaskException()
        {
            _MockTaskScheduler.Setup(ts => ts.StopTask(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null))
                .Throws(new Exception("Task not found"));

            TaskSchedulerService taskSchedulerService = new(
                _MockLogger.Object,
                _MockTaskScheduler.Object);

            (bool actual, string? errorMessage) = await taskSchedulerService.StopTask(
                "Test Task",
                "Test Device");

            Assert.IsFalse(actual);
            Assert.AreEqual(
                "Task not found",
                errorMessage);
        }

        /// <summary>
        /// Tests whether the StartTask method returns true when the task is started successfully.
        /// </summary>
        [TestMethod]
        public async Task TestStartTaskReturnsTrue()
        {
            _MockTaskScheduler.Setup(ts => ts.StartTask(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null));

            TaskSchedulerService taskSchedulerService = new(
                _MockLogger.Object,
                _MockTaskScheduler.Object);

            (bool actual, string? errorMessage) = await taskSchedulerService.StartTask(
                "Test Task",
                "Test Device");

            Assert.IsTrue(actual);
            Assert.IsNull(errorMessage);

            _MockTaskScheduler.Verify(
                ts => ts.StartTask(
                    "Test Task",
                    "Test Device",
                    null),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the StartTask method returns false when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestStartTaskException()
        {
            _MockTaskScheduler.Setup(ts => ts.StartTask(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null))
                .Throws(new Exception("Task not found"));

            TaskSchedulerService taskSchedulerService = new(
                _MockLogger.Object,
                _MockTaskScheduler.Object);

            (bool actual, string? errorMessage) = await taskSchedulerService.StartTask(
                "Test Task",
                "Test Device");

            Assert.IsFalse(actual);
            Assert.AreEqual(
                "Task not found",
                errorMessage);
        }
    }
}
