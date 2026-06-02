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

        public async Task WriteAllText(
            string path,
            string text)
            => await File.WriteAllTextAsync(path, text);
    }
}
