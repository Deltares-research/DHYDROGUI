using System;
using System.Collections.Generic;
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

        public delegate int ActualCountFuncDelegate(IHydroNetwork network);
        public static IEnumerable<TestCaseData> AcceptanceTests 
        {
            get
            {
                yield return new TestCaseData("GroekBeek2", "FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 719); // TODO: Add preconditions when the model can be correctly imported
                yield return new TestCaseData("Hydamo_DBV", "DVB", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 601); // TODO: Add preconditions when the model can be correctly imported
                yield return new TestCaseData("Hydamo_MoergestelBroek", "moergestels_broek", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 288); // TODO: Add preconditions when the model can be correctly imported
            }
        }
        

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
            string acceptanceModelFileName,
            ActualCountFuncDelegate actualCountFunc,
            int preconditionExpectedBranchFeaturesCount)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {

                ImportFlowFmModelAndAssertPreconditions(
                    acceptanceModelName,
                    acceptanceModelFileName,
                    actualCountFunc,
                    gui,
                    preconditionExpectedBranchFeaturesCount);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(firstSaveProjectPath + "_data"), acceptanceModelFileName);
            }
        }

        private void ImportFlowFmModelAndAssertPreconditions(
            string acceptanceModelName,
            string acceptanceModelFileName,
            ActualCountFuncDelegate actualCountFunc,
            DeltaShellGui gui,
            int expectedBranchFeaturesCount)
        {
            var importer = new WaterFlowFMFileImporter();
            var pathToMduFile = Path.Combine(acceptanceModelsDirectory, acceptanceModelName, acceptanceModelFileName+".mdu");
            WaterFlowFMModel model = null;
            var errorMessages = TestHelper.GetAllRenderedMessages(() => model = importer.ImportItem(pathToMduFile) as WaterFlowFMModel, Level.Error);

            Assert.IsNotNull(model);
            gui.Application.Project.RootFolder.Add(model);
            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the FlowFM model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            var hydroNetwork = model.Network;
            Assert.AreEqual(expectedBranchFeaturesCount, actualCountFunc(hydroNetwork), "[Precondition failure] Unexpected number of branch features");
        }

        private void CompareResultDataWithReferenceData(string flowFmReferenceFileDirectory, string acceptanceModelFileName)
        {
            var flowFmResultFiles = Directory.GetFiles(Path.Combine(secondSaveProjectPath + "_data", acceptanceModelFileName));
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);

            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory);
        }
    }
}