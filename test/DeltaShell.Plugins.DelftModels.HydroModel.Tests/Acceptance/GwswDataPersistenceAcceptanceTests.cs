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
        private string tempDirectory;
        private string firstSaveProjectPath;
        private string secondSaveProjectPath;
        private string acceptanceModelsDirectory;

        private static readonly object[] AcceptanceTests =
        {
            new object[] {"KorteWoerden", 84, 72},
            new object[] {"DidactischStelsel", 108, 73},
            new object[] {"Groesb2", 719, 675},
            new object[] {"Pudong", 4974, 4936},
            new object[] {"Eindhoven", 0, 0} // TODO: Add preconditions when the model can be correctly imported
        };

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "AcceptanceModels", "GWSW");
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
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(firstSaveProjectPath + "_data", "FlowFM"));
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
            var flowFmResultFiles = Directory.GetFiles(Path.Combine(secondSaveProjectPath + "_data", "FlowFM"));
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);

            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory);
        }
    }
}