// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Implementations;

namespace DeploymentManager.PersistenceTests.Implementations
{
    [TestClass]
    public class SystemClockProviderTest
    {
        /// <summary>
        /// Tests whether the UtcNow property returns a DateTime with UTC kind.
        /// </summary>
        [TestMethod]
        public void TestUtcNowReturnsUtcKind()
        {
            SystemClockProvider clock = new();

            DateTime actual = clock.UtcNow;

            Assert.AreEqual(
                DateTimeKind.Utc,
                actual.Kind);
        }

        /// <summary>
        /// Tests whether the DefaultDate property returns the expected date of 1900-01-01 UTC.
        /// </summary>
        [TestMethod]
        public void TestDefaultDateReturnsExpectedDate()
        {
            SystemClockProvider clock = new();

            DateTime expected = new(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            DateTime actual = clock.DefaultDate;

            Assert.AreEqual(
                expected,
                actual);
            Assert.AreEqual(
                DateTimeKind.Utc,
                actual.Kind);
        }

        /// <summary>
        /// Tests whether the DefaultTimeSpan property returns the expected value of zero.
        /// </summary>
        [TestMethod]
        public void TestDefaultTimeSpanReturnsExpectedValue()
        {
            SystemClockProvider clock = new();

            TimeSpan expected = new(0, 0, 0, 0, 0, 0);
            TimeSpan actual = clock.DefaultTimeSpan;

            Assert.AreEqual(
                expected,
                actual);
        }
    }
}
