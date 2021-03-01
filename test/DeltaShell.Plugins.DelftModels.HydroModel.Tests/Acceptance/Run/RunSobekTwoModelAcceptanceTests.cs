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
using DeltaShell.Plugins.ImportExport.Sobek;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("Run.SOBEK")]
    public class RunSobekTwoModelAcceptanceTests
    {
        private bool keepOutput = false;
        private string tempDirectory;
        private string acceptanceModelsDirectory;
        private string acceptanceModelsReferenceOutputDirectory;

        public static IEnumerable<TestCaseData> AcceptanceTests
        {
            get
            {
                yield return new TestCaseData("DarEsSalaam", "14", 177, 0, true).SetName("DarEsSalaam");
                yield return new TestCaseData("Jakarta", "3", 4148, 0, false).SetName("Jakarta");
                yield return new TestCaseData("Tholen", "1", 0, 0, false).SetName("Tholen");       // todo: fill in # of expected branch features
                yield return new TestCaseData("HogeRaam", "9", 1477, 0, true).SetName("HogeRaam"); // todo: fill in # of expected catchments

            }
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string acceptanceModelPath = GuiTestHelper.IsBuildServer
                ? @"..\..\AcceptanceModels\FlowFM"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModels\SOBEK2";
            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelPath);

            string acceptanceModelReferenceOutputPath = GuiTestHelper.IsBuildServer
                ? @"..\..\AcceptanceModels\FlowFM"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModelsReferenceOutput\SOBEK2";
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
        public void GivenRunningDeltaShellGuiWithImportedSobekTwoModel_WhenRunningImportedModel_ThenImportedModelHasSuccessfullyRunAndOutputIsSameAsExpectedOutput(
            string acceptanceModelName,
            string caseName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount,
            bool isFmOnly)
        {
            // [Given]
            using (DeltaShellGui gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                IHydroModel hydroModel;
                if (isFmOnly)
                {
                    hydroModel = new WaterFlowFMModel();
                    gui.Application.Project.RootFolder.Add(hydroModel);
                }
                else
                {
                    hydroModel = AcceptanceModelTestHelper.AddRhuHydroModel(gui.Application.Project.RootFolder);
                }

                ImportSobekTwoModelAndAssertPreconditions(
                    acceptanceModelName,
                    caseName,
                    hydroModel,
                    preconditionExpectedBranchFeaturesCount,
                    preconditionExpectedCatchmentsCount,
                    isFmOnly);
                
                // [When]
                Console.WriteLine("Running model");
                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                Console.WriteLine("Saving model");
                gui.Application.SaveProjectAs(Path.Combine(tempDirectory, "SavedModel"));

                // [Then]
                CompareResultDataWithReferenceData(acceptanceModelName);
            }
        }

        private void ImportSobekTwoModelAndAssertPreconditions(
            string acceptanceModelName,
            string caseFolder,
            IHydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount,
            bool isFmOnly)
        {
            var zipFilePath = Path.Combine(acceptanceModelsDirectory, acceptanceModelName + ".zip");
            var extractedModelDirectory = Path.Combine(tempDirectory, "Extracted model");

            ZipFileUtils.Extract(zipFilePath, extractedModelDirectory);

            var caseDirectory = Path.Combine(extractedModelDirectory, caseFolder);
            var pathToNetworkFile = Path.Combine(caseDirectory, "NETWORK.TP");
            
            var sobekHydroModelImporter = new SobekHydroModelImporter(false)
            {
                TargetObject = hydroModel,
                PartialSobekImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToNetworkFile, hydroModel),
                PathSobek = pathToNetworkFile
            };

            var errorMessages = TestHelper.GetAllRenderedMessages(() => sobekHydroModelImporter.Import(), Level.Error);

            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the SOBEK2 model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            var hydroNetwork = hydroModel.Region.SubRegions.OfType<IHydroNetwork>().Single();
            Assert.AreEqual(expectedBranchFeaturesCount, hydroNetwork.BranchFeatures.Count(), "[Precondition failure] Unexpected number of branch features");

            // [Precondition]
            if (!isFmOnly)
            {
                var basin = hydroModel.Region.SubRegions.OfType<IDrainageBasin>().Single();
                Assert.AreEqual(expectedCatchmentsCount, basin.AllCatchments.Count(), "[Precondition failure] Unexpected number of catchments");
            }
        }
        
        private void CompareResultDataWithReferenceData(string acceptanceModelName)
        {
            RunModelAcceptanceTestHelper.CompareFlowFmOutput(acceptanceModelName, acceptanceModelsReferenceOutputDirectory,
                                                             tempDirectory, keepOutput);
        }
    }
}