using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekHydroModelImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void SimpleSequentialWorkflow()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\demo_01.lit\1\NETWORK.TP";

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
        [Category("Quarantine")]
        public void SimpleParallelWorkflow()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\ZBOtest.lit\7\NETWORK.TP";

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
        [Category("Quarantine")]
        public void ComplexWorkflow()
        {
            string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\DWAQ_AC1\DWAQ_AC1.lit\37\NETWORK.TP";

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
        [Category("Quarantine")]
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
        [Category("Quarantine")]
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

        private static HydroModel CreateHydroModel()
        {
            var builder = new HydroModelBuilder();
            return builder.BuildModel(ModelGroup.SobekModels);
        }
    }
}