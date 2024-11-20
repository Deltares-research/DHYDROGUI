using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekControllersTriggersImporterTest
    {
        [Test, Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportControllerAndTriggers()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var realTimeControlModel = new RealTimeControlModel();
            realTimeControlModel.ControlGroups.Clear();

            var waterFlowFmModel = new WaterFlowFMModel();

            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(realTimeControlModel);
            hydroModel.Activities.Add(waterFlowFmModel);

            var partialSobekImporters = new IPartialSobekImporter[]
            {
                new SobekBranchesImporter(),
                new SobekStructuresImporter(),
                new SobekMeasurementStationsImporter(),
                new SobekControllersTriggersImporter()
            };

            PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroModel, partialSobekImporters).Import();

            Assert.AreEqual(14, realTimeControlModel.ControlGroups.Count);
        }
    }
}
