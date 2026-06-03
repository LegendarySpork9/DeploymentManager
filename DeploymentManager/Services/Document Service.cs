// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Values;

namespace DeploymentManager.Services
{
    public class DocumentService
    {
        private readonly ILoggerService _Logger;
        private readonly IFileSystem _FileSystem;

        // Sets the class's global variables.
        public DocumentService(
            ILoggerService _logger,
            IFileSystem _fileSystem)
        {
            _Logger = _logger;
            _FileSystem = _fileSystem;
        }

        /// <summary>
        /// Extracts the download artefact.
        /// </summary>
        public async Task<bool> ExtractArtefact(
            string file,
            string path)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Extracting artefact, {Path.GetFileNameWithoutExtension(file)}, to {path}");

            bool extracted = false;

            try
            {
                using (FileStream fileStream = await _FileSystem.ReadStream(file))
                {
                    await _FileSystem.ExtractArchive(
                        path,
                        fileStream);

                    extracted = true;

                    _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Extracted artefact, {Path.GetFileNameWithoutExtension(file)}, to {path}");
                }                    
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to extract artefact, {Path.GetFileNameWithoutExtension(file)}, to {path}");
            }

            return extracted;
        }

        /// <summary>
        /// Returns the list of extracted files.
        /// </summary>
        public async Task<string[]> GetExtractedArtefactFiles(
            string artefact,
            string path)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Obtaining extracted artefact, {artefact}");

            string[] files = [];

            try
            {
                files = await _FileSystem.GetFiles(path);

                _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Obtained extracted artefact, {artefact}");
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to obtain extracted artefact, {artefact}");
            }

            return files;
        }

        /// <summary>
        /// Moves the artefact files to the given directory.
        /// </summary>
        public async Task<bool> MoveArtefactFiles(
            string artefact,
            string path,
            List<(string, KeyValuePair<string, string>)> files)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Moving files for artefact, {artefact}, to {path}");

            bool moved = false;
            List<bool> moveResult = [];

            foreach ((string, KeyValuePair<string, string>) file in files)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Moving file, {file.Item1} for artefact, {artefact}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Source: {file.Item2.Key}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Destination: {file.Item2.Value}");

                try
                {
                    string? directory = Path.GetDirectoryName(file.Item2.Value);

                    if (!string.IsNullOrWhiteSpace(directory) && !await _FileSystem.CheckDirectory(directory))
                    {
                        await _FileSystem.CreateDirectory(directory);
                    }

                    await _FileSystem.CopyFile(
                        file.Item2.Key,
                        file.Item2.Value);
                    moveResult.Add(true);

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Moved file, {file.Item1} for artefact, {artefact}");
                }

                catch (Exception ex)
                {
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Warning,
                        ex.Message);
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Error,
                        ex.ToString());
                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Failed to move file, {file.Item1} for artefact, {artefact}");
                    moveResult.Add(false);
                }
            }

            moved = moveResult.All(m => m);

            if (moved)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Moved files for artefact, {artefact}, to {path}");
            }

            else
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to move files for artefact, {artefact}, to {path}");
            }

            return moved;
        }

        /// <summary>
        /// Deletes the downloaded artefact.
        /// </summary>
        public async Task<bool> DeleteArtefact(
            string artefact,
            string path,
            string extractPath)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Deleting artefact, {artefact}");

            bool deleted = false;

            try
            {
                await _FileSystem.DeleteDirectory(extractPath);
                await _FileSystem.DeleteFile(path);
                
                deleted = true;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Deleted artefact, {artefact}");
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to delete artefact, {artefact}");
            }

            return deleted;
        }
    }
}
