using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekInitialConditionsImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")]
        public void ImportInitialConditions()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("water flow fm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekInitialConditionsImporter() });

            importer.Import();

            // Assert.IsNotNull(waterFlowFmModel.InitialFlow);
            // Assert.AreEqual(37, waterFlowFmModel.InitialFlow.Locations.Values.Count);
        }
    }
}
