using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekBoundaryConditionsImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportBoundaryConditions()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("water flow fm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekBoundaryConditionsImporter() });

            importer.Import();

            var boundaryConditions = waterFlowFmModel.BoundaryConditions1D;
            Assert.AreEqual(34, boundaryConditions.Count());
            Assert.AreEqual(9, boundaryConditions.Count(bc => bc.DataType != Model1DBoundaryNodeDataType.None));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportBoundaryConditions_On_Manholes_And_Add_Outlets()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Groesbeek.lit\Network.TP";
            var flowModel = new WaterFlowFMModel();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, flowModel,
                new IPartialSobekImporter[]
                {
                    new SobekBranchesImporter(),
                    new SobekBoundaryConditionsImporter()
                });
            importer.Import();

            var network = flowModel.Network;
            var manholes = network.Nodes.OfType<Manhole>();
            Assert.Greater(manholes.Count(),0);
            IList<OutletCompartment> outlets = new List<OutletCompartment>();
            foreach (var manhole in manholes)
            {
                foreach (var c in manhole.Compartments)
                {
                    var outlet = c as OutletCompartment;
                    if (outlet != null)
                    {
                        outlets.Add(outlet);
                    }
                }
            }
            Assert.Greater(outlets.Count(),0);
            var outletWCN_Oost = outlets.FirstOrDefault(o => o.Name == "WCN_Oost_2");
            Assert.IsNotNull(outletWCN_Oost);
            Assert.AreEqual(123.0, outletWCN_Oost.SurfaceWaterLevel);
        }
    }
}
