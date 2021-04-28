using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("Run.GWSW")]

    public class RunGwswModelAcceptanceTests
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
            
            string acceptanceModelPath = Path.Combine(basePath, @"AcceptanceModels\GWSW");
            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelPath);

            string acceptanceModelReferenceOutputPath = Path.Combine(basePath, @"AcceptanceModelsReferenceOutput\GWSW");
            acceptanceModelsReferenceOutputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelReferenceOutputPath);
            
            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\GWSW");
            
            
        }

        [SetUp]
        public void SetUp()
        {
            string subFolder = "AcceptanceTests";
            tempDirectory = TestHelper.GetTestWorkingDirectory(subFolder);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory);
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        public void
            GivenRunningDeltaShellGuiWithImportedGwswModel_WhenRunningImportedModel_ThenImportedModelHasSuccessfullyRunAndOutputIsSameAsExpectedOutput(
                string acceptanceModelName,
                int preconditionExpectedBranchFeaturesCount,
                int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (DeltaShellGui gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
            {
                HydroModel hydroModel = AcceptanceModelTestHelper.AddRhuHydroModel(gui.Application.Project.RootFolder);

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
                Console.WriteLine("Running model");
                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                Console.WriteLine("Saving model");
                string savePath = Path.Combine(tempDirectory, "SavedModel");
                gui.Application.SaveProjectAs(savePath);
                
                // [Then]
                Console.WriteLine("Comparing saved input data with reference input data");
                bool hasRrData = preconditionExpectedCatchmentsCount > 0;
                string saveDirectory = savePath + "_data";
                string referenceSaveDataDirectory = Path.Combine(referenceSaveData, acceptanceModelName);
                string mduFileName = "FlowFM";
                AcceptanceModelTestHelper.CompareProjectDirectories(saveDirectory,
                                                                    referenceSaveDataDirectory,
                                                                    mduFileName,
                                                                    tempDirectory,
                                                                    hasRrData,
                                                                    AcceptanceModelTestHelper.GetFlowFmLinesToIgnore(mduFileName + ".mdu"),
                                                                    AcceptanceModelTestHelper.RainfallRunoffLinesToIgnore);
                
                Console.WriteLine("Comparing output");
                CompareOutputWithReferenceData(acceptanceModelName);
            }
        }

        private void CompareOutputWithReferenceData(string acceptanceModelName)
        {
            RunModelAcceptanceTestHelper.CompareFlowFmOutput(acceptanceModelName, 
                                                             acceptanceModelsReferenceOutputDirectory,
                                                             tempDirectory, 
                                                             keepOutput);
        }
        
        private static void SetModelSettings(HydroModel hydroModel)
        {
            RainfallRunoffModel rrModel = hydroModel.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
            Assert.That(rrModel, Is.Not.Null);
            
            WaterFlowFMModel fmModel = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault();
            Assert.That(fmModel, Is.Not.Null);

            SetHydroModelSettings(hydroModel);
            SetFlowFmModelSettings(fmModel);
            SetRrModelSettings(rrModel);
        }
        
        private static void SetHydroModelSettings(HydroModel hydroModel)
        {
            hydroModel.StartTime = new DateTime(2020, 01, 01, 0, 0, 0);
            hydroModel.StopTime = new DateTime(2020, 01, 01, 1, 0, 0);
        }

        private static void SetFlowFmModelSettings(WaterFlowFMModel fmModel)
        {
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.RefDate, "20200101000000");
        }
        
        private static void SetRrModelSettings(RainfallRunoffModel rrModel)
        {
            DateTime startDateTime = new DateTime(2020, 01, 01, 0, 0, 0);
            int offsetInMinutes = 0;
            
            var precipitationValues = new [] {5.0, 7.5, 10.0, 5.0, 0.0, 0.0};

            foreach (double precipitationValue in precipitationValues)
            {
                var dateTime = startDateTime.AddMinutes(offsetInMinutes);
                
                rrModel.Precipitation.Data.SetValues(
                    new[] { precipitationValue},
                    new VariableValueFilter<DateTime>(rrModel.Precipitation.Data.Arguments[0], dateTime));

                offsetInMinutes += 10; // Add 10 minutes.
            }
        }
    }
}