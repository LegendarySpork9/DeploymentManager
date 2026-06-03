// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Services;
using Moq;

namespace DeploymentManager.Test.Services
{
    [TestClass]
    public class DocumentServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();

        /// <summary>
        /// Tests whether the ExtractArtefact method returns true when extraction succeeds.
        /// </summary>
        [TestMethod]
        public async Task TestExtractArtefactReturnsTrue()
        {
            FileStream fileStream = new(
                Path.GetTempFileName(),
                FileMode.Open,
                FileAccess.Read,
                FileShare.None,
                4096,
                FileOptions.DeleteOnClose);

            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>()))
                .ReturnsAsync(fileStream);

            _MockFileSystem.Setup(fs => fs.ExtractArchive(
                    It.IsAny<string>(),
                    It.IsAny<FileStream>()))
                .Returns(Task.CompletedTask);

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.ExtractArtefact(
                @"C:\Deploy\test-artefact.zip",
                @"C:\Deploy\test-artefact");

            Assert.IsTrue(actual);

            _MockFileSystem.Verify(
                fs => fs.ReadStream(@"C:\Deploy\test-artefact.zip"),
                Times.Once);
            _MockFileSystem.Verify(
                fs => fs.ExtractArchive(
                    @"C:\Deploy\test-artefact",
                    It.IsAny<FileStream>()),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the ExtractArtefact method returns false when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestExtractArtefactException()
        {
            _MockFileSystem.Setup(fs => fs.ReadStream(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.ExtractArtefact(
                @"C:\Deploy\test-artefact.zip",
                @"C:\Deploy\test-artefact");

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether the GetExtractedArtefactFiles method returns the file list when successful.
        /// </summary>
        [TestMethod]
        public async Task TestGetExtractedArtefactFilesReturnsData()
        {
            string[] expected =
            [
                @"C:\Deploy\test-artefact\file1.dll",
                @"C:\Deploy\test-artefact\file2.dll"
            ];

            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>()))
                .ReturnsAsync(expected);

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            string[] actual = await documentService.GetExtractedArtefactFiles(
                "test-artefact",
                @"C:\Deploy\test-artefact");

            Assert.HasCount(
                expected.Length,
                actual);
            Assert.AreEqual(
                expected[0],
                actual[0]);
            Assert.AreEqual(
                expected[1],
                actual[1]);
        }

        /// <summary>
        /// Tests whether the GetExtractedArtefactFiles method returns an empty array when an exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task TestGetExtractedArtefactFilesException()
        {
            _MockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>()))
                .ThrowsAsync(new DirectoryNotFoundException("Directory not found"));

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            string[] actual = await documentService.GetExtractedArtefactFiles(
                "test-artefact",
                @"C:\Deploy\test-artefact");

            Assert.IsEmpty(
                actual);
        }

        /// <summary>
        /// Tests whether the MoveArtefactFiles method returns true when all files are moved successfully.
        /// </summary>
        [TestMethod]
        public async Task TestMoveArtefactFilesReturnsTrue()
        {
            List<(string, KeyValuePair<string, string>)> files =
            [
                ("file1.dll", new(@"C:\Deploy\test-artefact\file1.dll", @"C:\Project\file1.dll")),
                ("file2.dll", new(@"C:\Deploy\test-artefact\file2.dll", @"C:\Project\file2.dll"))
            ];

            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>()))
                .ReturnsAsync(true);

            _MockFileSystem.Setup(fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.MoveArtefactFiles(
                "test-artefact",
                @"C:\Project",
                files);

            Assert.IsTrue(actual);

            _MockFileSystem.Verify(
                fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Exactly(2));
            _MockFileSystem.Verify(
                fs => fs.CreateDirectory(It.IsAny<string>()),
                Times.Never);
        }

        /// <summary>
        /// Tests whether the MoveArtefactFiles method creates the directory when it does not exist.
        /// </summary>
        [TestMethod]
        public async Task TestMoveArtefactFilesCreatesDirectory()
        {
            List<(string, KeyValuePair<string, string>)> files =
            [
                ("file1.dll", new(@"C:\Deploy\test-artefact\file1.dll", @"C:\Project\sub\file1.dll"))
            ];

            _MockFileSystem.Setup(fs => fs.CheckDirectory(@"C:\Project\sub"))
                .ReturnsAsync(false);

            _MockFileSystem.Setup(fs => fs.CreateDirectory(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _MockFileSystem.Setup(fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.MoveArtefactFiles(
                "test-artefact",
                @"C:\Project",
                files);

            Assert.IsTrue(actual);

            _MockFileSystem.Verify(
                fs => fs.CreateDirectory(@"C:\Project\sub"),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the MoveArtefactFiles method returns false when a file fails to move.
        /// </summary>
        [TestMethod]
        public async Task TestMoveArtefactFilesReturnsFalse()
        {
            List<(string, KeyValuePair<string, string>)> files =
            [
                ("file1.dll", new(@"C:\Deploy\test-artefact\file1.dll", @"C:\Project\file1.dll")),
                ("file2.dll", new(@"C:\Deploy\test-artefact\file2.dll", @"C:\Project\file2.dll"))
            ];

            _MockFileSystem.Setup(fs => fs.CheckDirectory(It.IsAny<string>()))
                .ReturnsAsync(true);

            _MockFileSystem.SetupSequence(fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .ThrowsAsync(new IOException("Access denied"));

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.MoveArtefactFiles(
                "test-artefact",
                @"C:\Project",
                files);

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether the MoveArtefactFiles method returns true when the file list is empty.
        /// </summary>
        [TestMethod]
        public async Task TestMoveArtefactFilesEmptyList()
        {
            List<(string, KeyValuePair<string, string>)> files = [];

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.MoveArtefactFiles(
                "test-artefact",
                @"C:\Project",
                files);

            Assert.IsTrue(actual);

            _MockFileSystem.Verify(
                fs => fs.CopyFile(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        /// <summary>
        /// Tests whether the DeleteArtefact method returns true when deletion succeeds.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteArtefactReturnsTrue()
        {
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.DeleteArtefact(
                "test-artefact",
                @"C:\Deploy\test-artefact.zip",
                @"C:\Deploy\test-artefact");

            Assert.IsTrue(actual);

            _MockFileSystem.Verify(
                fs => fs.DeleteDirectory(@"C:\Deploy\test-artefact"),
                Times.Once);
            _MockFileSystem.Verify(
                fs => fs.DeleteFile(@"C:\Deploy\test-artefact.zip"),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the DeleteArtefact method returns false when DeleteDirectory throws an exception.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteArtefactDirectoryException()
        {
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>()))
                .ThrowsAsync(new IOException("Directory in use"));

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.DeleteArtefact(
                "test-artefact",
                @"C:\Deploy\test-artefact.zip",
                @"C:\Deploy\test-artefact");

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether the DeleteArtefact method returns false when DeleteFile throws an exception.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteArtefactFileException()
        {
            _MockFileSystem.Setup(fs => fs.DeleteDirectory(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _MockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .ThrowsAsync(new IOException("File in use"));

            DocumentService documentService = new(
                _MockLogger.Object,
                _MockFileSystem.Object);

            bool actual = await documentService.DeleteArtefact(
                "test-artefact",
                @"C:\Deploy\test-artefact.zip",
                @"C:\Deploy\test-artefact");

            Assert.IsFalse(actual);
        }
    }
}
