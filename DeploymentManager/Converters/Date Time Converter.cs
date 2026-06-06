// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Converters
{
    public static class DateTimeConverter
    {
        public static string FormatTime(DateTime dateTime, DateTime defaultDate)
        {
            if (dateTime == defaultDate)
            {
                return "-";
            }

            return dateTime.ToString("HH:mm:ss");
        }

        public static string FormatDateTime(DateTime dateTime, DateTime defaultDate)
        {
            if (dateTime == defaultDate)
            {
                return "-";
            }

            return dateTime.ToString("dd MMM yyyy HH:mm:ss");
        }

        public static string FormatRunTime(TimeSpan runTime, TimeSpan defaultTimeSpan)
        {
            if (runTime == defaultTimeSpan)
            {
                return "-";
            }

            if (runTime.TotalMinutes >= 1)
            {
                return runTime.ToString(@"m\:ss\.fff");
            }

            return runTime.ToString(@"s\.fff") + "s";
        }

        public static string FormatRunTimeFriendly(TimeSpan runTime, TimeSpan defaultTimeSpan)
        {
            if (runTime == defaultTimeSpan)
            {
                return "-";
            }

            if (runTime.TotalHours >= 1)
            {
                return $"{(int)runTime.TotalHours}h {runTime.Minutes}m";
            }

            if (runTime.TotalMinutes >= 1)
            {
                return $"{(int)runTime.TotalMinutes}m {runTime.Seconds}s";
            }

            return $"{runTime.Seconds}s";
        }
    }
}
