using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category( "SaveLoad.SOBEK")]
    public class SobekTwoDataPersistenceAcceptanceTests
    {
        private string tempDirectory;
        private string firstSaveProjectPath;
        private string secondSaveProjectPath;
        private string acceptanceModelsDirectory;
        private static readonly object[] AcceptanceTests =
        {
            new object[] {"DarEsSalaam", "14", 177, 0, true},
            new object[] {"Jakarta", "3", 4148, 0, false},
            new object[] {"Tholen", "1", 0, 0, false}, // todo: fill in # of expected branch features
            new object[] {"HogeRaam", "9", 1477, 0, true}, // todo: fill in # of expected catchments
        };

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string acceptanceModelPath = GuiTestHelper.IsBuildServer
                ? @"..\..\AcceptanceModels\SOBEK2"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModels\SOBEK2";

            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelPath);
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
            int preconditionExpectedCatchmentsCount,
            bool isFmOnly)
        {
            // [Given]
            using (var gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
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

        private void CompareResultDataWithReferenceData(string flowFmReferenceFileDirectory)
        {
            var flowFmResultFiles = Directory.GetFiles(Path.Combine(secondSaveProjectPath + "_data", "FlowFM"));
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);

            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory);
        }
    }
}