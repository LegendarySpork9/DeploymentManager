// Copyright © - Unpublished - Toby Hunter
namespace DeploymentManager.Abstractions
{
    /// <summary>
    /// Interface for the file system operations.
    /// </summary>
    public interface IFileSystem
    {
        // File Operations
        Task<string> ReadAllText(string path);
        Task WriteAllText(string path, string text);
        Task WriteStream(string path, Stream stream);
        Task<FileStream> ReadStream(string path);
        Task CopyFile(string source, string destination);
        Task DeleteFile(string path);
        Task<bool> CheckFile(string path);

        // Directory Operations
        Task CreateDirectory(string path);
        Task<string[]> GetFiles(string path);
        Task<bool> CheckDirectory(string path);
        Task DeleteDirectory(string path);

        // ZIP Operations
        Task ExtractArchive(string path, FileStream fileStream);
    }
}
