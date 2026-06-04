// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models.Data.Related;
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
        public async Task<(bool, string?)> ExtractArtefact(
            string file,
            string path)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Extracting artefact, {Path.GetFileNameWithoutExtension(file)}, to {path}");

            bool extracted = false;
            string? errorMessage = null;

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
                errorMessage = ex.Message;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    errorMessage);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to extract artefact, {Path.GetFileNameWithoutExtension(file)}, to {path}");
            }

            return (extracted, errorMessage);
        }

        /// <summary>
        /// Returns the list of extracted files.
        /// </summary>
        public async Task<(string[], string?)> GetExtractedArtefactFiles(
            string artefact,
            string path)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Obtaining extracted files for artefact, {artefact}");

            string[] files = [];
            string? errorMessage = null;

            try
            {
                files = await _FileSystem.GetFiles(path);

                _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Obtained extracted files for artefact, {artefact}");
            }

            catch (Exception ex)
            {
                errorMessage = ex.Message;

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    errorMessage);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to obtain extracted files for artefact, {artefact}");
            }

            return (files, errorMessage);
        }

        /// <summary>
        /// Moves the artefact files to the given directory.
        /// </summary>
        public async Task<(bool, List<string>)> MoveArtefactFiles(
            string artefact,
            string path,
            string device,
            List<ArtefactFileModel> files)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Moving files for artefact, {artefact}, to {path}");

            bool moved = false;
            List<bool> moveResult = [];
            List<string> errorMessages = [];

            foreach (ArtefactFileModel file in files)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Moving file, {file.Name} for artefact, {artefact}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Source: {file.Paths.Key}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Destination: {file.Paths.Value}");

                try
                {
                    string? directory = Path.GetDirectoryName(file.Paths.Value);

                    if (!string.IsNullOrWhiteSpace(directory) && !await _FileSystem.CheckDirectory(directory))
                    {
                        await _FileSystem.CreateDirectory(directory);
                    }

                    await _FileSystem.CopyFile(
                        file.Paths.Key,
                        file.Paths.Value);
                    moveResult.Add(true);

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Info,
                        $"Moved file, {file.Name} for artefact, {artefact}");
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
                        $"Failed to move file, {file.Name} for artefact, {artefact}");
                    moveResult.Add(false);
                    errorMessages.Add($"{device} ({file.Name}) - {ex.Message}");
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

            return (moved, errorMessages);
        }

        /// <summary>
        /// Deletes the downloaded artefact.
        /// </summary>
        public async Task<(bool, string?)> DeleteArtefact(
            string artefact,
            string path,
            string extractPath)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                $"Deleting artefact, {artefact}");

            bool deleted = false;
            string? errorMessage = null;

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
                errorMessage = ex.Message;
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    errorMessage);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"Failed to delete artefact, {artefact}");
            }

            return (deleted, errorMessage);
        }
    }
}
