// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Implementations;

namespace DeploymentManager.PersistenceTests.Implementations
{
    [TestClass]
    public class FileSystemWrapperTest
    {
        private string _TempDir = string.Empty;
        private FileSystemWrapper _FileSystem = null!;

        [TestInitialize]
        public void Setup()
        {
            _TempDir = Path.Combine(
                Path.GetTempPath(),
                "FileSystemWrapperTest_" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_TempDir);
            _FileSystem = new FileSystemWrapper();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_TempDir))
            {
                Directory.Delete(_TempDir, true);
            }
        }

        /// <summary>
        /// Tests whether WriteAllText and ReadAllText correctly round-trip text content.
        /// </summary>
        [TestMethod]
        public async Task TestWriteAndReadAllText()
        {
            string filePath = Path.Combine(
                _TempDir,
                "roundtrip.txt");
            string expected = "Hello, DeploymentManager!";

            await _FileSystem.WriteAllText(
                filePath,
                expected);
            string actual = await _FileSystem.ReadAllText(filePath);

            Assert.AreEqual(
                expected,
                actual);
        }

        /// <summary>
        /// Tests whether CheckFile returns true when the file exists.
        /// </summary>
        [TestMethod]
        public async Task TestCheckFileExists()
        {
            string filePath = Path.Combine(
                _TempDir,
                "exists.txt");
            await File.WriteAllTextAsync(
                filePath,
                "content");

            bool actual = await _FileSystem.CheckFile(filePath);

            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Tests whether CheckFile returns false when the file does not exist.
        /// </summary>
        [TestMethod]
        public async Task TestCheckFileNotExists()
        {
            string filePath = Path.Combine(
                _TempDir,
                "nonexistent.txt");

            bool actual = await _FileSystem.CheckFile(filePath);

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether CreateDirectory successfully creates a new directory.
        /// </summary>
        [TestMethod]
        public async Task TestCreateDirectory()
        {
            string dirPath = Path.Combine(
                _TempDir,
                "newdir");

            await _FileSystem.CreateDirectory(dirPath);

            Assert.IsTrue(Directory.Exists(dirPath));
        }

        /// <summary>
        /// Tests whether CheckDirectory returns true when the directory exists.
        /// </summary>
        [TestMethod]
        public async Task TestCheckDirectoryExists()
        {
            string dirPath = Path.Combine(
                _TempDir,
                "checkdir");
            Directory.CreateDirectory(dirPath);

            bool actual = await _FileSystem.CheckDirectory(dirPath);

            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Tests whether DeleteFile removes the file from disk.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteFile()
        {
            string filePath = Path.Combine(
                _TempDir,
                "deleteme.txt");
            await File.WriteAllTextAsync(
                filePath,
                "content");

            await _FileSystem.DeleteFile(filePath);

            Assert.IsFalse(File.Exists(filePath));
        }

        /// <summary>
        /// Tests whether GetFiles returns all files within a directory including subdirectories.
        /// </summary>
        [TestMethod]
        public async Task TestGetFiles()
        {
            string subDir = Path.Combine(
                _TempDir,
                "sub");
            Directory.CreateDirectory(subDir);

            await File.WriteAllTextAsync(
                Path.Combine(
                    _TempDir,
                    "file1.txt"),
                "a");
            await File.WriteAllTextAsync(
                Path.Combine(
                    _TempDir,
                    "file2.txt"),
                "b");
            await File.WriteAllTextAsync(
                Path.Combine(
                    subDir,
                    "file3.txt"),
                "c");

            string[] actual = await _FileSystem.GetFiles(_TempDir);

            Assert.HasCount(
                3,
                actual);
        }

        /// <summary>
        /// Tests whether CopyFile duplicates a file to the destination path.
        /// </summary>
        [TestMethod]
        public async Task TestCopyFile()
        {
            string sourcePath = Path.Combine(
                _TempDir,
                "source.txt");
            string destinationPath = Path.Combine(
                _TempDir,
                "destination.txt");
            string expected = "copy content";

            await File.WriteAllTextAsync(
                sourcePath,
                expected);

            await _FileSystem.CopyFile(
                sourcePath,
                destinationPath);

            Assert.IsTrue(File.Exists(destinationPath));

            string actual = await File.ReadAllTextAsync(destinationPath);

            Assert.AreEqual(
                expected,
                actual);
        }
    }
}
