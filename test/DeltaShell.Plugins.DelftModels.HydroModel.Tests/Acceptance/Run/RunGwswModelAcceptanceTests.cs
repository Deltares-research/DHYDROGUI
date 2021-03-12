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

            string acceptanceModelReferenceOutputPath = GuiTestHelper.IsBuildServer
                                                            ? @"..\..\AcceptanceModelsReferenceOutput\GWSW"
                                                            : @"..\..\..\nghs-1d2dflooding_AcceptanceModelData\AcceptanceModelsReferenceOutput\GWSW";
            acceptanceModelsReferenceOutputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelReferenceOutputPath);
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
                gui.Application.SaveProjectAs(Path.Combine(tempDirectory, "SavedModel"));

                // [Then]
                Console.WriteLine("Comparing output");
                CompareResultDataWithReferenceData(acceptanceModelName);
            }
        }

        private void CompareResultDataWithReferenceData(string acceptanceModelName)
        {
            RunModelAcceptanceTestHelper.CompareFlowFmOutput(acceptanceModelName, acceptanceModelsReferenceOutputDirectory,
                                                             tempDirectory, keepOutput);
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
            hydroModel.TimeStep = new TimeSpan(1, 0, 0);
        }

        private static void SetFlowFmModelSettings(WaterFlowFMModel fmModel)
        {
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.RefDate, "20200101000000");
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, "3600");
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.MapOutputDeltaT, "3600");
        }
        
        private static void SetRrModelSettings(RainfallRunoffModel rrModel)
        {
            rrModel.Precipitation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Precipitation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 0, 0, 0)));
            rrModel.Precipitation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Precipitation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 1, 0, 0)));
            rrModel.Evaporation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Evaporation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 0, 0, 0)));
            rrModel.Evaporation.Data.SetValues(
                new[] { 0.0 },
                new VariableValueFilter<DateTime>(rrModel.Evaporation.Data.Arguments[0],
                                                  new DateTime(2020, 01, 01, 1, 0, 0)));
        }
    }
}