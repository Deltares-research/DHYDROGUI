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
        private string tempDirectory;
        private string firstSaveProjectPath;
        private string secondSaveProjectPath;
        private string acceptanceModelsDirectory;

        private static readonly object[] AcceptanceTests =
        {
            new object[] {"DarEsSalaam", "14", 177, 0},
            new object[] {"Waardenburg", "27", 288, 0},
            new object[] {"HogeRaam", "9", 0, 0}, // TODO: Add preconditions when the model can be correctly imported
            new object[] {"Jakarta", "3", 0, 0} // TODO: Add preconditions when the model can be correctly imported
        };

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = TestHelper.GetTestFilePath(@"AcceptanceModels\SOBEK2");
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
        public void GivenRunningDeltaShellGuiWithImportedSobekTwoModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsInitiallySavedModel(
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
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(firstSaveProjectPath + "_data", "FlowFM"));
            }
        }

        private void ImportSobekTwoModelAndAssertPreconditions(
            string acceptanceModelName,
            string caseFolder,
            IHydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount)
        {
            var zipFilePath = Path.Combine(acceptanceModelsDirectory, acceptanceModelName + ".zip");
            var extractedModelDirectory = Path.Combine(tempDirectory, "Extracted model");

            ZipFileUtils.Extract(zipFilePath, extractedModelDirectory);

            var caseDirectory = Path.Combine(extractedModelDirectory, caseFolder);
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
            var flowFmResultFiles = Directory.GetFiles(Path.Combine(secondSaveProjectPath + "_data", "FlowFM"));
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);

            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory);
        }
    }
}