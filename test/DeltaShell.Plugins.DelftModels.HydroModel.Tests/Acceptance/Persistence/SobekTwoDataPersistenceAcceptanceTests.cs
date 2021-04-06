using System;
using System.Collections.Generic;
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
        private string referenceSaveData;

        public static IEnumerable<TestCaseData> AcceptanceTests
        {
            get
            {
                yield return new TestCaseData("DarEsSalaam", "14", 177, 0, true).SetName("DarEsSalaam");
                yield return new TestCaseData("Raam1D", "8", 11885, 0, true).SetName("Raam1D");
                //yield return new TestCaseData("Eindhoven", "10", 0, 0, true).SetName("Eindhoven"); // #todo: fill in expected data
            }
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string basePath = GuiTestHelper.IsBuildServer
                                  ? @"..\..\"
                                  : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\";

            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModels\SOBEK2");
            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\SOBEK2");
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

                Console.WriteLine("Importing model");
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
                Console.WriteLine("Comparing saved data");
                bool hasRrData = preconditionExpectedCatchmentsCount > 0;
                string firstSaveProjectDirectory = firstSaveProjectPath + "_data";
                string secondSaveProjectDirectory = secondSaveProjectPath + "_data";
                string mduFileName = "FlowFM";
                AcceptanceModelTestHelper.CompareProjectDirectories(firstSaveProjectDirectory, secondSaveProjectDirectory, mduFileName, tempDirectory, hasRrData);

                Console.WriteLine("Comparing saved data with reference data");
                string referenceSaveDataDirectory = Path.Combine(referenceSaveData, acceptanceModelName);
                AcceptanceModelTestHelper.CompareProjectDirectories(firstSaveProjectDirectory, referenceSaveDataDirectory, mduFileName, tempDirectory, hasRrData);
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
    }
}