// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Converters;
using DeploymentManager.Entities;

namespace DeploymentManager.Test.Converters
{
    [TestClass]
    public class DeploymentStageConverterTest
    {
        /// <summary>
        /// Tests whether FormatStageName returns "Fetch Artefacts" for FetchArtefacts.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameFetchArtefacts()
        {
            string actual = DeploymentStageConverter.FormatStageName(DeploymentStage.FetchArtefacts);

            Assert.AreEqual(
                "Fetch Artefacts",
                actual);
        }

        /// <summary>
        /// Tests whether FormatStageName returns "Extract Artefacts" for ExtractArtefacts.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameExtractArtefacts()
        {
            string actual = DeploymentStageConverter.FormatStageName(DeploymentStage.ExtractArtefacts);

            Assert.AreEqual(
                "Extract Artefacts",
                actual);
        }

        /// <summary>
        /// Tests whether FormatStageName returns "Fetch Artefact Files" for FetchArtefactFiles.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameFetchArtefactFiles()
        {
            string actual = DeploymentStageConverter.FormatStageName(DeploymentStage.FetchArtefactFiles);

            Assert.AreEqual(
                "Fetch Artefact Files",
                actual);
        }

        /// <summary>
        /// Tests whether FormatStageName returns "Stop Services" for StopServices.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameStopServices()
        {
            string actual = DeploymentStageConverter.FormatStageName(DeploymentStage.StopServices);

            Assert.AreEqual(
                "Stop Services",
                actual);
        }

        /// <summary>
        /// Tests whether FormatStageName returns "Move Artefacts" for MoveArtefacts.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameMoveArtefacts()
        {
            string actual = DeploymentStageConverter.FormatStageName(DeploymentStage.MoveArtefacts);

            Assert.AreEqual(
                "Move Artefacts",
                actual);
        }

        /// <summary>
        /// Tests whether FormatStageName returns "Start Services" for StartServices.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameStartServices()
        {
            string actual = DeploymentStageConverter.FormatStageName(DeploymentStage.StartServices);

            Assert.AreEqual(
                "Start Services",
                actual);
        }

        /// <summary>
        /// Tests whether FormatStageName returns "Clean Artefacts" for CleanArtefacts.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameCleanArtefacts()
        {
            string actual = DeploymentStageConverter.FormatStageName(DeploymentStage.CleanArtefacts);

            Assert.AreEqual(
                "Clean Artefacts",
                actual);
        }

        /// <summary>
        /// Tests whether FormatStageName returns the ToString fallback for an unknown value.
        /// </summary>
        [TestMethod]
        public void TestFormatStageNameDefault()
        {
            string actual = DeploymentStageConverter.FormatStageName((DeploymentStage)999);

            Assert.AreEqual(
                "999",
                actual);
        }
    }
}
