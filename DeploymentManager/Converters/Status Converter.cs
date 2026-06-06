// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Entities;

namespace DeploymentManager.Converters
{
    public static class StatusConverter
    {
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
