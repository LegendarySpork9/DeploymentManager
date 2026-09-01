// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Converters;

namespace DeploymentManager.UnitTests.Converters
{
    [TestClass]
    public class DateTimeConverterTest
    {
        private readonly DateTime DefaultDate = new(1900, 1, 1);
        private readonly TimeSpan DefaultTimeSpan = TimeSpan.FromSeconds(-1);

        /// <summary>
        /// Tests whether FormatTime returns "-" when given the default date.
        /// </summary>
        [TestMethod]
        public void TestFormatTimeDefault()
        {
            string actual = DateTimeConverter.FormatTime(
                DefaultDate,
                DefaultDate);

            Assert.AreEqual(
                "-",
                actual);
        }

        /// <summary>
        /// Tests whether FormatTime returns the HH:mm:ss format for a valid date.
        /// </summary>
        [TestMethod]
        public void TestFormatTimeValid()
        {
            DateTime dateTime = new(2026, 6, 2, 14, 30, 45);
            string actual = DateTimeConverter.FormatTime(
                dateTime,
                DefaultDate);

            Assert.AreEqual(
                "14:30:45.000",
                actual);
        }

        /// <summary>
        /// Tests whether FormatDateTime returns "-" when given the default date.
        /// </summary>
        [TestMethod]
        public void TestFormatDateTimeDefault()
        {
            string actual = DateTimeConverter.FormatDateTime(
                DefaultDate,
                DefaultDate);

            Assert.AreEqual(
                "-",
                actual);
        }

        /// <summary>
        /// Tests whether FormatDateTime returns the dd MMM yyyy HH:mm:ss format for a valid date.
        /// </summary>
        [TestMethod]
        public void TestFormatDateTimeValid()
        {
            DateTime dateTime = new(2026, 6, 2, 4, 16, 37);
            string actual = DateTimeConverter.FormatDateTime(
                dateTime,
                DefaultDate);

            Assert.AreEqual(
                "02 Jun 2026 04:16:37",
                actual);
        }

        /// <summary>
        /// Tests whether FormatRunTime returns "-" when given the default timespan.
        /// </summary>
        [TestMethod]
        public void TestFormatRunTimeDefault()
        {
            string actual = DateTimeConverter.FormatRunTime(
                DefaultTimeSpan,
                DefaultTimeSpan);

            Assert.AreEqual(
                "-",
                actual);
        }

        /// <summary>
        /// Tests whether FormatRunTime returns m:ss.fff format when duration is one minute or more.
        /// </summary>
        [TestMethod]
        public void TestFormatRunTimeMinutes()
        {
            TimeSpan runTime = new(0, 0, 2, 15, 123);
            string actual = DateTimeConverter.FormatRunTime(
                runTime,
                DefaultTimeSpan);

            Assert.AreEqual(
                "2:15.123",
                actual);
        }

        /// <summary>
        /// Tests whether FormatRunTime returns s.fff format with "s" suffix when duration is under one minute.
        /// </summary>
        [TestMethod]
        public void TestFormatRunTimeSeconds()
        {
            TimeSpan runTime = new(0, 0, 0, 45, 678);
            string actual = DateTimeConverter.FormatRunTime(
                runTime,
                DefaultTimeSpan);

            Assert.AreEqual(
                "45.678s",
                actual);
        }

        /// <summary>
        /// Tests whether FormatRunTimeFriendly returns "-" when given the default timespan.
        /// </summary>
        [TestMethod]
        public void TestFormatRunTimeFriendlyDefault()
        {
            string actual = DateTimeConverter.FormatRunTimeFriendly(
                DefaultTimeSpan,
                DefaultTimeSpan);

            Assert.AreEqual(
                "-",
                actual);
        }

        /// <summary>
        /// Tests whether FormatRunTimeFriendly returns hours and minutes format for durations of one hour or more.
        /// </summary>
        [TestMethod]
        public void TestFormatRunTimeFriendlyHours()
        {
            TimeSpan runTime = new(1, 30, 0);
            string actual = DateTimeConverter.FormatRunTimeFriendly(
                runTime,
                DefaultTimeSpan);

            Assert.AreEqual(
                "1h 30m",
                actual);
        }

