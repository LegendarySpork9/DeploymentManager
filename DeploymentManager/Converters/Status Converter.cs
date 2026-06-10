// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Converters
{
    public static class StatusConverter
    {
        /// <summary>
        /// Returns the status class for the given status.
        /// </summary>
        public static string GetStatusBadgeClass(Status status)
        {
            return status switch
            {
                Status.PendingApproval => "bg-secondary",
                Status.NotStarted => "bg-secondary",
                Status.Running => "bg-primary",
                Status.Completed => "bg-success",
                Status.CompletedWithWarnings => "bg-warning text-dark",
                Status.Failed => "bg-danger",
                Status.Skipped => "badge-skipped",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Returns the card class for the given status.
        /// </summary>
        public static string GetCardClass(Status status)
        {
            return status switch
            {
                Status.NotStarted => "card-not-started",
                Status.Running => "card-running",
                Status.Completed => "card-completed",
                Status.CompletedWithWarnings => "card-completed-warnings",
                Status.Failed => "card-failed",
                Status.Skipped => "card-skipped",
                _ => "card-not-started"
            };
        }

        /// <summary>
        /// Returns the status display text for the given status.
        /// </summary>
        public static string GetStatusDisplayText(Status status)
        {
            return status switch
            {
                Status.PendingApproval => "Pending Approval",
                Status.NotStarted => "Not Started",
                Status.Running => "Running",
                Status.Completed => "Complete",
                Status.CompletedWithWarnings => "Completed With Warnings",
                Status.Skipped => "Skipped",
                Status.Failed => "Failed",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Returns badge class for the given status.
        /// </summary>
        public static string GetDeploymentTypeBadgeClass(DeploymentType type)
        {
            return type switch
            {
                DeploymentType.GitHub => "bg-info text-dark",
                DeploymentType.FileUpload => "bg-success",
                _ => "bg-secondary"
            };
        }
    }
}
