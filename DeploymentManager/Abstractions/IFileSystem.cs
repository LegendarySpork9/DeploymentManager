// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for the file system operations.
    /// </summary>
    public interface IFileSystem
    {
        Task<string> ReadAllText(string path);
        Task WriteAllText(string path, string text);
        Task CreateDirectory(string path);
        Task WriteStream(string path, Stream stream);
    }
}
