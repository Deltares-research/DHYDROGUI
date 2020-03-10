using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.ImportExport.Gwsw;
using DeltaShell.Plugins.ImportExport.GWSW;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    [TestFixture]
    [Category("Build.Acceptance")]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    public class GwswDataPersistenceAcceptanceTests
    {
        private string tempDirectory1;
        private string tempProjectPath1;
        private string tempDirectory2;
        private string tempProjectPath2;
        private string acceptanceModelsDirectory;

        private static readonly object[] AcceptanceTests =
        {
            new object[] {"KorteWoerden", 84, 72},
            new object[] {"DidactischStelsel", 108, 73},
            new object[] {"Groesb2", 719, 675},
            new object[] {"Pudong", 4974, 4936}
        };

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "AcceptanceModels", "GWSW");
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
        public void GivenRunningDeltaShellGuiWithImportedGwswModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsInitiallySavedModel(
            string acceptanceModelName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                var hydroModel = AcceptanceModelTestHelper.AddRhuHydroModel(gui.Application.Project.RootFolder);

                ImportGwswModelAndAssertPreconditions(
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

        private void ImportGwswModelAndAssertPreconditions(
            string testDataDirectory, 
            IHydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount)
        {
            var inputDataDirectory = Path.Combine(acceptanceModelsDirectory, testDataDirectory);

            var fileImporter = new GwswFileImporter(new DefinitionsProvider())
            {
                FilesToImport = Directory.GetFiles(inputDataDirectory)
            };

            fileImporter.LoadFeatureFiles(inputDataDirectory);

            var errorMessages = TestHelper.GetAllRenderedMessages(() => fileImporter.ImportItem(null, hydroModel), Level.Error);

            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the GWSW model:{Environment.NewLine}{errorMessages}");

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