using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
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
        private string tempDirectory;
        private string acceptanceModelsDirectory;
        private string referenceSaveData;

        public static IEnumerable<TestCaseData> AcceptanceTests
        {
            get
            {
                yield return new TestCaseData("KorteWoerden", 84, 72).SetName("KorteWoerden");
                yield return new TestCaseData("DidactischStelsel", 108, 74).SetName("DidactischStelsel");
                yield return new TestCaseData("Pudong", 4974, 4936).SetName("Pudong");
                yield return new TestCaseData("Eindhoven", 16529, 16131).SetName("Eindhoven").Ignore("Currently failing since issue FM1D2D-1937. Model needs to be updated.");
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
            GivenRunningDeltaShellGuiWithImportedGwswModel_WhenRunningImportedModel_ThenImportedModelHasSuccessfullyRunAndOutputFunctionsExist(
                string acceptanceModelName,
                int preconditionExpectedBranchFeaturesCount,
                int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (IGui gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
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
                var exportConfig = new ModelArtifactsExporterConfig(

                    gui.Application.WorkDirectory,
                    hydroModel.Name,
                    TestContext.CurrentContext.Test.Name
                );
                var artifactsExporter = new ModelArtifactsExporter(exportConfig);
                
                Console.WriteLine("Running model");
                ActivityRunner.RunActivity(hydroModel);
                artifactsExporter.ExportModelLogFiles();
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                // [Then]
                Console.WriteLine("Comparing output");
                RunModelAcceptanceTestHelper.CheckHydroModelOutputFileStores(hydroModel);
            }
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
            AcceptanceModelTestHelper.EnableAllRainfallRunoffOutputSettings(rrModel);
        }
        
        private static void SetHydroModelSettings(HydroModel hydroModel)
        {
            hydroModel.StartTime = new DateTime(2020, 01, 01, 0, 0, 0);
            hydroModel.StopTime = new DateTime(2020, 01, 01, 1, 0, 0); // 1 hour simulation
        }

        private static void SetFlowFmModelSettings(WaterFlowFMModel fmModel)
        {
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.UseVolumeTables, false);
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.UseVolumeTablesFile, false);
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.RefDate, "20200101");
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.HisInterval, "1200"); // 20 minutes output step
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, "1200"); // 20 minutes output step
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.MapInterval, "1200"); // 20 minutes output step
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.MapOutputDeltaT, "1200"); // 20 minutes output step
        }
        
        private static void SetRrModelSettings(RainfallRunoffModel rrModel)
        {
            OverwriteDefaultEvaporationData(rrModel.Evaporation.Data);
            SetPrecipitationValues(rrModel);
        }

        private static void SetPrecipitationValues(RainfallRunoffModel rrModel)
        {
            DateTime startDateTime = new DateTime(2020, 01, 01, 0, 0, 0);
            int offsetInMinutes = 0;

            var precipitationValues = new[]
            {
                5.0,
                7.5,
                10.0,
                5.0,
                0.0,
                0.0
            };

            foreach (double precipitationValue in precipitationValues)
            {
                var dateTime = startDateTime.AddMinutes(offsetInMinutes);

                rrModel.Precipitation.Data.SetValues(
                    new[]
                    {
                        precipitationValue
                    },
                    new VariableValueFilter<DateTime>(rrModel.Precipitation.Data.Arguments[0], dateTime));

                offsetInMinutes += 10; // Add 10 minutes.
            }
        }

        private static void OverwriteDefaultEvaporationData(IFunction data)
        {
            var timeArgument = data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
            if (timeArgument != null)
            {

                var startDate = new DateTime(1980, 01, 01);
                var endDate = new DateTime(2030, 01, 01);
                var dates = new List<DateTime>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    dates.Add(currentDate);
                    currentDate = currentDate.AddDays(1);
                }

                timeArgument.Clear();
                timeArgument.SetValues(dates);
            }
        }
    }
}