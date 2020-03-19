using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.Readers;
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
        [Category(TestCategory.Slow)]
        public void ReadBoundaryConditionsAndBuildWithInterpolationTypeConstant()
        {
            // PDIN ..pdin = period and interpolation method, 0 0 or 0 1 = interpolation continuous, 1 0 or 1 1 = interpolation block 
            var initialConditionsText = @"FLBO id '1' st 0 ty 0 h_ wt 1 0 0 PDIN 1 0  pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;18:00:00' 20 < " + Environment.NewLine +
                @"'1996/01/02;00:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";

            var boundaryConditions = new SobekBoundaryConditionReader();
            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);
            var flowBoundaryCondition = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(sobekFlowBoundaryCondition);
            
            Assert.AreEqual(InterpolationType.Constant, flowBoundaryCondition.Data.Arguments[0].InterpolationType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadBoundaryConditionsAndBuildWithPeriodicExtrapolation()
        {
            //PDIN 0 1 '43200' pdin = extrapolation periodic 43200 sec (12 hours) (source sobek 2.12)
            var initialConditionsText = @"FLBO id '1' st 0 ty 0 h_ wt 1 0 0 PDIN 0 1 '43200' pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";

            var boundaryConditions = new SobekBoundaryConditionReader();
            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);
            var flowBoundaryCondition = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(sobekFlowBoundaryCondition);
            
            Assert.AreEqual(ExtrapolationType.Periodic, flowBoundaryCondition.Data.Arguments[0].ExtrapolationType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadBoundaryConditionsAndBuildWithPeriodicExtrapolation2()
        {
            //PDIN 0 1 365;00:00:00 means linear interpolation, period one year (source manual)
            var initialConditionsText = @"FLBO id '1' st 0 ty 0 h_ wt 1 0 0 PDIN 0 1 365;00:00:00 pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";

            var boundaryConditions = new SobekBoundaryConditionReader();
            var sobekFlowBoundaryCondition = boundaryConditions.GetFlowBoundaryCondition(initialConditionsText);
            var flowBoundaryCondition = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(sobekFlowBoundaryCondition);
            
            Assert.AreEqual(ExtrapolationType.Periodic, flowBoundaryCondition.Data.Arguments[0].ExtrapolationType);
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void ImportInitialConditionsLandelijkSobekModel()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\LSM1_0.lit\12\NETWORK.TP";
            var waterFlowFmModel = new WaterFlowFMModel("water flow fm");

            var partialSobekImporters = new IPartialSobekImporter[]
                                            {
                                                new SobekBranchesImporter(), // 7,5 sec
                                                new SobekBoundaryConditionsImporter()
                                            };

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, partialSobekImporters);
            TestHelper.AssertIsFasterThan(80000, importer.Import);
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
