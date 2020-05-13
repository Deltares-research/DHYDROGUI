using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    [TestFixture]
    [Category("Build.Acceptance")]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    public class RunGwswModelAcceptanceTests
    {
        private string acceptanceModelsDirectory;

        private static readonly object[] AcceptanceTests =
        {
            // acceptanceModelName, preconditionExpectedBranchFeaturesCount, preconditionExpectedCatchmentsCount
            new object[] {"KorteWoerden", 84, 72},
            new object[] {"DidactischStelsel", 108, 74},
            new object[] {"Groesb2", 719, 675},
            new object[] {"Pudong", 4974, 4936},
            new object[] {"Eindhoven", 16529, 16131}
        };

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            acceptanceModelsDirectory = TestHelper.GetTestFilePath(@"AcceptanceModels\GWSW");
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        public void
            GivenRunningDeltaShellGuiWithImportedGwswModel_WhenRunningRhuHydroModel_ThenHydroModelHasSuccessfullyRun(
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


                Console.WriteLine("Setting model settings");
                SetModelSettings(hydroModel);


                // [When]
                Console.WriteLine("Running model");
                ActivityRunner.RunActivity(hydroModel);

                // [Then]
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));
            }
        }

        private void SetModelSettings(HydroModel hydroModel)
        {
            var rrModel = hydroModel.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
            Assert.That(rrModel, Is.Not.Null);
            var fmModel = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault();
            Assert.That(fmModel, Is.Not.Null);

            hydroModel.StartTime = new DateTime(2020, 01, 01, 0, 0, 0);
            hydroModel.StopTime = new DateTime(2020, 01, 01, 1, 0, 0);
            hydroModel.TimeStep = new TimeSpan(1, 0, 0);

            fmModel.ModelDefinition.SetModelProperty(KnownProperties.RefDate, "20200101000000");
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, "3600");
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.MapOutputDeltaT, "3600");

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