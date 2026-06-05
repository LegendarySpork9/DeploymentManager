// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Entities
{
    /// <summary>
    /// Status of Deployments.
    /// </summary>
    public enum Status
    {
        PendingApproval,
        NotStarted,
        Running,
        Completed,
        CompletedWithWarnings,
        Skipped,
        Failed
    }
}
