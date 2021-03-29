using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
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
    [Category(TestCategories.AcceptanceCategory)]
    [Category("SaveLoad.mdu")]
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
                yield return new TestCaseData("Hydamo_MoergestelBroek", "moergestels_broek", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 289).SetName("Hydamo_MoergestelBroek");
                //yield return new TestCaseData("FlowFM_Eindhoven", "FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 0).SetName("Eindhoven"); // TODO: Add preconditions when the model can be correctly imported
                //yield return new TestCaseData("Pudong", @"FM\FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 0).SetName("Pudong");          // TODO: Add preconditions when the model can be correctly imported
            }
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string acceptanceModelPath = GuiTestHelper.IsBuildServer
                ? @"..\..\AcceptanceModels\FlowFM"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModels\FlowFM";

            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelPath);
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
                CompareResultDataWithReferenceData(Path.Combine(firstSaveProjectPath + "_data", "input"), acceptanceModelFileName);
            }
        }

        private void ImportFlowFmModelAndAssertPreconditions(
            string acceptanceModelName,
            string acceptanceModelFileName,
            ActualCountFuncDelegate actualCountFunc,
            DeltaShellGui gui,
            int expectedBranchFeaturesCount)
        {
            var importer = new WaterFlowFMFileImporter(()=> TestHelper.GetTestWorkingDirectory());
            string pathToMduFile = Path.Combine(acceptanceModelsDirectory, acceptanceModelName, acceptanceModelFileName+".mdu");

            // [Precondition]
            Assert.That(File.Exists(pathToMduFile), $"[Precondition failure] Cannot find the specified mdu file: {pathToMduFile}.");

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
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);
            if (!flowFmReferenceFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {flowFmReferenceFiles}.");
            }
            
            var secondSaveDirectory = Path.Combine(secondSaveProjectPath + "_data", "FlowFM", "input");
            var flowFmResultFiles = Directory.GetFiles(secondSaveDirectory);
            if (!flowFmResultFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {secondSaveDirectory}.");
            }

            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory);
        }
    }
}