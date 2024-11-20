using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("SaveLoad.GWSW")]
    public class GwswDataPersistenceAcceptanceTests
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
                yield return new TestCaseData("KorteWoerden", 84, 72).SetName("KorteWoerden");
                yield return new TestCaseData("DidactischStelsel", 108, 74).SetName("DidactischStelsel");
                yield return new TestCaseData("Pudong", 4974, 4936).SetName("Pudong");
                yield return new TestCaseData("Eindhoven", 16529, 16131).SetName("Eindhoven");
            }
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            string basePath = GuiTestHelper.IsBuildServer
                ? @"..\..\"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\";

            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModels\GWSW");
            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\GWSW");
            
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
                HydroModel hydroModel = AcceptanceModelTestHelper.AddRhuHydroModel(gui.Application.ProjectService.Project.RootFolder);

                Console.WriteLine("Importing model");
                GwswAcceptanceModelTestHelper.ImportGwswModelAndAssertPreconditions(
                    acceptanceModelName,
                    acceptanceModelsDirectory,
                    hydroModel,
                    preconditionExpectedBranchFeaturesCount,
                    preconditionExpectedCatchmentsCount, gui.Application);

                Console.WriteLine("Setting model settings");
                SetModelSettings(hydroModel);
                
                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application.ProjectService, firstSaveProjectPath, secondSaveProjectPath);

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
                                                          hasRrData);

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
        
        private static void SetModelSettings(HydroModel hydroModel)
        {
            RainfallRunoffModel rrModel = hydroModel.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
            Assert.That(rrModel, Is.Not.Null);
            
            WaterFlowFMModel fmModel = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault();
            Assert.That(fmModel, Is.Not.Null);

            SetFlowFmModelSettings(fmModel);
            AcceptanceModelTestHelper.EnableAllRainfallRunoffOutputSettings(rrModel);
        }
        
        private static void SetFlowFmModelSettings(WaterFlowFMModel fmModel)
        {
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.UseVolumeTables, false);
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.UseVolumeTablesFile, false);
        }
    }
}