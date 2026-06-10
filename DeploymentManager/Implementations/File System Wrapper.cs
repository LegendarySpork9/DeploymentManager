// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using System.IO.Compression;

namespace DeploymentManager.Implementations
{
    public class FileSystemWrapper : IFileSystem
    {
        // File Operations

        /// <summary>
        /// Returns the text in a given file.
        /// </summary>
        public async Task<string> ReadAllText(string path) => await File.ReadAllTextAsync(path);

        /// <summary>
        /// Creates/Updates a given file with the given text.
        /// </summary>
        public async Task WriteAllText(
            string path,
            string text) => await File.WriteAllTextAsync(path, text);

        /// <summary>
        /// Creates/updates a given file with the given stream.
        /// </summary>
        public async Task WriteStream(
            string path,
            Stream stream)
        {
            using (FileStream fileStream = new(
                path,
                FileMode.Create,
                FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Returns a file stream for a given file.
        /// </summary>
        public Task<FileStream> ReadStream(string path) => Task.FromResult(new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read));

        /// <summary>
        /// Copies a given file to a given folder.
        /// </summary>
        public Task CopyFile(
            string source,
            string destination)
        {
            File.Copy(source, destination, true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a given file.
        /// </summary>
        public Task DeleteFile(string path)
        {
            File.Delete(path);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if a given file exists.
        /// </summary>
        public Task<bool> CheckFile(string path) => Task.FromResult(File.Exists(path));

        // Directory Operations

        /// <summary>
        /// Creates a given directory.
        /// </summary>
        public Task CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a list of the files in a given directory.
        /// </summary>
        public Task<string[]> GetFiles(string path) => Task.FromResult(Directory.GetFiles(
            path,
            "*",
            SearchOption.AllDirectories));

        /// <summary>
        /// Checks if a given directory exists.
        /// </summary>
        public Task<bool> CheckDirectory(string path) => Task.FromResult(Directory.Exists(path));

        /// <summary>
        /// Deletes a given directory.
        /// </summary>
        public Task DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
            return Task.CompletedTask;
        }

        // ZIP Operations

        public async Task ExtractArchive(
            string path,
            FileStream fileStream)
        {
            using (ZipArchive zip = new(fileStream))
            {
                await zip.ExtractToDirectoryAsync(path);
            }
        }
    }
}
