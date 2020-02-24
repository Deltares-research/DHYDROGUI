using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    [TestFixture]
    [Category("Build.Acceptance")]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    public class FlowFmDataPersistenceAcceptanceTests
    {
        private string tempDirectory1;
        private string tempProjectPath1;
        private string tempDirectory2;
        private string tempProjectPath2;
        private string acceptanceModelsDirectory;

        private static readonly object[] AcceptanceTests =
        {
            new object[] {"Groesbeek", 0, 0}
        };

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "AcceptanceModels", "FlowFM");
        }

        [SetUp]
        public void SetUp()
        {
            tempDirectory1 = FileUtils.CreateTempDirectory();
            tempProjectPath1 = Path.Combine(tempDirectory1, "TestProject.dsproj");
            tempDirectory2 = FileUtils.CreateTempDirectory();
            tempProjectPath2 = Path.Combine(tempDirectory2, "TestProject.dsproj");
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory1);
            FileUtils.DeleteIfExists(tempDirectory2);
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        public void GivenRunningDeltaShellGuiWithImportedFlowFmModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsInitiallySavedModel(
            string acceptanceModelName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                var hydroModel = AcceptanceModelTestHelper.AddRhuHydroModel(gui.Application.Project.RootFolder);

                ImportFlowFmModelAndAssertPreconditions(
                    acceptanceModelName,
                    hydroModel,
                    preconditionExpectedBranchFeaturesCount,
                    preconditionExpectedCatchmentsCount);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, tempProjectPath1, tempProjectPath2);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(tempProjectPath1 + "_data", "FlowFM"));
            }
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        [Ignore("Add when acceptance data is available")]
        public void GivenRunningDeltaShellGuiWithImportedFlowFmModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsAcceptanceData(
            string acceptanceModelName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                var hydroModel = AcceptanceModelTestHelper.AddRhuHydroModel(gui.Application.Project.RootFolder);

                ImportFlowFmModelAndAssertPreconditions(
                    acceptanceModelName,
                    hydroModel,
                    preconditionExpectedBranchFeaturesCount,
                    preconditionExpectedCatchmentsCount);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, tempProjectPath1, tempProjectPath2);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(acceptanceModelsDirectory, acceptanceModelName, "AcceptanceData", "FlowFM"));
            }
        }

        private void ImportFlowFmModelAndAssertPreconditions(
            string testDataDirectory, 
            HydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount)
        {
            var importer = new WaterFlowFMFileImporter();
            var pathToMduFile = Path.Combine(acceptanceModelsDirectory, testDataDirectory, "InputData", "FlowFM.mdu");
           
            var errorMessages = TestHelper.GetAllRenderedMessages(() => importer.ImportItem(pathToMduFile, hydroModel), Level.Error);

            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the FlowFM model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            var hydroNetwork = hydroModel.Region.SubRegions.OfType<IHydroNetwork>().Single();
            Assert.AreEqual(expectedBranchFeaturesCount, hydroNetwork.BranchFeatures.Count(), "[Precondition failure] Unexpected number of branch features");

            // [Precondition]
            var basin = hydroModel.Region.SubRegions.OfType<IDrainageBasin>().Single();
            Assert.AreEqual(expectedCatchmentsCount, basin.AllCatchments.Count(), "[Precondition failure] Unexpected number of catchments");
        }

        private void CompareResultDataWithReferenceData(string flowFmReferenceFileDirectory)
        {
            var flowFmResultFiles = Directory.GetFiles(Path.Combine(tempProjectPath2 + "_data", "FlowFM"));
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);

            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory2);
        }
    }
}