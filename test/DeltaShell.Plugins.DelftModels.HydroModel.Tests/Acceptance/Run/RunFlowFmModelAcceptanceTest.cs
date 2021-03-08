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

        public delegate int ActualCountFuncDelegate(IHydroNetwork network);
        public static IEnumerable<TestCaseData> AcceptanceTests 
        {
            get
            {
                yield return new TestCaseData("FlowFM_Eindhoven", "FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 0).SetName("Eindhoven");                                 // TODO: Add preconditions when the model can be correctly imported
                yield return new TestCaseData("Pudong", @"FM\FlowFM", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 0).SetName("Pudong");                                          // TODO: Add preconditions when the model can be correctly imported
                yield return new TestCaseData("Hydamo_MoergestelBroek", "moergestels_broek", new ActualCountFuncDelegate(network => network.BranchFeatures.Count()), 288).SetName("Hydamo_MoergestelBroek"); // TODO: Add preconditions when the model can be correctly imported
            }
        }
        
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string acceptanceModelPath = GuiTestHelper.IsBuildServer
                ? @"..\..\AcceptanceModels\FlowFM"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModels\FlowFM";
            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelPath);

            string acceptanceModelReferenceOutputPath = GuiTestHelper.IsBuildServer
                ? @"..\..\AcceptanceModels\FlowFM"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModelsReferenceOutput\FlowFM";
            acceptanceModelsReferenceOutputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelReferenceOutputPath);
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
                
                // [When]
                Console.WriteLine("Running model");
                ActivityRunner.RunActivity(fmModel);
                Assert.That(fmModel.Status, Is.EqualTo(ActivityStatus.Cleaned));
                
                Console.WriteLine("Saving model");
                gui.Application.SaveProjectAs(Path.Combine(tempDirectory, "SavedModel"));
                
                // [Then]
                CompareResultDataWithReferenceData(acceptanceModelName);
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

        private void CompareResultDataWithReferenceData(string acceptanceModelName)
        {
            RunModelAcceptanceTestHelper.CompareFlowFmOutput(acceptanceModelName, acceptanceModelsReferenceOutputDirectory,
                                                             tempDirectory, keepOutput);
        }
    }
}