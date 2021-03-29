using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
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
            string acceptanceModelPath = GuiTestHelper.IsBuildServer
                ? @"..\..\AcceptanceModels\GWSW"
                : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModels\GWSW";

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

                GwswAcceptanceModelTestHelper.ImportGwswModelAndAssertPreconditions(
                    acceptanceModelName,
                    acceptanceModelsDirectory,
                    hydroModel,
                    preconditionExpectedBranchFeaturesCount,
                    preconditionExpectedCatchmentsCount, gui.Application);

                // [When]
                AcceptanceModelTestHelper.SaveLoadAndResaveProject(gui.Application, firstSaveProjectPath, secondSaveProjectPath);

                // [Then]
                CompareResultDataWithReferenceData(Path.Combine(firstSaveProjectPath + "_data", "FlowFM", "input"));
            }
        }
        
        private void CompareResultDataWithReferenceData(string flowFmReferenceFileDirectory)
        {
            var flowFmReferenceFiles = Directory.GetFiles(flowFmReferenceFileDirectory);
            if (!flowFmReferenceFiles.Any())
            {
                Assert.Fail($"No saved files (first save) could be found at {flowFmReferenceFiles}.");
            }
            
            var secondSaveDirectory = Path.Combine(secondSaveProjectPath + "_data", "FlowFM", "input");
            var flowFmResultFiles = Directory.GetFiles(secondSaveDirectory);
            if (!flowFmResultFiles.Any())
            {
                Assert.Fail($"No saved files (second save) could be found at {secondSaveDirectory}.");
            }
            
            FlowFmFileComparer.Compare(flowFmReferenceFiles, flowFmResultFiles, tempDirectory);
        }
    }
}