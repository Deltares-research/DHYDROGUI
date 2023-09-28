using System;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class StructureFileWriterTest
    {
        private IHydroNetwork network;

        [SetUp]
        public void SetUp()
        {
            network = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
        }

        [TearDown]
        public void TearDown()
        {
        }
        
        [Test]
        public void TestStructuresFileWriterGivesExpectedResults_MultipleTypes()
        {
            var relativePathStructuresExpectedFile = TestHelper.GetTestFilePath(@"FileWriters/Structures_expected.txt");

            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddPump(
                StructureFileWriterTestHelper.PUMP_ID,
                StructureFileWriterTestHelper.PUMP_NAME,
                StructureFileWriterTestHelper.PUMP_CAPACITY,
                StructureFileWriterTestHelper.PUMP_CHAINAGE,
                StructureFileWriterTestHelper.PUMP_CONTROL_DIRECTION,
                StructureFileWriterTestHelper.PUMP_SUCTION_START,
                StructureFileWriterTestHelper.PUMP_SUCTION_STOP,
                StructureFileWriterTestHelper.PUMP_DELIVERY_START,
                StructureFileWriterTestHelper.PUMP_DELIVERY_STOP,
                StructureFileWriterTestHelper.PUMP_HEAD_VALUES,
                StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES);

            branch.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.WEIR_CHAINAGE,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF);

            branch.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                StructureFileWriterTestHelper.UNI_WEIR_CHAINAGE,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.UNI_WEIR_Y_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);

            branch.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                StructureFileWriterTestHelper.RIVER_WEIR_CHAINAGE,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            branch.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                StructureFileWriterTestHelper.ADV_WEIR_CHAINAGE,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            branch.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                StructureFileWriterTestHelper.ORIFICE_CHAINAGE,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_NEG,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            branch.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CHAINAGE,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LOWER_EDGE_LEVEL,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_USE_VELOCITY_HEIGHT);

            branch.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                StructureFileWriterTestHelper.CULVERT_CHAINAGE,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                StructureFileWriterTestHelper.CULVERT_LENGTH,
                StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);

            branch.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                StructureFileWriterTestHelper.INV_SIPHON_CHAINAGE,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);
            
            branch.AddBridgeStandard(
                StructureFileWriterTestHelper.BRIDGE_ID,
                StructureFileWriterTestHelper.BRIDGE_NAME,
                StructureFileWriterTestHelper.BRIDGE_CHAINAGE,
                StructureFileWriterTestHelper.BRIDGE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.BRIDGE_BED_LEVEL,
                StructureFileWriterTestHelper.BRIDGE_CSDEF_ID,
                StructureFileWriterTestHelper.BRIDGE_LENGTH,
                StructureFileWriterTestHelper.BRIDGE_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.BRIDGE_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.BRIDGE_FRICTION,
                StructureFileWriterTestHelper.BRIDGE_GROUNDFRICTION,
                StructureFileWriterTestHelper.BRIDGE_ENABLE_GROUNDLAYER);

