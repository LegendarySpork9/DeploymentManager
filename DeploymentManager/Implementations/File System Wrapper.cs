// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;

namespace DeploymentManager.Implementations
{
    public class FileSystemWrapper : IFileSystem
    {
        /// <summary>
        /// Returns the text in a given file.
        /// </summary>
        public async Task<string> ReadAllText(string path) => await File.ReadAllTextAsync(path);

        /// <summary>
        /// Creates/Updates a given file with the given text.
        /// </summary>
        public async Task WriteAllText(
            string path,
            string text)
            => await File.WriteAllTextAsync(path, text);

        /// <summary>
        /// Creates a given directory.
        /// </summary>
        public async Task CreateDirectory(string path) => Directory.CreateDirectory(path);
    }
}
