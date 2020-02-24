using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.ImportExport.Sobek;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    [TestFixture]
    [Category("Build.Acceptance")]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    public class SobekTwoDataPersistenceAcceptanceTests
    {
        private string tempDirectory1;
        private string tempProjectPath1;
        private string tempDirectory2;
        private string tempProjectPath2;
        private string acceptanceModelsDirectory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "AcceptanceModels", "SOBEK2");
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
        [Ignore("Add when acceptance data is available")]
        [TestCase("DarEsSalaam", "14", 177, 0)]
        [TestCase("Waardenburg", "16", 297, 0)]
        [TestCase("HogeRaam", "9", 0, 0)] // TODO: Add preconditions and ReferenceData
        public void GivenRunningDeltaShellGuiWithImportedSobekTwoModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsAcceptanceData(
            string acceptanceModelName,
            string caseName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                var hydroModel = AcceptanceModelTestHelper.AddRhuHydroModel(gui.Application.Project.RootFolder);

                ImportSobekTwoModelAndAssertPreconditions(
                    acceptanceModelName,
                    caseName,
                    hydroModel,
                    preconditionExpectedBranchFeaturesCount,
                    preconditionExpectedCatchmentsCount);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, tempProjectPath1, tempProjectPath2);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(acceptanceModelsDirectory, acceptanceModelName, "AcceptanceData", "FlowFM"));
            }
        }

        private void ImportSobekTwoModelAndAssertPreconditions(
            string testDataDirectory,
            string caseFolder,
            IHydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount)
        {
            var caseDirectory = Path.Combine(acceptanceModelsDirectory, testDataDirectory, "InputData", caseFolder);
            var pathToNetworkFile = Path.Combine(caseDirectory, "NETWORK.TP");
            
            var sobekHydroModelImporter = new SobekHydroModelImporter(true)
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