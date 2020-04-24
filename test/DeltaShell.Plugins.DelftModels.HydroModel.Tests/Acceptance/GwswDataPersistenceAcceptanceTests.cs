using System;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
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
            // acceptanceModelName, preconditionExpectedBranchFeaturesCount, preconditionExpectedCatchmentsCount
            new object[] {"KorteWoerden", 84, 72},
            new object[] {"DidactischStelsel", 108, 73},
            new object[] {"Groesb2", 719, 675},
            new object[] {"Pudong", 4974, 4936},
            new object[] {"Eindhoven", 0, 0} // TODO: Add preconditions when the model can be correctly imported
        };

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = TestHelper.GetTestFilePath(@"AcceptanceModels\GWSW");
        }

        [SetUp]
        public void SetUp()
        {
            var subFolder = "AcceptanceTests";
            tempDirectory = TestHelper.GetTestWorkingDirectory(subFolder);

            firstSaveProjectPath = TestHelper.GetTestWorkingDirectoryTestProjectPath("TestProject", $@"{subFolder}\First save");
            secondSaveProjectPath = TestHelper.GetTestWorkingDirectoryTestProjectPath("TestProject", $@"{subFolder}\Second save");

            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(firstSaveProjectPath), true);
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(secondSaveProjectPath), true);
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

                Console.WriteLine("Importing model");

                ImportGwswModelAndAssertPreconditions(
                    acceptanceModelName,
                    hydroModel,
                    preconditionExpectedBranchFeaturesCount,
                    preconditionExpectedCatchmentsCount, gui.Application);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(firstSaveProjectPath + "_data", "FlowFM"));
            }
        }

        private void ImportGwswModelAndAssertPreconditions(
            string acceptanceModelName, 
            IHydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount, IApplication app)
        {
            var inputDataDirectory = Path.Combine(acceptanceModelsDirectory, acceptanceModelName);

            var fileImporter = new GwswFileImporter(new DefinitionsProvider())
            {
                FilesToImport = Directory.GetFiles(inputDataDirectory)
            };

            var fileImportActivity = new FileImportActivity(fileImporter, hydroModel);
            fileImporter.LoadFeatureFiles(inputDataDirectory);

            var errorMessages = TestHelper.GetAllRenderedMessages(() =>
            {
                app.ActivityRunner.Enqueue(fileImportActivity);

                while (app.IsActivityRunningOrWaiting(fileImportActivity))
                {
                    Thread.Sleep(100);
                    ((DeltaShellApplication)app).WaitMethod();
                }

            }, Level.Error);

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