/*
 bridge pillar not yet implemented in the kernel
            branch.AddBridgePillar(
                StructureFileWriterTestHelper.BRIDGE_PILLAR_ID,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_NAME,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_CHAINAGE,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_BED_LEVEL,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_CSDEF_ID,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_WIDTH,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_FORM_FACTOR);
*/

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            string errorMessage;
            var relativePathActualFile = Path.Combine(FileWriterTestHelper.RelativeTargetDirectory, FileWriterTestHelper.ModelFileNames.Structures);
            Assert.IsTrue(FileComparer.Compare(relativePathStructuresExpectedFile, relativePathActualFile, out errorMessage, true),
                          string.Format("Generated Structures file does not match template!{0}{1}", Environment.NewLine, errorMessage));
        }
        
        [Test]
        public void TestStructureFileWriterGivesExpectedResults_Pump()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddPump(
                StructureFileWriterTestHelper.PUMP_ID, 
                StructureFileWriterTestHelper.PUMP_NAME, 
                StructureFileWriterTestHelper.PUMP_CAPACITY, 
                StructureFileWriterTestHelper.PUMP_CHAINAGE, 
                StructureFileWriterTestHelper.PUMP_CONTROL_DIRECTION,
                StructureFileWriterTestHelper.PUMP_SUCTION_START,
                StructureFileWriterTestHelper.PUMP_SUCTION_STOP, 
                StructureFileWriterTestHelper.PUMP_DELIVERY_START, 
                StructureFileWriterTestHelper.PUMP_DELIVERY_STOP, 
                StructureFileWriterTestHelper.PUMP_HEAD_VALUES, 
                StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES);

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(16, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.Pump, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Direction.Key);
            Assert.AreEqual("deliverySide", idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NrStages.Key);
            Assert.AreEqual("1", idProperty.Value); // default value in DefinitionGenerator

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Capacity.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_CAPACITY.ToString(StructureRegion.Capacity.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.StartLevelSuctionSide.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_SUCTION_START.ToString(StructureRegion.StartLevelSuctionSide.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.StopLevelSuctionSide.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_SUCTION_STOP.ToString(StructureRegion.StopLevelSuctionSide.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.StartLevelDeliverySide.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_DELIVERY_START.ToString(StructureRegion.StartLevelDeliverySide.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.StopLevelDeliverySide.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.PUMP_DELIVERY_STOP.ToString(StructureRegion.StopLevelDeliverySide.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Head.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.PUMP_HEAD_VALUES.Select(v => v.ToString(StructureRegion.Head.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.ReductionFactor.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES.Select(v => v.ToString(StructureRegion.ReductionFactor.Format, CultureInfo.InvariantCulture))), idProperty.Value);
        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_Weir()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.WEIR_CHAINAGE,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF);

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(10, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.WEIR_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.WEIR_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.WEIR_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.Weir, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.WEIR_CREST_LEVEL.ToString(StructureRegion.CrestLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestWidth.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.WEIR_CREST_WIDTH.ToString(StructureRegion.CrestWidth.Format, CultureInfo.InvariantCulture), idProperty.Value);
            
            idProperty = content.Properties.First(p => p.Key == StructureRegion.AllowedFlowDir.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION.GetDescription().ToLower(), idProperty.Value);

        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_UniversalWeir()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddUniversalWeir(
                StructureFileWriterTestHelper.UNI_WEIR_ID,
                StructureFileWriterTestHelper.UNI_WEIR_NAME,
                StructureFileWriterTestHelper.UNI_WEIR_CHAINAGE,
                StructureFileWriterTestHelper.UNI_WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.UNI_WEIR_Y_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_Z_VALUES.ToArray(),
                StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF);
            
            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(11, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.UNI_WEIR_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.UNI_WEIR_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.UNI_WEIR_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.UniversalWeir, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.AllowedFlowDir.Key);
            Assert.AreEqual("both", idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.LevelsCount.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.UNI_WEIR_Y_VALUES.Count.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.YValues.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.UNI_WEIR_Y_VALUES.Select(v => v.ToString(StructureRegion.YValues.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.ZValues.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.UNI_WEIR_Z_VALUES.Select(v => v.ToString(StructureRegion.ZValues.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestLevel.Key);
            Assert.AreEqual(0d.ToString(StructureRegion.CrestLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DischargeCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.UNI_WEIR_DISCHARGE_COEFF.ToString(StructureRegion.DischargeCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);
        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_RiverWeir()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddRiverWeir(
                StructureFileWriterTestHelper.RIVER_WEIR_ID,
                StructureFileWriterTestHelper.RIVER_WEIR_NAME,
                StructureFileWriterTestHelper.RIVER_WEIR_CHAINAGE,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF,
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT,
                StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.ToArray(),
                StructureFileWriterTestHelper.RIVER_WEIR_NEG_RED.ToArray());

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(17, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.RiverWeir, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_CREST_LEVEL.ToString(StructureRegion.CrestLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestWidth.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_CREST_WIDTH.ToString(StructureRegion.CrestWidth.Format, CultureInfo.InvariantCulture), idProperty.Value);
            
            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosCwCoef.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_POS_CW_COEFF.ToString(StructureRegion.PosCwCoef.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosSlimLimit.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_POS_SLIM_LIMIT.ToString(StructureRegion.PosSlimLimit.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegCwCoef.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_NEG_CW_COEFF.ToString(StructureRegion.NegCwCoef.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegSlimLimit.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_NEG_SLIM_LIMIT.ToString(StructureRegion.NegSlimLimit.Format, CultureInfo.InvariantCulture), idProperty.Value);
            
            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosSfCount.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.Count.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosSf.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.Select(v => v.ToString(StructureRegion.PosSf.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosRed.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.Select(v => v.ToString(StructureRegion.PosRed.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegSfCount.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.RIVER_WEIR_NEG_SF.Count.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegSf.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.RIVER_WEIR_POS_SF.Select(v => v.ToString(StructureRegion.NegSf.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegRed.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.RIVER_WEIR_POS_RED.Select(v => v.ToString(StructureRegion.NegRed.Format, CultureInfo.InvariantCulture))), idProperty.Value);
        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_AdvancedWeir()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddAdvancedWeir(
                StructureFileWriterTestHelper.ADV_WEIR_ID,
                StructureFileWriterTestHelper.ADV_WEIR_NAME,
                StructureFileWriterTestHelper.ADV_WEIR_CHAINAGE,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS,
                StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG,
                StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG);

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(16, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.AdvancedWeir, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_CREST_LEVEL.ToString(StructureRegion.CrestLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestWidth.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_CREST_WIDTH.ToString(StructureRegion.CrestWidth.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NPiers.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_NUM_PIERS.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosHeight.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_POS.ToString(StructureRegion.PosHeight.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosDesignHead.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_POS.ToString(StructureRegion.PosDesignHead.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosPierContractCoef.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_POS.ToString(StructureRegion.PosPierContractCoef.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosAbutContractCoef.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_POS.ToString(StructureRegion.PosAbutContractCoef.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegHeight.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_UPSTREAM_FACE_NEG.ToString(StructureRegion.NegHeight.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegDesignHead.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_DESIGN_HEAD_NEG.ToString(StructureRegion.NegDesignHead.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegPierContractCoef.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_PIER_CONTRACTION_NEG.ToString(StructureRegion.NegPierContractCoef.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegAbutContractCoef.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ADV_WEIR_ABUT_CONTRACTION_NEG.ToString(StructureRegion.NegAbutContractCoef.Format, CultureInfo.InvariantCulture), idProperty.Value);
        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_Orifice()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                StructureFileWriterTestHelper.ORIFICE_CHAINAGE,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_NEG,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(13, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.Orifice, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL.ToString(StructureRegion.CrestLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestWidth.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH.ToString(StructureRegion.CrestWidth.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.AllowedFlowDir.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION.GetDescription().ToLower(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.UseLimitFlowNeg.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_NEG.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.LimitFlowNeg.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG.ToString(StructureRegion.LimitFlowNeg.Format, CultureInfo.InvariantCulture), idProperty.Value);
        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_GeneralStructure()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CHAINAGE,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LOWER_EDGE_LEVEL,
                !StructureFileWriterTestHelper.GENERAL_STRUCTURE_USE_VELOCITY_HEIGHT
                );

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(32, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.GeneralStructure, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Upstream1Width.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1.ToString(StructureRegion.Upstream1Width.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Upstream2Width.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL.ToString(StructureRegion.Upstream2Width.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestWidth.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER.ToString(StructureRegion.CrestWidth.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Downstream1Width.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR.ToString(StructureRegion.Downstream1Width.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Downstream2Width.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2.ToString(StructureRegion.Downstream2Width.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Upstream1Level.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1.ToString(StructureRegion.Upstream1Level.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Upstream2Level.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL.ToString(StructureRegion.Upstream2Level.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CrestLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER.ToString(StructureRegion.CrestLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Downstream1Level.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR.ToString(StructureRegion.Downstream1Level.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Downstream2Level.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2.ToString(StructureRegion.Downstream2Level.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.GateLowerEdgeLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_LOWER_EDGE_LEVEL.ToString(StructureRegion.GateLowerEdgeLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosFreeGateFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS.ToString(StructureRegion.PosFreeGateFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosDrownGateFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS.ToString(StructureRegion.PosDrownGateFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosFreeWeirFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS.ToString(StructureRegion.PosFreeWeirFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosDrownWeirFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS.ToString(StructureRegion.PosDrownWeirFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PosContrCoefFreeGate.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS.ToString(StructureRegion.PosContrCoefFreeGate.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegFreeGateFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG.ToString(StructureRegion.NegFreeGateFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegDrownGateFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG.ToString(StructureRegion.NegDrownGateFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegFreeWeirFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG.ToString(StructureRegion.NegFreeWeirFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegDrownWeirFlowCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG.ToString(StructureRegion.NegDrownWeirFlowCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.NegContrCoefFreeGate.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG.ToString(StructureRegion.NegContrCoefFreeGate.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.ExtraResistance.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE.ToString(StructureRegion.ExtraResistance.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.UseVelocityHeight.Key);
            Assert.AreEqual((!StructureFileWriterTestHelper.GENERAL_STRUCTURE_USE_VELOCITY_HEIGHT).ToString().ToLower(), idProperty.Value);
        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_GeneralStructureWithExtraResistanceDisabled()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddGeneralStructure(
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_ID,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_NAME,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CHAINAGE,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_GATE_OPENING,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_EXTRA_RESISTANCE,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_W1,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_LEFT_WSDL,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_CENTER,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_WIDTH_RIGHT_W2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZB1,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_CENTER,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG,
                StructureFileWriterTestHelper.GENERAL_STRUCTURE_LOWER_EDGE_LEVEL,
                false);

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(32, content.Properties.Count());
            
            var idProperty = content.Properties.First(p => p.Key == StructureRegion.ExtraResistance.Key);
            const double expectedExtraResistance = 0.0;
            Assert.AreEqual(expectedExtraResistance.ToString(StructureRegion.ExtraResistance.Format, CultureInfo.InvariantCulture), idProperty.Value);
        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_Culvert()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddCulvert(
                StructureFileWriterTestHelper.CULVERT_ID,
                StructureFileWriterTestHelper.CULVERT_NAME,
                StructureFileWriterTestHelper.CULVERT_CHAINAGE,
                StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION,
                StructureFileWriterTestHelper.CULVERT_INLET_LEVEL,
                StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL,
                StructureFileWriterTestHelper.CULVERT_LENGTH,
                StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.CULVERT_IS_GATED,
                StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.CULVERT_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.CULVERT_FRICTION,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS);
            
            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(21, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.Culvert, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.AllowedFlowDir.Key);
            Assert.That((StructureFileWriterTestHelper.CULVERT_FLOW_DIRECTION).ToString(), Is.EqualTo(idProperty.Value).IgnoreCase);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.LeftLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_INLET_LEVEL.ToString(StructureRegion.LeftLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.RightLevel.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_OUTLET_LEVEL.ToString(StructureRegion.RightLevel.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CsDefId.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_ID.ToString(), idProperty.Value);
            
            idProperty = content.Properties.First(p => p.Key == StructureRegion.Length.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_LENGTH.ToString(StructureRegion.Length.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.InletLossCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_INLET_LOSS_COEFF.ToString(StructureRegion.InletLossCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.OutletLossCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_OUTLET_LOSS_COEFF.ToString(StructureRegion.OutletLossCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.ValveOnOff.Key);
            Assert.AreEqual(Convert.ToInt32(StructureFileWriterTestHelper.CULVERT_IS_GATED).ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.IniValveOpen.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_GATE_INITIAL_OPENING.ToString(StructureRegion.IniValveOpen.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.LossCoeffCount.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.Count.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.RelativeOpening.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.CULVERT_REL_OPENING.Select(v => v.ToString(StructureRegion.RelativeOpening.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.LossCoefficient.Key);
            Assert.AreEqual(string.Join(" ", StructureFileWriterTestHelper.CULVERT_LOSS_COEFF.Select(v => v.ToString(StructureRegion.LossCoefficient.Format, CultureInfo.InvariantCulture))), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BedFrictionType.Key);
            Assert.AreEqual(((int)Friction.Chezy).ToString(), idProperty.Value); // Determined in Mock Class

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BedFriction.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_FRICTION.ToString(StructureRegion.BedFriction.Format, CultureInfo.InvariantCulture), idProperty.Value);
            
            idProperty = content.Properties.First(p => p.Key == StructureRegion.GroundFrictionType.Key);
            Assert.AreEqual(((int)Friction.Chezy).ToString(), idProperty.Value); // Determined in Mock Class

            idProperty = content.Properties.First(p => p.Key == StructureRegion.GroundFriction.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.CULVERT_GROUNDLAYER_ROUGHNESS.ToString(StructureRegion.GroundFriction.Format, CultureInfo.InvariantCulture), idProperty.Value);
        }
        
        [Test]
        public void TestStructureFileWriterGivesExpectedResults_InvertedSiphon()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddInvertedSiphon(
                StructureFileWriterTestHelper.INV_SIPHON_ID,
                StructureFileWriterTestHelper.INV_SIPHON_NAME,
                StructureFileWriterTestHelper.INV_SIPHON_CHAINAGE,
                StructureFileWriterTestHelper.INV_SIPHON_FLOW_DIRECTION,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LEVEL,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LEVEL,
                StructureFileWriterTestHelper.INV_SIPHON_LENGTH,
                StructureFileWriterTestHelper.INV_SIPHON_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.INV_SIPHON_IS_GATED,
                StructureFileWriterTestHelper.INV_SIPHON_GATE_INITIAL_OPENING,
                StructureFileWriterTestHelper.INV_SIPHON_REL_OPENING.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_LOSS_COEFF.ToArray(),
                StructureFileWriterTestHelper.INV_SIPHON_FRICTION,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ENABLED,
                StructureFileWriterTestHelper.INV_SIPHON_GROUNDLAYER_ROUGHNESS,
                StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF);
            
            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(23, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.INV_SIPHON_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.INV_SIPHON_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.INV_SIPHON_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.Culvert, idProperty.Value);

            // Many common properties tested by Culvert test (above)

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BendLossCoef.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.INV_SIPHON_BEND_LOSS_COEFF.ToString(StructureRegion.BendLossCoef.Format, CultureInfo.InvariantCulture), idProperty.Value);

        }

        [Test]
        public void TestStructureFileWriterGivesExpectedResults_Bridge()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddBridgeStandard(
                StructureFileWriterTestHelper.BRIDGE_ID,
                StructureFileWriterTestHelper.BRIDGE_NAME,
                StructureFileWriterTestHelper.BRIDGE_CHAINAGE,
                StructureFileWriterTestHelper.BRIDGE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.BRIDGE_BED_LEVEL,
                StructureFileWriterTestHelper.BRIDGE_CSDEF_ID,
                StructureFileWriterTestHelper.BRIDGE_LENGTH,
                StructureFileWriterTestHelper.BRIDGE_INLET_LOSS_COEFF,
                StructureFileWriterTestHelper.BRIDGE_OUTLET_LOSS_COEFF,
                StructureFileWriterTestHelper.BRIDGE_FRICTION,
                StructureFileWriterTestHelper.BRIDGE_GROUNDFRICTION,
                StructureFileWriterTestHelper.BRIDGE_ENABLE_GROUNDLAYER);

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(15, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.Bridge, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.AllowedFlowDir.Key);
            Assert.That((StructureFileWriterTestHelper.BRIDGE_FLOW_DIRECTION).ToString(), Is.EqualTo(idProperty.Value).IgnoreCase);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Shift.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_BED_LEVEL.ToString(StructureRegion.Shift.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.CsDefId.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Length.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_LENGTH.ToString(StructureRegion.Length.Format, CultureInfo.InvariantCulture), idProperty.Value);
            
            idProperty = content.Properties.First(p => p.Key == StructureRegion.InletLossCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_INLET_LOSS_COEFF.ToString(StructureRegion.InletLossCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.OutletLossCoeff.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_OUTLET_LOSS_COEFF.ToString(StructureRegion.OutletLossCoeff.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BedFrictionType.Key);
            Assert.AreEqual(((int)Friction.Chezy).ToString(), idProperty.Value); // Determined in Mock Class

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BedFriction.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_FRICTION.ToString(StructureRegion.BedFriction.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.GroundFrictionType.Key);
            Assert.AreEqual(((int)Friction.Chezy).ToString(), idProperty.Value); // Determined in Mock Class

            idProperty = content.Properties.First(p => p.Key == StructureRegion.GroundFriction.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_GROUNDFRICTION.ToString(StructureRegion.GroundFriction.Format, CultureInfo.InvariantCulture), idProperty.Value);
        }

        [Test]
        [Ignore("Not yet implemented in the kernel")]
        public void TestStructureFileWriterGivesExpectedResults_BridgePillar()
        {
            var branch = network.Branches.First();
            Assert.NotNull(branch, "No branched added to the network");

            branch.AddBridgePillar(
                StructureFileWriterTestHelper.BRIDGE_PILLAR_ID,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_NAME,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_CHAINAGE,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_BED_LEVEL,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_CSDEF_ID,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_WIDTH,
                StructureFileWriterTestHelper.BRIDGE_PILLAR_FORM_FACTOR);

            StructureFileWriterTestHelper.WriteCrossSectionsToIni(network.Structures);

            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Structures);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == StructureRegion.Header));

            var content = iniSections.Where(c => c.Name == StructureRegion.Header).ToList().First();
            Assert.AreEqual(9, content.Properties.Count());

            var idProperty = content.Properties.First(p => p.Key == StructureRegion.Id.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_PILLAR_ID.ToString(), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Name.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_PILLAR_NAME, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Chainage.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_PILLAR_CHAINAGE.ToString(StructureRegion.Chainage.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.DefinitionType.Key);
            Assert.AreEqual(StructureRegion.StructureTypeName.BridgePillar, idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.AllowedFlowDir.Key);
            Assert.That((StructureFileWriterTestHelper.BRIDGE_PILLAR_FLOW_DIRECTION).ToString(), Is.EqualTo(idProperty.Value).IgnoreCase);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.Shift.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_PILLAR_BED_LEVEL.ToString(StructureRegion.Shift.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.PillarWidth.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_PILLAR_WIDTH.ToString(StructureRegion.PillarWidth.Format, CultureInfo.InvariantCulture), idProperty.Value);

            idProperty = content.Properties.First(p => p.Key == StructureRegion.FormFactor.Key);
            Assert.AreEqual(StructureFileWriterTestHelper.BRIDGE_PILLAR_FORM_FACTOR.ToString(StructureRegion.FormFactor.Format, CultureInfo.InvariantCulture), idProperty.Value);
        }
    }
}
