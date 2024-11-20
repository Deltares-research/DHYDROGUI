using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("Run.dimr")]
    public class RunDimrModelAcceptanceTest
    {
        private string tempDirectory;
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
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory);
        }

        [Test]
        [TestCaseSource(nameof(AcceptanceTests))]
        public void GivenRunningDeltaShellGuiWithImportedDimrModel_WhenRunningImportedModel_ThenImportedModelHasSuccessfullyRunAndOutputFunctionsExist(
            string acceptanceModelName,
            string xmlFileName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount)
        {
            // [Given]
            using (IGui gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
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
                Console.WriteLine("Checking output");
                RunModelAcceptanceTestHelper.CheckHydroModelOutputFileStores(hydroModel);
            }
        }
    }
}