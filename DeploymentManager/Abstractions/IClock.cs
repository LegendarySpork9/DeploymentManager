// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for the DateTime object.
    /// </summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateTime DefaultDate { get; }
        TimeSpan DefaultTimeSpan { get; }
    }
}
