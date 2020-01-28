using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekWaterFlowModel1DImporterNetworkTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportNetworkLinkageNodes()
        {
            // this model is used as testmodel for the PI to Sobek conversion and as such has used several options 
            // of Sobek:
            //  
            //                        CN
            //                        |
            //                        W      CN
            //                  (br4) |      | (br5)
            //                        CS     CS
            //                        |      |
            //  BN---CS---W----LN-----LN-----LN---COMP---LN---COMP---CS---BN  <--(br1)
            //               / |                   =      |    =
            //              /  |                   RW    CS    RW
            //             /   |                   GS     |    DS
            //             W   P                         CN
            //       (br2) |   | (br3)
            //             CS  CS                       (br6)
            //             \   |
            //              \  |
            //               \ |
            //                CN
            //
            // where 
            //   BN = Boundary Node
            //   LN = Linkage Node
            //   CN = Connection Node with storage and lateral flow 
            //   CS = Cross Section
            //    W = Weir
            //    P = Pump Station 
            // COMP = compound structure
            //   RW = River Weir
            //   GS = General Structure
            //   DS = Database structure

            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\LinkageNodes\network.tp";
            var modelImporter = new SobekModelToIntegratedModelImporter();

            // The expected result is that the original branch 1 is split in 5 branches
            var waterFlowFmModel = (WaterFlowFMModel)modelImporter.ImportItem(pathToSobekNetwork);
            IHydroNetwork network = waterFlowFmModel.Network;
            Assert.AreEqual(10, network.Branches.Count);
            Assert.AreEqual(10, network.Nodes.Count);

            // For the orignal branch a reach is added. All created subbranches are part 
            // of the newly created reach.
            // Assert.AreEqual(1, network.Reaches.Count);
            // Assert.AreEqual(5, network.Reaches[0].Count);
            // the original branch with the 4 linkage nodes is now the left branch at position 0

            var length = 0.0;

            var channels = network.Channels.ToList();
            var branch = channels.First(b => b.Name == "1_A");
            Assert.AreEqual(1, branch.BranchFeatures.Count());
            // Assert.AreEqual(network.Reaches[0], branch.Reach);
            length += branch.Length;

            // Three extra branches are added, all without a cross section
            branch = channels.First(b => b.Name == "1_B_A");
            Assert.AreEqual(0, branch.CrossSections.Count());
            // Assert.AreEqual(network.Reaches[0], branch.Reach);
            length += branch.Length;

            branch = channels.First(b => b.Name == "1_B_B_A");
            Assert.AreEqual(0, branch.CrossSections.Count());
            // Assert.AreEqual(network.Reaches[0], branch.Reach);
            length += branch.Length;

            branch = channels.First(b => b.Name == "1_B_B_B_A");
            Assert.AreEqual(0, branch.CrossSections.Count());
            // Assert.AreEqual(network.Reaches[0], branch.Reach);
            length += branch.Length;


            // The last branch has 1 cs
            branch = channels.First(b => b.Name == "1_B_B_B_B");
            Assert.AreEqual(1, branch.CrossSections.Count());
            // Assert.AreEqual(network.Reaches[0], branch.Reach);
            length += branch.Length;

            Assert.Less(Math.Abs(length - 20000), 1.0e-4);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportReadsAllCrossSections()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\NetworkWithStructures\network.tp";
            var modelImporter = new SobekModelToIntegratedModelImporter();
            
            var waterFlowFmModel = (WaterFlowFMModel)modelImporter.ImportItem(pathToSobekNetwork);
            IHydroNetwork network = waterFlowFmModel.Network;
            Assert.IsNotNull(network);
            int actualNumberOfCrossSections = network.CrossSections.Count();
            // of the 105 profiles only 27 are supported and processed
            const int exptectedNumberOfCrossSections = 27;
            Assert.GreaterOrEqual(actualNumberOfCrossSections, exptectedNumberOfCrossSections);

            //Assert.AreEqual(17, network.SharedCrossSectionDefinitions.Count); circle and eggshape cross-section wait for implementation closed branch
            Assert.AreEqual(11, network.SharedCrossSectionDefinitions.Count);
            var numberOfProxiedCrossSections = network.CrossSections.Count(cs => cs.Definition.IsProxy);

            //Assert.AreEqual(98, numberOfProxiedCrossSections); circle and eggshape cross-section wait for implementation closed branch
            Assert.AreEqual(43, numberOfProxiedCrossSections);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportNetworkWithWeirs()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\NetworkWithStructures\network.tp";
            var modelImporter = new SobekModelToIntegratedModelImporter();
            var waterFlowFmModel = (WaterFlowFMModel)modelImporter.ImportItem(pathToSobekNetwork);
            IHydroNetwork network = waterFlowFmModel.Network;
            Assert.IsNotNull(network);
            
            IEnumerable<Weir> weirs = network.Structures.OfType<Weir>();
            Assert.IsNotNull(weirs);            
            Assert.Greater(network.Structures.Count(), 0);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportBridgeAndCulvertFrictionsTest()
        {
            //STFR id '5' ci '5' mf 3 mt cp 0 35 0 mr cp 0 35 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 stfr
            //STFR id '4' ci '4' mf 1 mt cp 0 0.022 0 mr cp 0 0.022 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 stfr
            var path = TestHelper.GetTestDataDirectory() + @"\StrucFr2.lit\2\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(path);

            Assert.AreEqual(1, network.Bridges.Count());
            var bridge = network.Structures.Where(s => s.Name == "4").First(); // river bridge id != def id
            Assert.IsInstanceOfType(typeof(Bridge), bridge);
            Assert.AreEqual(BridgeFrictionType.Manning, ((Bridge)bridge).FrictionType);
            Assert.AreEqual(0.022, ((Bridge)bridge).Friction);

            Assert.AreEqual(1, network.Culverts.Count());
            var culvert = network.Structures.Where(s => s.Name == "5").First(); // river culvert id != def id
            Assert.IsInstanceOfType(typeof(Culvert), culvert);
            Assert.AreEqual(CulvertFrictionType.StricklerKs, ((Culvert)culvert).FrictionType);
            Assert.AreEqual(35, ((Culvert)culvert).Friction);
        }

        [Test]
        public void ReadAndConvertLateralFlowWithInterpolationTypeConstant()
        {
            // PDIN ..pdin = period and interpolation method, 0 0 or 0 1 = interpolation continuous, 1 0 or 1 1 = interpolation block 
            string initialConditionsText =
                @"FLBR id '5' sc 0 lt 0 dc lt 0 50 0 flbr" + Environment.NewLine +
                @"FLBR id '6' sc 0 lt 0 dc lt 1 0 0 PDIN 1 0  pdin" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"'1996/01/01;00:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;06:00:00' 15 < " + Environment.NewLine +
                @"'1996/01/01;12:00:00' 10 < " + Environment.NewLine +
                @"'1996/01/01;18:00:00' 20 < " + Environment.NewLine +
                @"'1996/01/02;00:00:00' 10 < " + Environment.NewLine +
                @"tble flbo";
            var reader = new SobekLateralFlowReader();

            var lateralFlow = reader.GetLateralFlow(initialConditionsText);

            Model1DLateralSourceData model1DLateralSourceData = new Model1DLateralSourceData();
            SobekLateralSourcesDataImporter.ConvertToLateralSourceData(lateralFlow, model1DLateralSourceData);
            Assert.AreEqual(InterpolationType.Constant, model1DLateralSourceData.Data.Arguments[0].InterpolationType);
        }

        [Test]
        public void ReadAndConvertLateralFlowWithQhTable()
        {
            var source =
                @"FLBR id 'AL1_1031' sc 0 dc lw 2 9.9999e+009 9.9999e+009 'Lateral Discharge' PDIN 0 0 '' pdin CLTT 'H [m]' 'Q' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"49.86 0 < " + Environment.NewLine +
                @"49.96 -10 < " + Environment.NewLine +
                @"52.44 -952 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" lt 0 sd '-1' si '-1' wl ow 0 9.9999e+009 9.9999e+009 hs 0 as SLST slst flbr";
            var reader = new SobekLateralFlowReader();

            var lateralFlow = reader.GetLateralFlow(source);

            var waterFlowModel1DLateralSourceData = new Model1DLateralSourceData();

            SobekLateralSourcesDataImporter.ConvertToLateralSourceData(lateralFlow, waterFlowModel1DLateralSourceData);

            Assert.AreEqual(Model1DLateralDataType.FlowWaterLevelTable,
                            waterFlowModel1DLateralSourceData.DataType);
            Assert.AreEqual(0.0, (double)waterFlowModel1DLateralSourceData.Data[49.86], 1.0e-6);
            Assert.AreEqual(-10.0, (double)waterFlowModel1DLateralSourceData.Data[49.96], 1.0e-6);
            Assert.AreEqual(-952.0, (double)waterFlowModel1DLateralSourceData.Data[52.44], 1.0e-6);
            Assert.AreEqual(ExtrapolationType.Constant, waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType);
        }
    }
}