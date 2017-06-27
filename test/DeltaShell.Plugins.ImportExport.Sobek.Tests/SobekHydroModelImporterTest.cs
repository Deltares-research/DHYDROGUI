using System;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
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
            string pathToSobekModel = TestHelper.GetDataDir() + @"\demo_01.lit\1\NETWORK.TP";

            var hydroModel = CreateHydroModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(false, false, true)
                {
                    TargetObject = hydroModel,
                    PartialSobekImporter = importer,
                    PathSobek = pathToSobekModel
                };
            sobekModelImporter.Import();

            Assert.AreEqual("(RR + Flow1D)", hydroModel.CurrentWorkflow.ToString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SimpleParallelWorkflow()
        {
            string pathToSobekModel = TestHelper.GetDataDir() + @"\ZBOtest.lit\7\NETWORK.TP";

            var hydroModel = CreateHydroModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(true, true, true)
                {
                    TargetObject = hydroModel,
                    PartialSobekImporter = importer,
                    PathSobek = pathToSobekModel
                };
            sobekModelImporter.Import();

            Assert.AreEqual("RR + (RTC + Flow1D)", hydroModel.CurrentWorkflow.ToString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ComplexWorkflow()
        {
            string pathToSobekModel = TestHelper.GetDataDir() + @"\DWAQ_AC1\DWAQ_AC1.lit\37\NETWORK.TP";

            var hydroModel = CreateHydroModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(true, false, true)
                {
                    TargetObject = hydroModel,
                    PartialSobekImporter = importer,
                    PathSobek = pathToSobekModel
                };

            sobekModelImporter.Import();

            Assert.AreEqual("(RR + Flow1D)", hydroModel.CurrentWorkflow.ToString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportSobekModelWithSalinityThenRemoveWaterFlow1DShouldNotCrash()
        {
            string pathToSobekModel = TestHelper.GetDataDir() + @"\SOBEK3-1015\6\DEFTOP.1";

            var hydroModel = CreateHydroModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekModel, hydroModel);
            var sobekModelImporter = new SobekHydroModelImporter(false)
            {
                TargetObject = hydroModel,
                PartialSobekImporter = importer,
                PathSobek = pathToSobekModel
            };

            sobekModelImporter.Import();

            var acts1D = hydroModel.Activities.GetActivitiesOfType<WaterFlowModel1D>().ToList();
            Assert.NotNull(acts1D);
            Assert.IsNotEmpty(acts1D);

            Assert.DoesNotThrow(() => hydroModel.CurrentWorkflow.Activities.Remove(acts1D.First()));
        }

        private static HydroModel CreateHydroModel()
        {
            var builder = new HydroModelBuilder();
            return builder.BuildModel(ModelGroup.SobekModels);
        }
    }
}