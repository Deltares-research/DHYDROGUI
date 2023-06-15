using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    [TestFixture]
    [Category(TestCategories.AcceptanceCategory)]
    [Category("Run.SOBEK")]
    public class RunSobekTwoModelAcceptanceTests
    {
        private string tempDirectory;
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
            
            string acceptanceModelPath = Path.Combine(basePath, @"AcceptanceModels\SOBEK2");
            acceptanceModelsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, acceptanceModelPath);

            referenceSaveData = Path.Combine(TestContext.CurrentContext.TestDirectory, basePath, @"AcceptanceModelsReferenceSaveData\SOBEK2");
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
        public void GivenRunningDeltaShellGuiWithImportedSobekTwoModel_WhenRunningImportedModel_ThenImportedModelHasSuccessfullyRunAndOutputFunctionsExist(
            string acceptanceModelName,
            string caseName,
            string litDirectoryName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount,
            bool isFmOnly)
        {
            // [Given]
            using (DeltaShellGui gui = AcceptanceModelTestHelper.CreateRunningDeltaShellGui())
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

                SobekAcceptanceModelTestHelper.ImportSobekTwoModelAndAssertPreconditions(acceptanceModelName,
                                                                                         caseName,
                                                                                         litDirectoryName,
                                                                                         acceptanceModelsDirectory,
                                                                                         tempDirectory,
                                                                                         hydroModel,
                                                                                         preconditionExpectedBranchFeaturesCount,
                                                                                         preconditionExpectedCatchmentsCount,
                                                                                         isFmOnly);
                
                bool hasRrData = preconditionExpectedCatchmentsCount > 0;
                if (hasRrData)
                {
                    RainfallRunoffModel rrModel = ((HydroModel)hydroModel).GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
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

                if (isFmOnly)
                    RunModelAcceptanceTestHelper.CheckFlowFMOutputFileStores((WaterFlowFMModel)hydroModel);
                else
                    RunModelAcceptanceTestHelper.CheckHydroModelOutputFileStores((HydroModel)hydroModel);
            }
        }
    }
}