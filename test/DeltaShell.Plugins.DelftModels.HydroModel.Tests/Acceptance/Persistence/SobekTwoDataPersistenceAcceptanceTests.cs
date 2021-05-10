using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM;
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
                yield return new TestCaseData("DarEsSalaam", "14", "DarEs1D.lit", 177, 0, true).SetName("DarEsSalaam");
                yield return new TestCaseData("Raam1D", "8", "Raam1D.lit", 11885, 0, true).SetName("Raam1D");
                yield return new TestCaseData("HEAs1DFM", "19", "HEAs1DFM.lit", 37, 0, true).SetName("Small Hunze&Aas 1D");
                yield return new TestCaseData("HEA_FM_RR", "15", "HEAs1DRR.lit", 35, 2, false).SetName("Small Hunze&Aas 1D + RR");
                //yield return new TestCaseData("Eindhoven", "10", "Eindho.lit", 0, 0, true).SetName("Eindhoven"); // #todo: fill in expected data
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
            string litDirectoryName,
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
                SobekAcceptanceModelTestHelper.ImportSobekTwoModelAndAssertPreconditions(acceptanceModelName,
                                                                                         caseName,
                                                                                         litDirectoryName,
                                                                                         acceptanceModelsDirectory,
                                                                                         tempDirectory,
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
                InputFileComparer.CompareInputDirectories(firstSaveProjectDirectory, 
                                                          secondSaveProjectDirectory, 
                                                          mduFileName, 
                                                          tempDirectory, 
                                                          hasRrData,
                                                          new Dictionary<string, IEnumerable<string>>(),
                                                          AcceptanceModelTestHelper.RainfallRunoffLinesToIgnore);

                Console.WriteLine("Comparing saved data with reference data");
                string referenceSaveDataDirectory = Path.Combine(referenceSaveData, acceptanceModelName);
                InputFileComparer.CompareInputDirectories(firstSaveProjectDirectory,
                                                          referenceSaveDataDirectory,
                                                          mduFileName,
                                                          tempDirectory,
                                                          hasRrData,
                                                          new Dictionary<string, IEnumerable<string>>(),
                                                          AcceptanceModelTestHelper.RainfallRunoffLinesToIgnore);
            }
        }
    }
}