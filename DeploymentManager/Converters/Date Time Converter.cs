// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Converters
{
    public static class DateTimeConverter
    {
        /// <summary>
        /// Returns the formatted time string.
        /// </summary>
        public static string FormatTime(
            DateTime dateTime,
            DateTime defaultDate)
        {
            string formattedTime = dateTime.ToString("HH:mm:ss.fff");

            if (dateTime == defaultDate)
            {
                formattedTime = "-";
            }

            return formattedTime;
        }

        /// <summary>
        /// Returns the formatted dtaetime string.
        /// </summary>
        public static string FormatDateTime(
            DateTime dateTime,
            DateTime defaultDate)
        {
            string formattedDateTime = dateTime.ToString("dd MMM yyyy HH:mm:ss");

            if (dateTime == defaultDate)
            {
                formattedDateTime = "-";
            }

            return formattedDateTime;
        }

        /// <summary>
        /// Returns the formatted run times string.
        /// </summary>
        public static string FormatRunTime(
            TimeSpan runTime,
            TimeSpan defaultTimeSpan)
        {
            string formattedRunTime = runTime.ToString(@"s\.fff") + "s";

            if (runTime == defaultTimeSpan)
            {
                formattedRunTime = "-";
            }

            if (runTime.TotalMinutes >= 1)
            {
                formattedRunTime = runTime.ToString(@"m\:ss\.fff");
            }

            return formattedRunTime;
        }

        /// <summary>
        /// Returns the formatted friendly run time.
        /// </summary>
        public static string FormatRunTimeFriendly(
            TimeSpan runTime,
            TimeSpan defaultTimeSpan)
        {
            string formattedFriendlyRunTime = $"{runTime.Seconds}s";

            if (runTime == defaultTimeSpan)
            {
                formattedFriendlyRunTime = "-";
            }

            if (runTime.TotalMinutes >= 1)
            {
                formattedFriendlyRunTime = $"{(int)runTime.TotalMinutes}m {runTime.Seconds}s";
            }

            if (runTime.TotalHours >= 1)
            {
                formattedFriendlyRunTime = $"{(int)runTime.TotalHours}h {runTime.Minutes}m";
            }

            return formattedFriendlyRunTime;
        }
    }
}
