// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Implementations;
using DeploymentManager.Models;
using Moq;

namespace DeploymentManager.PersistenceTests.Implementations
{
    [TestClass]
    public class LoginRateLimiterWrapperTest
    {
        private readonly Mock<IClock> _MockClock = new();
        private readonly AppSettingsModel _AppSettings = new()
        {
            MaxLoginAttempts = 5,
            LoginAttemptWindowMinutes = 5,
            LockoutDurationMinutes = 15
        };

        private LoginRateLimiterWrapper CreateWrapper()
        {
            return new LoginRateLimiterWrapper(
                _MockClock.Object,
                _AppSettings);
        }

        /// <summary>
        /// Tests whether CheckLockout returns not locked out when no attempts have been made.
        /// </summary>
        [TestMethod]
        public void TestCheckLockoutNoAttempts()
        {
            LoginRateLimiterWrapper wrapper = CreateWrapper();

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether CheckLockout returns not locked out when given a null IP address.
        /// </summary>
        [TestMethod]
        public void TestCheckLockoutNullIpAddress()
        {
            LoginRateLimiterWrapper wrapper = CreateWrapper();

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout(null!);

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether CheckLockout returns not locked out when given an empty IP address.
        /// </summary>
        [TestMethod]
        public void TestCheckLockoutEmptyIpAddress()
        {
            LoginRateLimiterWrapper wrapper = CreateWrapper();

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("");

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether CheckLockout returns not locked out when given a whitespace IP address.
        /// </summary>
        [TestMethod]
        public void TestCheckLockoutWhitespaceIpAddress()
        {
            LoginRateLimiterWrapper wrapper = CreateWrapper();

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("   ");

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether RecordFailedAttempt does not throw when given a null IP address.
        /// </summary>
        [TestMethod]
        public void TestRecordFailedAttemptNullIpAddress()
        {
            LoginRateLimiterWrapper wrapper = CreateWrapper();

            wrapper.RecordFailedAttempt(null!);

            (bool isLockedOut, _) = wrapper.CheckLockout(null!);
            Assert.IsFalse(isLockedOut);
        }

        /// <summary>
        /// Tests whether RecordFailedAttempt does not throw when given an empty IP address.
        /// </summary>
        [TestMethod]
        public void TestRecordFailedAttemptEmptyIpAddress()
        {
            LoginRateLimiterWrapper wrapper = CreateWrapper();

            wrapper.RecordFailedAttempt("");

            (bool isLockedOut, _) = wrapper.CheckLockout("");
            Assert.IsFalse(isLockedOut);
        }

        /// <summary>
        /// Tests whether a single failed attempt does not trigger a lockout.
        /// </summary>
        [TestMethod]
        public void TestSingleFailedAttemptNoLockout()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            wrapper.RecordFailedAttempt("192.168.1.1");

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether reaching the maximum number of failed attempts triggers a lockout.
        /// </summary>
        [TestMethod]
        public void TestMaxAttemptsTriggersLockout()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsTrue(isLockedOut);
            Assert.AreEqual(
                TimeSpan.FromMinutes(15),
                remainingTime);
        }

        /// <summary>
        /// Tests whether four failed attempts do not trigger a lockout.
        /// </summary>
        [TestMethod]
        public void TestBelowMaxAttemptsNoLockout()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 4; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether the lockout expires after the configured duration.
        /// </summary>
        [TestMethod]
        public void TestLockoutExpiresAfterDuration()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            DateTime afterLockout = now.AddMinutes(16);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(afterLockout);

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether the lockout remaining time decreases over time.
        /// </summary>
        [TestMethod]
        public void TestLockoutRemainingTimeDecreases()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            DateTime fiveMinutesLater = now.AddMinutes(5);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(fiveMinutesLater);

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsTrue(isLockedOut);
            Assert.AreEqual(
                TimeSpan.FromMinutes(10),
                remainingTime);
        }

        /// <summary>
        /// Tests whether ResetAttempts clears the lockout for a given IP address.
        /// </summary>
        [TestMethod]
        public void TestResetAttemptsClearsLockout()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            wrapper.ResetAttempts("192.168.1.1");

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsFalse(isLockedOut);
            Assert.AreEqual(
                TimeSpan.Zero,
                remainingTime);
        }

        /// <summary>
        /// Tests whether ResetAttempts does not affect other IP addresses.
        /// </summary>
        [TestMethod]
        public void TestResetAttemptsDoesNotAffectOtherIPs()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
                wrapper.RecordFailedAttempt("192.168.1.2");
            }

