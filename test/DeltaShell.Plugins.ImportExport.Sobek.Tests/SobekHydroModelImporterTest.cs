using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekHydroModelImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SimpleSequentialWorkflow()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\demo_01.lit\1\NETWORK.TP";

            var hydroModel = CreateHydroModel();
            var realTimeControlModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
            hydroModel.Activities.Remove(realTimeControlModel);
            
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(false, false, true)
                {
                    TargetObject = hydroModel,
                    PartialSobekImporter = importer,
                    PathSobek = pathToSobekModel
                };
            sobekModelImporter.Import();

            Assert.AreEqual("(RR + FlowFM)", hydroModel.CurrentWorkflow.ToString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SimpleParallelWorkflow()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\164_000.lit\2\NETWORK.TP";

            var hydroModel = CreateHydroModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(true, true, true)
                {
                    TargetObject = hydroModel,
                    PartialSobekImporter = importer,
                    PathSobek = pathToSobekModel
                };
            sobekModelImporter.Import();

            Assert.AreEqual("(RR + RTC + FlowFM)", hydroModel.CurrentWorkflow.ToString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ComplexWorkflow()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\DWAQ_AC1\DWAQ_AC1.lit\37\NETWORK.TP";

            var hydroModel = CreateHydroModel();
            var realTimeControlModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
            hydroModel.Activities.Remove(realTimeControlModel);
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(true, false, true)
                {
                    TargetObject = hydroModel,
                    PartialSobekImporter = importer,
                    PathSobek = pathToSobekModel
                };

            sobekModelImporter.Import();

            Assert.AreEqual("(RR + FlowFM)", hydroModel.CurrentWorkflow.ToString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportSobekModelWithSalinityThenRemoveWaterFlow1DShouldNotCrash()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\SOBEK3-1015\6\DEFTOP.1";

            var hydroModel = CreateHydroModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(false)
            {
                TargetObject = hydroModel,
                PartialSobekImporter = importer,
                PathSobek = pathToSobekModel
            };

            sobekModelImporter.Import();

            var acts1D = hydroModel.Activities.GetActivitiesOfType<WaterFlowFMModel>().ToList();
            Assert.NotNull(acts1D);
            Assert.IsNotEmpty(acts1D);
            var actToRemove = acts1D.First();
            Assert.DoesNotThrow(() => hydroModel.CurrentWorkflow.Activities.Remove(actToRemove));
            Assert.That(!hydroModel.CurrentWorkflow.Activities.Contains(actToRemove));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestImportSobekModel_CompositeStructureNamesAreUnique()
        {
            var pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\SOBEK3-1015\6\DEFTOP.1";

            var hydroModel = CreateHydroModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(false)
            {
                TargetObject = hydroModel,
                PartialSobekImporter = importer,
                PathSobek = pathToSobekModel
            };

            sobekModelImporter.Import();

            var network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault();
            Assert.NotNull(network);
            var compositeStructures = network.CompositeBranchStructures.ToList();

            Assert.IsTrue(compositeStructures.Count > 1);
            Assert.IsTrue(compositeStructures.Select(cbs => cbs.Name).HasUniqueValues());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenAHydroModel_WhenImportingSobekModelInWaterFlowFmModel_ThenWriteRestartFileModelSettingIsFalse()
        {
            // Setup
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\demo_01.lit\1\NETWORK.TP";
            
            HydroModel hydroModel = CreateHydroModel();
            
            IPartialSobekImporter importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(false)
            {
                TargetObject = hydroModel,
                PartialSobekImporter = importer,
                PathSobek = pathToSobekModel
            };
            
            // Call
            sobekModelImporter.ImportItem(pathToSobekModel, hydroModel);
            
            // Assert
            IEnumerable<WaterFlowFMModel> waterFlowFmModels = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>();
            Assert.That(waterFlowFmModels.Count(), Is.EqualTo(1));

            WaterFlowFMModel waterFlowFmModel = waterFlowFmModels.Single();
            List<double> writeRestartFile = (List<double>)waterFlowFmModel.ModelDefinition.GetModelProperty(KnownProperties.RstInterval).Value;
            Assert.That(writeRestartFile.Count, Is.EqualTo(1));
            Assert.That(writeRestartFile.First(), Is.Zero);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenAHydroModel_WhenImportingModelsWithDifferentTimeSettings_ThenHydroModelDoesNotOverrideTheseTimes()
        {
            // Setup
            string testDir = TestHelper.GetTestDataDirectory() + @"\demo_01.lit";
            HydroModel hydroModel = CreateHydroModel();
            
            using (var temp = new TemporaryDirectory())
            {
                string directory = temp.CopyDirectoryToTempDirectory(testDir);
                string filePath = Path.Combine(directory, "1", "NETWORK.TP");
                
                IPartialSobekImporter partialImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(filePath, hydroModel);
                var importer = new SobekHydroModelImporter(false)
                {
                    TargetObject = hydroModel,
                    PartialSobekImporter = partialImporter,
                    PathSobek = filePath
                };
                
                // Call
                importer.ImportItem(filePath, hydroModel);
            }
            
            // Assert
            Assert.That(hydroModel.StartTime, Is.EqualTo(new DateTime(2021, 1, 1)));
            Assert.That(hydroModel.StopTime, Is.EqualTo(new DateTime(2021, 1, 2)));
            Assert.That(hydroModel.TimeStep, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(hydroModel.OverrideStartTime, Is.False);
            Assert.That(hydroModel.OverrideStopTime, Is.False);
            
            WaterFlowFMModel waterFlowFMModel = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>().Single();
            Assert.That(waterFlowFMModel.StartTime, Is.EqualTo(new DateTime(1996, 1, 1)));
            Assert.That(waterFlowFMModel.StopTime, Is.EqualTo(new DateTime(1996, 1, 15)));
            Assert.That(waterFlowFMModel.TimeStep, Is.EqualTo(TimeSpan.FromHours(1)));
            
            RainfallRunoffModel rainFallRunoffModel = hydroModel.GetAllActivitiesRecursive<RainfallRunoffModel>().Single();
            Assert.That(rainFallRunoffModel.StartTime, Is.EqualTo(new DateTime(2021, 1, 1)));
            Assert.That(rainFallRunoffModel.StopTime, Is.EqualTo(new DateTime(2021, 1, 2)));
            Assert.That(rainFallRunoffModel.TimeStep, Is.EqualTo(TimeSpan.FromHours(1)));
        }
        

        private static HydroModel CreateHydroModel()
        {
            var builder = new HydroModelBuilder();
            return builder.BuildModel(ModelGroup.SobekModels);
        }
    }
}