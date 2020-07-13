using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence
{
    [TestFixture]
    [Category("Build.Acceptance.SaveLoad")]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    public class FlowFmDataPersistenceAcceptanceTests
    {
        private string tempDirectory;
        private string firstSaveProjectPath;
        private string secondSaveProjectPath;
        private string acceptanceModelsDirectory;

        private static readonly object[] AcceptanceTests =
        {
            new object[] {"Groesbeek", 722}, // TODO: Add preconditions when the model can be correctly imported
            //new object[] { "Hydamo_DBV", 0}, // TODO: Add preconditions when the model can be correctly imported
            //new object[] { "Hydamo_MoergestelBroek", 0} // TODO: Add preconditions when the model can be correctly imported
        };

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "AcceptanceModels", "FlowFM");
        }

        [SetUp]
        public void SetUp()
        {
            tempDirectory = FileUtils.CreateTempDirectory();

            var firstSaveDirectory = Path.Combine(tempDirectory, "First save");
            firstSaveProjectPath = Path.Combine(firstSaveDirectory, "TestProject.dsproj");

            var secondSaveDirectory = Path.Combine(tempDirectory, "Second save");
            secondSaveProjectPath = Path.Combine(secondSaveDirectory, "TestProject.dsproj");

            Directory.CreateDirectory(firstSaveDirectory);
            Directory.CreateDirectory(secondSaveDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory);
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        public void GivenRunningDeltaShellGuiWithImportedFlowFmModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsInitiallySavedModel(
            string acceptanceModelName,
            int preconditionExpectedBranchFeaturesCount)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {

                ImportFlowFmModelAndAssertPreconditions(
                    acceptanceModelName,
                    gui,
                    preconditionExpectedBranchFeaturesCount);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(firstSaveProjectPath + "_data", "FlowFM"));
            }
        }

        private void ImportFlowFmModelAndAssertPreconditions(
            string acceptanceModelName, 
            DeltaShellGui gui,
            int expectedBranchFeaturesCount)
        {
            var importer = new WaterFlowFMFileImporter();
            var pathToMduFile = Path.Combine(acceptanceModelsDirectory, acceptanceModelName, "FlowFM.mdu");
            WaterFlowFMModel model = null;
            var errorMessages = TestHelper.GetAllRenderedMessages(() => model = importer.ImportItem(pathToMduFile) as WaterFlowFMModel, Level.Error);

            Assert.IsNotNull(model);
            gui.Application.Project.RootFolder.Add(model);
            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the FlowFM model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            var hydroNetwork = model.Network;
            Assert.AreEqual(expectedBranchFeaturesCount, hydroNetwork.BranchFeatures.Count(), "[Precondition failure] Unexpected number of branch features");
        }

        private void CompareResultDataWithReferenceData(string flowFmReferenceFileDirectory)
        {
            var flowFmResultFiles = Directory.GetFiles(Path.Combine(secondSaveProjectPath + "_data", "FlowFM"));
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);

            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory);
        }
    }
}