        /// <summary>
        /// Tests whether FormatRunTimeFriendly returns minutes and seconds format for durations of one minute or more.
        /// </summary>
        [TestMethod]
        public void TestFormatRunTimeFriendlyMinutes()
        {
            TimeSpan runTime = new(0, 10, 15);
            string actual = DateTimeConverter.FormatRunTimeFriendly(
                runTime,
                DefaultTimeSpan);

            Assert.AreEqual(
                "10m 15s",
                actual);
        }

        /// <summary>
        /// Tests whether FormatRunTimeFriendly returns seconds format for durations under one minute.
        /// </summary>
        [TestMethod]
        public void TestFormatRunTimeFriendlySeconds()
        {
            TimeSpan runTime = new(0, 0, 45);
            string actual = DateTimeConverter.FormatRunTimeFriendly(
                runTime,
                DefaultTimeSpan);

            Assert.AreEqual(
                "45s",
                actual);
        }

        /// <summary>
        /// Tests whether FormatDateGroupHeader returns the correct ordinal suffix for "st" days.
        /// </summary>
        [TestMethod]
        public void TestFormatDateGroupHeaderSt()
        {
            DateTime date = new(2026, 5, 1);
            string actual = DateTimeConverter.FormatDateGroupHeader(date);

            Assert.AreEqual(
                "1st May 2026",
                actual);
        }

        /// <summary>
        /// Tests whether FormatDateGroupHeader returns the correct ordinal suffix for "nd" days.
        /// </summary>
        [TestMethod]
        public void TestFormatDateGroupHeaderNd()
        {
            DateTime date = new(2026, 5, 2);
            string actual = DateTimeConverter.FormatDateGroupHeader(date);

            Assert.AreEqual(
                "2nd May 2026",
                actual);
        }

        /// <summary>
        /// Tests whether FormatDateGroupHeader returns the correct ordinal suffix for "rd" days.
        /// </summary>
        [TestMethod]
        public void TestFormatDateGroupHeaderRd()
        {
            DateTime date = new(2026, 5, 3);
            string actual = DateTimeConverter.FormatDateGroupHeader(date);

            Assert.AreEqual(
                "3rd May 2026",
                actual);
        }

        /// <summary>
        /// Tests whether FormatDateGroupHeader returns the correct ordinal suffix for "th" days.
        /// </summary>
        [TestMethod]
        public void TestFormatDateGroupHeaderTh()
        {
            DateTime date = new(2026, 5, 10);
            string actual = DateTimeConverter.FormatDateGroupHeader(date);

            Assert.AreEqual(
                "10th May 2026",
                actual);
        }

        /// <summary>
        /// Tests whether FormatDateGroupHeader returns "th" for the 11th, 12th, and 13th.
        /// </summary>
        [TestMethod]
        public void TestFormatDateGroupHeaderTeens()
        {
            Assert.AreEqual(
                "11th May 2026",
                DateTimeConverter.FormatDateGroupHeader(new(2026, 5, 11)));

            Assert.AreEqual(
                "12th May 2026",
                DateTimeConverter.FormatDateGroupHeader(new(2026, 5, 12)));

            Assert.AreEqual(
                "13th May 2026",
                DateTimeConverter.FormatDateGroupHeader(new(2026, 5, 13)));
        }

        /// <summary>
        /// Tests whether FormatDateGroupHeader returns the correct ordinal suffix for 21st, 22nd, 23rd.
        /// </summary>
        [TestMethod]
        public void TestFormatDateGroupHeaderTwenties()
        {
            Assert.AreEqual(
                "21st May 2026",
                DateTimeConverter.FormatDateGroupHeader(new(2026, 5, 21)));

            Assert.AreEqual(
                "22nd May 2026",
                DateTimeConverter.FormatDateGroupHeader(new(2026, 5, 22)));

            Assert.AreEqual(
                "23rd May 2026",
                DateTimeConverter.FormatDateGroupHeader(new(2026, 5, 23)));
        }
    }
}
