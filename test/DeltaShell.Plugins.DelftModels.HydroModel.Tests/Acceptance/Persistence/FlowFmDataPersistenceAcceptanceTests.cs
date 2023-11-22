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
    [Category(TestCategories.AcceptanceCategory)]
    [Category("SaveLoad.mdu")]
    public class FlowFmDataPersistenceAcceptanceTests
    {
        private string tempDirectory;
        private string firstSaveProjectPath;
        private string secondSaveProjectPath;
        private string acceptanceModelsDirectory;
        private string referenceSaveData;

        public delegate int ActualCountFuncDelegate(IHydroNetwork network);
        public static IEnumerable<TestCaseData> AcceptanceTests
        {
            get
            {
                yield return new TestCaseData("Hydamo_MoergestelBroek", "moergestels_broek", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 289).SetName("Hydamo_MoergestelBroek");
                yield return new TestCaseData("FlowFM_Eindhoven", "FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 398).SetName("Eindhoven");
            }
        }
        
        public static IEnumerable<TestCaseData> SoftSupTests 
        {
            get
            {
                yield return new TestCaseData("Hooge_Raam", "DFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 1571).SetName("Hooge_Raam");//SOFTSUP-439
            }
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string basePath = GuiTestHelper.IsBuildServer
                                  ? @"..\..\"
                                  : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\";

            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModels\FlowFM");
            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\FlowFM");
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
        [TestCaseSource(nameof(SoftSupTests))]
        public void GivenRunningDeltaShellGuiWithImportedFlowFmModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsInitiallySavedModel(
            string acceptanceModelName,
            string acceptanceModelFileName,
            ActualCountFuncDelegate actualCountFunc,
            int preconditionExpectedBranchFeaturesCount)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                Console.WriteLine("Importing model");
                ImportFlowFmModelAndAssertPreconditions(
                    acceptanceModelName,
                    acceptanceModelFileName,
                    actualCountFunc,
                    gui,
                    preconditionExpectedBranchFeaturesCount);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);

                // [Then]
                Console.WriteLine("Comparing saved data");
                string firstSaveProjectDirectory = Path.Combine(firstSaveProjectPath + "_data");
                string secondSaveProjectDirectory = Path.Combine(secondSaveProjectPath + "_data");
                InputFileComparer.CompareInputDirectories(firstSaveProjectDirectory, 
                                                          secondSaveProjectDirectory, 
                                                          acceptanceModelFileName, 
                                                          tempDirectory, 
                                                          false);

                Console.WriteLine("Comparing saved data with reference data");
                string referenceSaveDataDirectory = Path.Combine(referenceSaveData, acceptanceModelName);
                InputFileComparer.CompareInputDirectories(referenceSaveDataDirectory,
                                                          firstSaveProjectDirectory,
                                                          acceptanceModelFileName,
                                                          tempDirectory,
                                                          false,
                                                          new Dictionary<string, IEnumerable<string>>(),
                                                          AcceptanceModelTestHelper.RainfallRunoffLinesToIgnore);
            }
        }

        private void ImportFlowFmModelAndAssertPreconditions(
            string acceptanceModelName,
            string acceptanceModelFileName,
            ActualCountFuncDelegate actualCountFunc,
            IGui gui,
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
    }
}