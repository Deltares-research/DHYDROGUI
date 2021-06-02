using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("Run.dimr")]
    public class RunDimrModelAcceptanceTest
    {
        private bool keepOutput = true;
        private string tempDirectory;
        private string acceptanceModelsDirectory;
        private string acceptanceModelsReferenceOutputDirectory;
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
            
            string acceptanceModelReferenceOutputPath = Path.Combine(basePath, @"AcceptanceModelsReferenceOutput\DIMR");
            acceptanceModelsReferenceOutputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelReferenceOutputPath);
            
            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\DIMR");

        }

        [SetUp]
        public void SetUp()
        {
            var subFolder = "AcceptanceTests";
            tempDirectory = TestHelper.GetTestWorkingDirectory(subFolder);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory);
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        public void GivenRunningDeltaShellGuiWithImportedDimrModel_WhenRunningImportedModel_ThenImportedModelHasSuccessfullyRunAndOutputIsSameAsExpectedOutput(
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
                Console.WriteLine("Running model");
                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));
                
                Console.WriteLine("Saving model");
                string savePath = Path.Combine(tempDirectory, "SavedModel");
                gui.Application.SaveProjectAs(savePath);
                
                // [Then]
                Console.WriteLine("Comparing saved input data with reference input data");
                string saveDirectory = savePath + "_data";
                string referenceSaveDataDirectory = Path.Combine(referenceSaveData, acceptanceModelName);
                string mduFileName = "FlowFM";
                InputFileComparer.CompareInputDirectories(referenceSaveDataDirectory,
                                                          saveDirectory,
                                                          mduFileName,
                                                          tempDirectory,
                                                          hasRrData,
                                                          AcceptanceModelTestHelper.GetFlowFmLinesToIgnore(mduFileName + ".mdu"),
                                                          AcceptanceModelTestHelper.RainfallRunoffLinesToIgnore);
                
                Console.WriteLine("Comparing output");
                CompareOutputWithReferenceData(acceptanceModelName, hasRrData);
            }
        }
        
        private void CompareOutputWithReferenceData(string acceptanceModelName, bool hasRrData)
        {
            RunModelAcceptanceTestHelper.CompareFlowFmOutput(acceptanceModelName, 
                                                             acceptanceModelsReferenceOutputDirectory,
                                                             tempDirectory, 
                                                             keepOutput);
            
            if (hasRrData)
            {
                Console.WriteLine("Comparing Rainfall Runoff output");
                RunModelAcceptanceTestHelper.CompareRainfallRunoffOutput(acceptanceModelName,
                                                                         acceptanceModelsReferenceOutputDirectory,
                                                                         tempDirectory,
                                                                         keepOutput);
            }
        }
    }
}