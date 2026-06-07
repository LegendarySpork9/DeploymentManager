// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models;
using DeploymentManager.Models.Data;
using DeploymentManager.Values;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace DeploymentManager.Components.Dialogs
{
    public partial class FileUploadDialog
    {
        [Inject]
        private ILoggerService _Logger { get; set; } = default!;
        [Inject]
        private IFileSystem _FileSystem { get; set; } = default!;
        [Inject]
        private AppSettingsModel AppSettings { get; set; } = default!;

        [Parameter]
        public EventCallback<UploadFileModel> OnFileUploaded { get; set; }
        [Parameter]
        public EventCallback OnCancelled { get; set; }

        private IBrowserFile? SelectedFile;

        private bool IsVisible;
        private bool IsUploading;

        private string ErrorMessage = string.Empty;

        private string BranchId = "main";
        private string BranchName = "main";

        /// <summary>
        /// Shows the file upload dialog.
        /// </summary>
        public void Show()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Showing file upload dialog");

            IsVisible = true;
            ErrorMessage = string.Empty;
            SelectedFile = null;
            StateHasChanged();
        }

        /// <summary>
        /// Handles file selection from the input.
        /// </summary>
        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            SelectedFile = e.File;
            ErrorMessage = string.Empty;

            if (!SelectedFile.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Only .zip files are accepted.";
                SelectedFile = null;
            }

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Selected File: {SelectedFile?.Name ?? "None"}");
        }

        /// <summary>
        /// Uploads the selected file to the artefact download location.
        /// </summary>
        private async Task HandleUpload()
        {
            if (SelectedFile == null)
            {
                return;
            }

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Branch Id {BranchId}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Branch Name: {BranchName}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                "Upload Clicked");

            IsUploading = true;
            ErrorMessage = string.Empty;

            try
            {
                string uploadDirectory = AppSettings.ArtefactDownloadLocation;

                if (!await _FileSystem.CheckDirectory(uploadDirectory))
                {
                    await _FileSystem.CreateDirectory(uploadDirectory);
                }

                string filePath = Path.Combine(
                    uploadDirectory,
                    SelectedFile.Name);

                await using (Stream browserStream = SelectedFile.OpenReadStream(524_288_000))
                {
                    await _FileSystem.WriteStream(
                        filePath,
                        browserStream);
                }
                
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    $"File Uploaded: {SelectedFile.Name} ({SelectedFile.Size} bytes)");

                UploadFileModel uploadFile = new()
                {
                    Id = 0,
                    Name = Path.GetFileNameWithoutExtension(SelectedFile.Name),
                    Size = SelectedFile.Size,
                    BranchId = BranchId,
                    BranchName = BranchName,
                    Directory = uploadDirectory
                };

                IsVisible = false;
                await OnFileUploaded.InvokeAsync(uploadFile);
            }

            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during upload. Please try again.";
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            IsUploading = false;
        }

        /// <summary>
        /// Cancels the file upload and fires the cancelled event.
        /// </summary>
        private async Task HandleCancel()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "File upload cancelled");

            IsVisible = false;
            await OnCancelled.InvokeAsync();
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes =
            [
                "B",
                "KB",
                "MB",
                "GB"
            ];
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}
