using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("Run.mdu")]
    public class RunFlowFmAcceptanceTests
    {
        private bool keepOutput = true;
        private string tempDirectory;
        private string acceptanceModelsDirectory;
        private string acceptanceModelsReferenceOutputDirectory;
        private string referenceSaveData;

        public delegate int ActualCountFuncDelegate(IHydroNetwork network);
        public static IEnumerable<TestCaseData> AcceptanceTests 
        {
            get
            {
                yield return new TestCaseData("Hydamo_MoergestelBroek", "moergestels_broek", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 289).SetName("Hydamo_MoergestelBroek");
                yield return new TestCaseData("FlowFM_Eindhoven", "FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 398).SetName("Eindhoven");
                //yield return new TestCaseData("Pudong", @"FM\FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 0).SetName("Pudong");          // TODO: Add preconditions when the model can be correctly imported
            }
        }
        
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string basePath = GuiTestHelper.IsBuildServer
                                  ? @"..\..\"
                                  : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\";
            
            string acceptanceModelPath = Path.Combine(basePath, @"AcceptanceModels\FlowFM");
            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelPath);

            string acceptanceModelReferenceOutputPath = Path.Combine(basePath, @"AcceptanceModelsReferenceOutput\FlowFM");
            acceptanceModelsReferenceOutputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelReferenceOutputPath);
            
            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\FlowFM");
        }

        [SetUp]
        public void SetUp()
        {
            string subFolder = "AcceptanceTests";
            tempDirectory = TestHelper.GetTestWorkingDirectory(subFolder);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory);
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        public void GivenRunningDeltaShellGuiWithImportedFlowFmModel_WhenRunningImportedModel_ThenImportedModelHasSuccessfullyRunAndOutputIsSameAsExpectedOutput(
            string acceptanceModelName,
            string acceptanceModelFileName,
            ActualCountFuncDelegate actualCountFunc,
            int preconditionExpectedBranchFeaturesCount)
        {
            // [Given]
            using (DeltaShellGui gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {

                WaterFlowFMModel fmModel = ImportFlowFmModelAndAssertPreconditions(
                                            acceptanceModelName,
                                            acceptanceModelFileName,
                                            actualCountFunc,
                                            gui,
                                            preconditionExpectedBranchFeaturesCount);

                Console.WriteLine("Setting model settings");

                // [When]
                Console.WriteLine("Running model");
                ActivityRunner.RunActivity(fmModel);
                Assert.That(fmModel.Status, Is.EqualTo(ActivityStatus.Cleaned));
                
                Console.WriteLine("Saving model");
                string savePath = Path.Combine(tempDirectory, "SavedModel");
                gui.Application.SaveProjectAs(savePath);

                // [Then]
                Console.WriteLine("Comparing saved input data with reference input data");
                string saveDirectory = savePath + "_data";
                string referenceSaveDataDirectory = Path.Combine(referenceSaveData, acceptanceModelName);
                InputFileComparer.CompareInputDirectories(referenceSaveDataDirectory,
                                                          saveDirectory,
                                                          acceptanceModelFileName,
                                                          tempDirectory,
                                                          false,
                                                          AcceptanceModelTestHelper.GetFlowFmLinesToIgnore(acceptanceModelFileName + ".mdu"),
                                                          AcceptanceModelTestHelper.RainfallRunoffLinesToIgnore);
                CompareResultDataWithReferenceData(acceptanceModelName, acceptanceModelFileName);
            }
        }

        private WaterFlowFMModel ImportFlowFmModelAndAssertPreconditions(
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
            IEnumerable<string> errorMessages = TestHelper.GetAllRenderedMessages(() => model = importer.ImportItem(pathToMduFile) as WaterFlowFMModel, Level.Error);

            Assert.IsNotNull(model);
            gui.Application.Project.RootFolder.Add(model);

            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the FlowFM model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            IHydroNetwork hydroNetwork = model.Network;
            Assert.AreEqual(expectedBranchFeaturesCount, actualCountFunc(hydroNetwork), "[Precondition failure] Unexpected number of branch features");

            return model;
        }

        private void CompareResultDataWithReferenceData(string acceptanceModelName, string acceptanceModelFileName)
        {
            RunModelAcceptanceTestHelper.CompareFlowFmOutput(acceptanceModelName, acceptanceModelsReferenceOutputDirectory,
                                                             tempDirectory, keepOutput, acceptanceModelFileName);
        }
    }
}