            wrapper.ResetAttempts("192.168.1.1");

            (bool isLockedOutFirst, _) = wrapper.CheckLockout("192.168.1.1");
            (bool isLockedOutSecond, _) = wrapper.CheckLockout("192.168.1.2");

            Assert.IsFalse(isLockedOutFirst);
            Assert.IsTrue(isLockedOutSecond);
        }

        /// <summary>
        /// Tests whether the attempt window resets after the configured duration.
        /// </summary>
        [TestMethod]
        public void TestAttemptWindowResetsAfterExpiry()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 4; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            DateTime afterWindow = now.AddMinutes(6);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(afterWindow);

            for (int i = 0; i < 4; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            (bool isLockedOut, _) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsFalse(isLockedOut);
        }

        /// <summary>
        /// Tests whether different IP addresses are tracked independently.
        /// </summary>
        [TestMethod]
        public void TestDifferentIPsTrackedIndependently()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            wrapper.RecordFailedAttempt("192.168.1.2");

            (bool isLockedOutFirst, _) = wrapper.CheckLockout("192.168.1.1");
            (bool isLockedOutSecond, _) = wrapper.CheckLockout("192.168.1.2");

            Assert.IsTrue(isLockedOutFirst);
            Assert.IsFalse(isLockedOutSecond);
        }

        /// <summary>
        /// Tests whether custom app settings override the default values.
        /// </summary>
        [TestMethod]
        public void TestCustomAppSettingsOverrideDefaults()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            AppSettingsModel customSettings = new()
            {
                MaxLoginAttempts = 3,
                LoginAttemptWindowMinutes = 10,
                LockoutDurationMinutes = 30
            };

            LoginRateLimiterWrapper wrapper = new(
                _MockClock.Object,
                customSettings);

            for (int i = 0; i < 3; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsTrue(isLockedOut);
            Assert.AreEqual(
                TimeSpan.FromMinutes(30),
                remainingTime);
        }

        /// <summary>
        /// Tests whether zero app settings fall back to the default values.
        /// </summary>
        [TestMethod]
        public void TestZeroAppSettingsUseDefaults()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            AppSettingsModel zeroSettings = new()
            {
                MaxLoginAttempts = 0,
                LoginAttemptWindowMinutes = 0,
                LockoutDurationMinutes = 0
            };

            LoginRateLimiterWrapper wrapper = new(
                _MockClock.Object,
                zeroSettings);

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            (bool isLockedOut, TimeSpan remainingTime) = wrapper.CheckLockout("192.168.1.1");

            Assert.IsTrue(isLockedOut);
            Assert.AreEqual(
                TimeSpan.FromMinutes(15),
                remainingTime);
        }

        /// <summary>
        /// Tests whether ResetAttempts does not throw when given a null IP address.
        /// </summary>
        [TestMethod]
        public void TestResetAttemptsNullIpAddress()
        {
            LoginRateLimiterWrapper wrapper = CreateWrapper();

            wrapper.ResetAttempts(null!);

            (bool isLockedOut, _) = wrapper.CheckLockout(null!);
            Assert.IsFalse(isLockedOut);
        }

        /// <summary>
        /// Tests whether a new attempt after lockout expiry starts a fresh window.
        /// </summary>
        [TestMethod]
        public void TestNewAttemptsAfterLockoutExpiry()
        {
            DateTime now = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(now);

            LoginRateLimiterWrapper wrapper = CreateWrapper();

            for (int i = 0; i < 5; i++)
            {
                wrapper.RecordFailedAttempt("192.168.1.1");
            }

            DateTime afterLockout = now.AddMinutes(16);
            _MockClock.Setup(c => c.UtcNow)
                .Returns(afterLockout);

            (bool isLockedOut, _) = wrapper.CheckLockout("192.168.1.1");
            Assert.IsFalse(isLockedOut);

            wrapper.RecordFailedAttempt("192.168.1.1");

            (bool isLockedOutAfter, _) = wrapper.CheckLockout("192.168.1.1");
            Assert.IsFalse(isLockedOutAfter);
        }
    }
}
