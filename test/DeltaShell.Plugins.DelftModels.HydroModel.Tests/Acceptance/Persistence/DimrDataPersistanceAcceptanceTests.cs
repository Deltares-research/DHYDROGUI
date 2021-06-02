using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("SaveLoad.dimr")]
    public class DimrDataPersistanceAcceptanceTests
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
                yield return new TestCaseData("small_HEA_FMRR", "small_HEA_FMRR", 35, 2).SetName("HEA small FM RR");
            }
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string basePath = GuiTestHelper.IsBuildServer
                                  ? @"..\..\"
                                  : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\";

            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModels\DIMR");
            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\DIMR");

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
        public void GivenRunningDeltaShellGuiWithImportedDimrModel_WhenSavingLoadingAndResavingRhuHydroModel_ThenResavedModelIsSameAsInitiallySavedModel(
            string acceptanceModelName,
            string xmlFileName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (DeltaShellGui gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                Console.WriteLine("Importing model");
                HydroModel hydroModel = DimrAcceptanceModelTestHelper.ImportDimrModelAndAssertPreconditions(acceptanceModelName,
                                                                                                            acceptanceModelsDirectory,
                                                                                                            xmlFileName,
                                                                                                            preconditionExpectedBranchFeaturesCount,
                                                                                                            preconditionExpectedCatchmentsCount,
                                                                                                            gui.Application);
                
                bool hasRrData = preconditionExpectedCatchmentsCount > 0;
                if (hasRrData)
                {
                    RainfallRunoffModel rrModel = hydroModel.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                    Assert.That(rrModel, Is.Not.Null);
                    AcceptanceModelTestHelper.EnableAllRainfallRunoffOutputSettings(rrModel);
                }
                
                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);
                
                // [Then]
                Console.WriteLine("Comparing saved data");
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
                InputFileComparer.CompareInputDirectories(referenceSaveDataDirectory,
                                                          firstSaveProjectDirectory,
                                                          mduFileName,
                                                          tempDirectory,
                                                          hasRrData,
                                                          new Dictionary<string, IEnumerable<string>>(),
                                                          AcceptanceModelTestHelper.RainfallRunoffLinesToIgnore);
            }
        }
    }
}