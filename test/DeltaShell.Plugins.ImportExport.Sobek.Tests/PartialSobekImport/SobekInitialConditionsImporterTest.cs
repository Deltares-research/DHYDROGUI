using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekInitialConditionsImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportInitialConditions()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowModel1DModel = new WaterFlowModel1D("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowModel1DModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekInitialConditionsImporter() });

            importer.Import();

            Assert.IsNotNull(waterFlowModel1DModel.InitialFlow);
            Assert.AreEqual(37, waterFlowModel1DModel.InitialFlow.Locations.Values.Count);
        }
    }
}
