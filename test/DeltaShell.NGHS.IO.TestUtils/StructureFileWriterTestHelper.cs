using System.Collections.Generic;
using System.IO;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public static class StructureFileWriterTestHelper
    {
        #region Const TestValues

        public const int PUMP_ID = 1;
        public const string PUMP_NAME = "Pump_1D_1";
        public const double PUMP_CAPACITY = 3.0;
        public const double PUMP_CHAINAGE = 0.5;
        public const PumpControlDirection PUMP_CONTROL_DIRECTION = PumpControlDirection.DeliverySideControl;
        public const double PUMP_SUCTION_START = 4.0;
        public const double PUMP_SUCTION_STOP = 5.0;
        public const double PUMP_DELIVERY_START = 2.0;
        public const double PUMP_DELIVERY_STOP = 3.0;
        public static readonly List<double> PUMP_HEAD_VALUES = new List<double> { 0.0, 0.1, 0.2 };
        public static readonly List<double> PUMP_REDUCTION_VALUES = new List<double> { 0.0, 0.1, 0.2 };

        public const int WEIR_ID = 2;
        public const string WEIR_NAME = "Weir_1D_1";
        public const double WEIR_CREST_LEVEL = 3.0;
        public const double WEIR_CREST_WIDTH = 5.0;
        public const double WEIR_CHAINAGE = 1.0;
        public const FlowDirection WEIR_FLOW_DIRECTION = FlowDirection.Both;
        public const double WEIR_DISCHARGE_COEFF = 2.0;

        public const int UNI_WEIR_ID = 3;
        public const string UNI_WEIR_NAME = "Weir_1D_2";
        public const double UNI_WEIR_CHAINAGE = 1.5;
        public const FlowDirection UNI_WEIR_FLOW_DIRECTION = FlowDirection.Both;
        public static readonly List<double> UNI_WEIR_Y_VALUES = new List<double> { 5.0, -2.0, -2.0, 5.0 };
        public static readonly List<double> UNI_WEIR_Z_VALUES = new List<double> { 10.0, 2.5, 2.5, 10.0 };
        public const double UNI_WEIR_DISCHARGE_COEFF = 0.5;


        public const int RIVER_WEIR_ID = 4;
        public const string RIVER_WEIR_NAME = "Weir_1D_3";
        public const double RIVER_WEIR_CHAINAGE = 2.0;
        public const double RIVER_WEIR_CREST_LEVEL = 2.0;
        public const double RIVER_WEIR_CREST_WIDTH = 5.0;
        public const double RIVER_WEIR_POS_CW_COEFF = 1.4;
        public const double RIVER_WEIR_POS_SLIM_LIMIT = 0.05;
        public const double RIVER_WEIR_NEG_CW_COEFF = 1.4;
        public const double RIVER_WEIR_NEG_SLIM_LIMIT = 0.05;
        public static readonly List<double> RIVER_WEIR_POS_SF = new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 };
        public static readonly List<double> RIVER_WEIR_POS_RED = new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 };
        public static readonly List<double> RIVER_WEIR_NEG_SF = new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 };
        public static readonly List<double> RIVER_WEIR_NEG_RED = new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 };

        public const int ADV_WEIR_ID = 5;
        public const string ADV_WEIR_NAME = "Weir_1D_4";
        public const double ADV_WEIR_CHAINAGE = 2.5;
        public const double ADV_WEIR_CREST_LEVEL = 3.0;
        public const double ADV_WEIR_CREST_WIDTH = 6.0;
        public const int ADV_WEIR_NUM_PIERS = 1;
        public const double ADV_WEIR_UPSTREAM_FACE_POS = 9.0;
        public const double ADV_WEIR_DESIGN_HEAD_POS = 2.5;
        public const double ADV_WEIR_PIER_CONTRACTION_POS = 0.05;
        public const double ADV_WEIR_ABUT_CONTRACTION_POS = 0.2;
        public const double ADV_WEIR_UPSTREAM_FACE_NEG = 9.0;
        public const double ADV_WEIR_DESIGN_HEAD_NEG = 2.5;
        public const double ADV_WEIR_PIER_CONTRACTION_NEG = 0.05;
        public const double ADV_WEIR_ABUT_CONTRACTION_NEG = 0.2;

        public const int ORIFICE_ID = 6;
        public const string ORIFICE_NAME = "Weir_1D_5";
        public const double ORIFICE_CHAINAGE = 3.0;
        public const FlowDirection ORIFICE_FLOW_DIRECTION = FlowDirection.Both;
        public const double ORIFICE_CREST_LEVEL = 3.0;
        public const double ORIFICE_CREST_WIDTH = 5.0;
        public const double ORIFICE_GATE_OPENING = 2.5;
        public const double ORIFICE_CONTRACTION_COEFF = 0.5;
        public const double ORIFICE_LAT_CONTRACTION_COEFF = 1.0;
        public const bool ORIFICE_USE_LIMIT_FLOW_POS = false;
        public const double ORIFICE_LIMIT_FLOW_POS = 0.0;
        public const bool ORIFICE_USE_LIMIT_FLOW_NEG = true;
        public const double ORIFICE_LIMIT_FLOW_NEG = 0.15;

        public const int GENERAL_STRUCTURE_ID = 7;
        public const string GENERAL_STRUCTURE_NAME = "Weir_1D_6";
        public const double GENERAL_STRUCTURE_CHAINAGE = 3.5;
        public const double GENERAL_STRUCTURE_GATE_OPENING = 1.5;
        public const double GENERAL_STRUCTURE_EXTRA_RESISTANCE = 0.0;
        public const double GENERAL_STRUCTURE_WIDTH_LEFT_W1 = 1.0;
        public const double GENERAL_STRUCTURE_WIDTH_LEFT_WSDL = 0.5;
        public const double GENERAL_STRUCTURE_WIDTH_CENTER = 1.0;
        public const double GENERAL_STRUCTURE_WIDTH_RIGHT_WSDR = 0.5;
        public const double GENERAL_STRUCTURE_WIDTH_RIGHT_W2 = 1.0;
        public const double GENERAL_STRUCTURE_LEVEL_LEFT_ZB1 = 1.1;
        public const double GENERAL_STRUCTURE_LEVEL_LEFT_ZBSL = 0.6;
        public const double GENERAL_STRUCTURE_LEVEL_CENTER = 1.1;
        public const double GENERAL_STRUCTURE_LEVEL_RIGHT_ZBSR = 0.6;
        public const double GENERAL_STRUCTURE_LEVEL_RIGHT_ZB2 = 1.1;
        public const double GENERAL_STRUCTURE_LOWER_EDGE_LEVEL = 11.0;
        public const double GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_POS = 1.2;
        public const double GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_POS = 0.7;
        public const double GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_POS = 1.2;
        public const double GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_POS = 0.7;
        public const double GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_POS = 1.2;
        public const double GENERAL_STRUCTURE_FREE_GATE_FLOW_COEFF_NEG = 1.3;
        public const double GENERAL_STRUCTURE_DROWN_GATE_FLOW_COEFF_NEG = 0.8;
        public const double GENERAL_STRUCTURE_FREE_WEIR_FLOW_COEFF_NEG = 1.3;
        public const double GENERAL_STRUCTURE_DROWN_WEIR_FLOW_COEFF_NEG = 0.8;
        public const double GENERAL_STRUCTURE_CONTROL_COEFF_FREE_GATE_NEG = 1.3;
        public const bool   GENERAL_STRUCTURE_USE_VELOCITY_HEIGHT = true;

        public const int CULVERT_ID = 8;
        public const string CULVERT_NAME = "Culvert_1D_1";
        public const double CULVERT_CHAINAGE = 0.4;
        public const FlowDirection CULVERT_FLOW_DIRECTION = FlowDirection.Both;
        public const double CULVERT_INLET_LEVEL = 0.5;
        public const double CULVERT_OUTLET_LEVEL = 1.5;
        public const int CULVERT_CSDEF_ID = 1;
        public const double CULVERT_LENGTH = 10.0;
        public const double CULVERT_INLET_LOSS_COEFF = 0.2;
        public const double CULVERT_OUTLET_LOSS_COEFF = 0.3;
        public const bool CULVERT_IS_GATED = false;
        public const double CULVERT_GATE_INITIAL_OPENING = 0.0;
        public static readonly List<double> CULVERT_REL_OPENING = new List<double> { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
        public static readonly List<double> CULVERT_LOSS_COEFF = new List<double> { 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2.0 };
        public const double CULVERT_FRICTION = 0.25;
        public const bool CULVERT_GROUNDLAYER_ENABLED = true;
        public const double CULVERT_GROUNDLAYER_ROUGHNESS = 0.0;

        public const int INV_SIPHON_ID = 9;
        public const string INV_SIPHON_NAME = "Culvert_1D_2";
        public const double INV_SIPHON_CHAINAGE = 4.5;
        public const FlowDirection INV_SIPHON_FLOW_DIRECTION = FlowDirection.Both;
        public const double INV_SIPHON_INLET_LEVEL = 0.4;
        public const double INV_SIPHON_OUTLET_LEVEL = 1.3;
        public const int INV_SIPHON_CSDEF_ID = 1;
        public const double INV_SIPHON_LENGTH = 8.0;
        public const double INV_SIPHON_INLET_LOSS_COEFF = 0.15;
        public const double INV_SIPHON_OUTLET_LOSS_COEFF = 0.25;
        public const bool INV_SIPHON_IS_GATED = false;
        public const double INV_SIPHON_GATE_INITIAL_OPENING = 0.0;
        public static readonly List<double> INV_SIPHON_REL_OPENING = new List<double> { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
        public static readonly List<double> INV_SIPHON_LOSS_COEFF = new List<double> { 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2.0 };
        public const double INV_SIPHON_FRICTION = 0.35;
        public const bool INV_SIPHON_GROUNDLAYER_ENABLED = false;
        public const double INV_SIPHON_GROUNDLAYER_ROUGHNESS = 0.0;
        public const double INV_SIPHON_BEND_LOSS_COEFF = 0.6;
        
        public const int BRIDGE_ID = 11;
        public const string BRIDGE_NAME = "Bridge_1D_1";
        public const double BRIDGE_CHAINAGE = 5.5;
        public const FlowDirection BRIDGE_FLOW_DIRECTION = FlowDirection.Both;
        public const double BRIDGE_BED_LEVEL = 0.3;
        public const int BRIDGE_CSDEF_ID = 2;
        public const double BRIDGE_LENGTH = 1.5;
        public const double BRIDGE_INLET_LOSS_COEFF = 0.4;
        public const double BRIDGE_OUTLET_LOSS_COEFF = 0.8;
        public const double BRIDGE_FRICTION = 0.65;
        public const double BRIDGE_GROUNDFRICTION = 0.35;
        public const bool BRIDGE_ENABLE_GROUNDLAYER = true;

        public const int BRIDGE_PILLAR_ID = 12;
        public const string BRIDGE_PILLAR_NAME = "BridgePillar_1D_1";
        public const double BRIDGE_PILLAR_CHAINAGE = 6.0;
        public const FlowDirection BRIDGE_PILLAR_FLOW_DIRECTION = FlowDirection.Both;
        public const double BRIDGE_PILLAR_BED_LEVEL = 0.3;
        public const int BRIDGE_PILLAR_CSDEF_ID = 2;
        public const double BRIDGE_PILLAR_WIDTH = 0.75;
        public const double BRIDGE_PILLAR_FORM_FACTOR = 0.5;

        public const int EXTRA_RESISTANCE_ID = 13;
        public const string EXTRA_RESISTANCE_NAME = "ExtraResistance1";
        public const double EXTRA_RESISTANCE_CHAINAGE = 6.0;
        public static readonly List<double> EXTRA_RESISTANCE_LEVELS = new List<double> { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
        public static readonly List<double> EXTRA_RESISTANCE_KSI = new List<double> { 0.13, 0.23, 0.33, 0.43, 0.53, 0.63, 0.73, 0.83, 0.93 };

        #endregion Const Test Values

        #region Structure Creation Helpers

        public static void AddPump(this IBranch branch, long id, string name, double capacity, double chainage,
                                    PumpControlDirection controlDirection, double startSuction, double stopSuction,
                                    double startDelivery, double stopDelivery, List<double> headValues, List<double> reductionFactorValues)
        {
            var pump = new Pump(name)
            {
                Branch = branch,
                Name = id.ToString(),
                LongName = name,
                Capacity = capacity,
                Chainage = chainage,
                ControlDirection = controlDirection,
                StartSuction = startSuction,
                StopSuction = stopSuction,
                StartDelivery = startDelivery,
                StopDelivery = stopDelivery
            };

            var argument = new Variable<double>();
            argument.Values.AddRange(headValues);
            pump.ReductionTable.Arguments.Clear();
            pump.ReductionTable.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(reductionFactorValues);
            pump.ReductionTable.Components.Clear();
            pump.ReductionTable.Components.Add(component);

            branch.AddStructure(pump);
        }

        private static IWeir AddWeir1D(this IBranch branch, long id, string name, double chainage)
        {
            var weir = new Weir
            {
                Branch = branch,
                Name = id.ToString(),
                LongName = name,
                Chainage = chainage
            };

            branch.AddStructure(weir);
            return weir;
        }

        public static void AddSimpleWeir(this IBranch branch, long id, string name, double crestLevel,
            double crestWidth, double chainage,
            FlowDirection flowDirection, double corrCoefficient)
        {
            var weir = AddWeir1D(branch, id, name, chainage);
            weir.CrestLevel = crestLevel;
            weir.CrestWidth = crestWidth;
            weir.FlowDirection = flowDirection;

            weir.WeirFormula = new SimpleWeirFormula
            {
                CorrectionCoefficient = corrCoefficient
            };
        }

        public static void AddUniversalWeir(this IBranch branch, long id, string name, double chainage, FlowDirection flowDirection,
                                             double[] yValues, double[] zValues, double dischargeCoefficient)
        {
            var weir = AddWeir1D(branch, id, name, chainage);
            weir.FlowDirection = flowDirection;

            Coordinate[] coordinates = new Coordinate[yValues.Length];
            for (var i = 0; i < yValues.Length; i++)
            {
                coordinates[i] = new Coordinate(yValues[i], zValues[i]);
            }

            weir.WeirFormula = new FreeFormWeirFormula
            {
                Shape = new LineString(coordinates), // CrestLevel == lowest zValue
                DischargeCoefficient = dischargeCoefficient
            };
        }

        public static void AddRiverWeir(this IBranch branch, long id, string name, double chainage, double crestLevel, double crestWidth,
                                         double posCwCoef, double posSlimLimit, double negCwCoef, double negSlimLimit,
                                         double[] posSf, double[] posRed, double[] negSf, double[] negRed)
        {
            var weir = AddWeir1D(branch, id, name, chainage);
            weir.CrestLevel = crestLevel;
            weir.CrestWidth = crestWidth;

            var formula = new RiverWeirFormula
            {
                CorrectionCoefficientPos = posCwCoef,
                SubmergeLimitPos = posSlimLimit,
                CorrectionCoefficientNeg = negCwCoef,
                SubmergeLimitNeg = negSlimLimit
            };

            var argumentPos = new Variable<double>();
            argumentPos.Values.AddRange(posSf);
            formula.SubmergeReductionPos.Arguments.Clear();
            formula.SubmergeReductionPos.Arguments.Add(argumentPos);

            var componentPos = new Variable<double>();
            componentPos.Values.AddRange(posRed);
            formula.SubmergeReductionPos.Components.Clear();
            formula.SubmergeReductionPos.Components.Add(componentPos);

            var argumentNeg = new Variable<double>();
            argumentNeg.Values.AddRange(negSf);
            formula.SubmergeReductionNeg.Arguments.Clear();
            formula.SubmergeReductionNeg.Arguments.Add(argumentNeg);

            var componentNeg = new Variable<double>();
            componentNeg.Values.AddRange(negRed);
            formula.SubmergeReductionNeg.Components.Clear();
            formula.SubmergeReductionNeg.Components.Add(componentNeg);

            weir.WeirFormula = formula;
        }

        public static void AddAdvancedWeir(this IBranch branch, long id, string name, double chainage, double crestLevel, double crestWidth, int numPiers,
                                            double upstreamFacePos, double designHeadPos, double pierContractionPos, double abutContractionPos,
                                            double upstreamFaceNeg, double designHeadNeg, double pierContractionNeg, double abutContractionNeg)
        {
            var weir = AddWeir1D(branch, id, name, chainage);
            weir.CrestLevel = crestLevel;
            weir.CrestWidth = crestWidth;

            weir.WeirFormula = new PierWeirFormula
            {
                NumberOfPiers = numPiers,
                UpstreamFacePos = upstreamFacePos,
                DesignHeadPos = designHeadPos,
                PierContractionPos = pierContractionPos,
                AbutmentContractionPos = abutContractionPos,
                UpstreamFaceNeg = upstreamFaceNeg,
                DesignHeadNeg = designHeadNeg,
                PierContractionNeg = pierContractionNeg,
                AbutmentContractionNeg = abutContractionNeg
            };
        }

        public static void AddOrifice(this IBranch branch, long id, string name, double chainage,
            FlowDirection flowDirection,
            double crestLevel, double crestWidth, double gateOpening, double corrCoeff,
            bool useLimitFlowPos, double limitFlowPos, bool useLimitFlowNeg, double limitFlowNeg)
        {
            var orifice = new Orifice
            {
                Branch = branch,
                Name = id.ToString(),
                LongName = name,
                Chainage = chainage
            };
            orifice.FlowDirection = flowDirection;
            orifice.CrestLevel = crestLevel;
            orifice.CrestWidth = crestWidth;
            
            branch.AddStructure(orifice);

            orifice.WeirFormula = new GatedWeirFormula
            {
                GateOpening = gateOpening, // openlevel = CrestHeight + GateOpening
                ContractionCoefficient = corrCoeff,
                UseMaxFlowPos = useLimitFlowPos,
                MaxFlowPos = limitFlowPos,
                UseMaxFlowNeg = useLimitFlowNeg,
                MaxFlowNeg = limitFlowNeg
            };
        }

        public static void AddGeneralStructure(this IBranch branch, long id, string name, double chainage, double gateHeight, double extraResistance,
                                                double widthLeftW1, double widthLeftWsdl, double widthCenter, double widthRightWsdr, double widthRightW2,
                                                double levelLeftZb1, double levelLeftZbsl, double levelCenter, double levelRightZbsr, double levelRightZb2,
                                                double posFreeGateFlowCoeff, double posDrownGateFlowCoeff, double posFreeWeirFlowCoeff, double posDrownWeirFlowCoeff, double posContrCoefFreeGate,
                                                double negFreeGateFlowCoeff, double negDrownGateFlowCoeff, double negFreeWeirFlowCoeff, double negDrownWeirFlowCoeff, double negContrCoefFreeGate,
                                                double lowerEdgeLevel,
                                                bool useVelocityHeight,
                                                bool useExtraResistance = true)
        {
            var weir = AddWeir1D(branch, id, name, chainage);
            weir.UseVelocityHeight = useVelocityHeight;

            weir.WeirFormula = new GeneralStructureWeirFormula
            {
                WidthLeftSideOfStructure = widthLeftW1,
                WidthStructureLeftSide = widthLeftWsdl,
                WidthStructureCentre = widthCenter,
                WidthStructureRightSide = widthRightWsdr,
                WidthRightSideOfStructure = widthRightW2,

                BedLevelLeftSideOfStructure = levelLeftZb1,
                BedLevelLeftSideStructure = levelLeftZbsl,
                BedLevelStructureCentre = levelCenter, // CrestLevel == BedLevelStructureCentre
                BedLevelRightSideStructure = levelRightZbsr,
                BedLevelRightSideOfStructure = levelRightZb2,

                GateHeight = gateHeight,
                LowerEdgeLevel = lowerEdgeLevel,

                PositiveFreeGateFlow = posFreeGateFlowCoeff,
                PositiveDrownedGateFlow = posDrownGateFlowCoeff,
                PositiveFreeWeirFlow = posFreeWeirFlowCoeff,
                PositiveDrownedWeirFlow = posDrownWeirFlowCoeff,
                PositiveContractionCoefficient = posContrCoefFreeGate,

                NegativeFreeGateFlow = negFreeGateFlowCoeff,
                NegativeDrownedGateFlow = negDrownGateFlowCoeff,
                NegativeFreeWeirFlow = negFreeWeirFlowCoeff,
                NegativeDrownedWeirFlow = negDrownWeirFlowCoeff,
                NegativeContractionCoefficient = negContrCoefFreeGate,

                UseExtraResistance = useExtraResistance,
                ExtraResistance = extraResistance
            };
        }

        public static ICulvert AddCulvert(this IBranch branch, long id, string name, double chainage, FlowDirection flowDirection, double inletLevel, double outletLevel,
                                          double length, double inletLossCoefficient, double outletLossCoefficient, bool isGated, double gateInitialOpening, 
                                          double[] relOpening, double[] lossCoeff, double friction, bool groundLayerEnabled, double groundLayerRoughness)
        {
            ICulvert culvert = new Culvert
            {
                Name = id.ToString(),
                LongName = name,
                Chainage = chainage,
                FlowDirection = flowDirection,
                InletLevel = inletLevel,
                OutletLevel = outletLevel,
                Length = length,
                Diameter = 2.0,
                InletLossCoefficient = inletLossCoefficient,
                OutletLossCoefficient = outletLossCoefficient,
                IsGated = isGated,
                GateInitialOpening = gateInitialOpening,
                Friction = friction, // friction type set in RoughnessStructureData
                FrictionDataType = Friction.Chezy,
                GroundLayerEnabled = groundLayerEnabled,
                GroundLayerRoughness = groundLayerRoughness
            };

            var argument = new Variable<double>();
            argument.Values.AddRange(relOpening);
            culvert.GateOpeningLossCoefficientFunction.Arguments.Clear();
            culvert.GateOpeningLossCoefficientFunction.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(lossCoeff);
            culvert.GateOpeningLossCoefficientFunction.Components.Clear();
            culvert.GateOpeningLossCoefficientFunction.Components.Add(component);

            branch.AddStructure(culvert);
            return culvert;
        }

        public static void AddInvertedSiphon(this IBranch branch, long id, string name, double chainage, FlowDirection flowDirection, double inletLevel, double outletLevel,
                                          double length, double inletLossCoefficient, double outletLossCoefficient, bool isGated, double gateInitialOpening,
                                          double[] relOpening, double[] lossCoeff, double friction, bool groundLayerEnabled, double groundLayerRoughness, double bendLossCoefficient)
        {
            ICulvert culvert = AddCulvert(branch, id, name, chainage, flowDirection, inletLevel, outletLevel,
                                          length, inletLossCoefficient, outletLossCoefficient, isGated, gateInitialOpening, 
                                          relOpening, lossCoeff, friction, groundLayerEnabled, groundLayerRoughness);
            culvert.CulvertType = CulvertType.InvertedSiphon;
            culvert.BendLossCoefficient = bendLossCoefficient; // this value should not be zero, otherwise you're actually creating a culvert
        }
        
        private static IBridge AddBridge(this IBranch branch, long id, string name, double chainage, FlowDirection flowDirection, double shift, long crossSectionId)
        {
            IBridge bridge = new Bridge
            {
                Branch = branch,
                Name = id.ToString(),
                LongName = name,
                Chainage = chainage,
                FlowDirection = flowDirection,
                Shift = shift
            };

            bridge.TabulatedCrossSectionDefinition.Name = crossSectionId.ToString();
            branch.AddStructure(bridge);
            return bridge;
        }

        public static void AddBridgePillar(this IBranch branch, long id, string name, double chainage, FlowDirection flowDirection, double bedLevel, long crossSectionId,
                                           double pillarWidth, double formFactor)
        {
            var bridge = AddBridge(branch, id, name, chainage, flowDirection, bedLevel, crossSectionId);
            bridge.PillarWidth = pillarWidth;
            bridge.ShapeFactor = formFactor;
            bridge.IsPillar = true;
        }

        public static void AddBridgeStandard(this IBranch branch, long id, string name, double chainage, FlowDirection flowDirection, double bedLevel, long crossSectionId,
                                             double length, double inletLossCoeff, double outletLossCoeff, double friction, double groundFriction, bool enableGroundLayer = false)
        {
            var bridge = AddBridge(branch, id, name, chainage, flowDirection, bedLevel, crossSectionId);
            bridge.Length = length;
            bridge.InletLossCoefficient = inletLossCoeff;
            bridge.OutletLossCoefficient = outletLossCoeff;
            bridge.Friction = friction;
            bridge.GroundLayerRoughness = groundFriction;
            bridge.GroundLayerEnabled = enableGroundLayer;
            bridge.IsPillar = false;
        }

        public static void AddExtraResistance(this IBranch branch, long id, string name, double chainage, double[] levels, double[] ksi)
        {
            IExtraResistance extraResistance = new ExtraResistance
            {
                Branch = branch,
                Name = id.ToString(),
                LongName = name,
                Chainage = chainage
            };

            var argument = new Variable<double>();
            argument.Values.AddRange(levels);
            extraResistance.FrictionTable.Arguments.Clear();
            extraResistance.FrictionTable.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(ksi);
            extraResistance.FrictionTable.Components.Clear();
            extraResistance.FrictionTable.Components.Add(component);

            branch.AddStructure(extraResistance);
        }

        #endregion Structure Creation Helpers

        public static void WriteCrossSectionsToIni(IEnumerable<IStructure1D> structures)
        {
            // Mock of StructureFileWriter

            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.StructureDefinitionsMajorVersion, 
                                                             GeneralRegion.StructureDefinitionsMinorVersion, 
                                                             GeneralRegion.FileTypeName.StructureDefinition)
            };

            structures.ForEach(structure =>
            {
                var definitionGeneratorStructure = DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structure.GetStructureType());
                if (definitionGeneratorStructure != null)
                {
                    var structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure);

                    switch (structure.GetStructureType())
                    {
                        case StructureType.Culvert:
                        case StructureType.InvertedSiphon:
                            var culvert = structure as Culvert;
                            if (culvert != null)
                            {
                                AddFrictionData(structureCategory, Friction.Chezy, culvert.Friction,
                                    culvert.GroundLayerEnabled ? culvert.GroundLayerRoughness : culvert.Friction);
                            }
                            break;
                        case StructureType.Bridge:
                            var bridge = structure as Bridge;
                            if (bridge != null)
                            {
                                AddFrictionData(structureCategory, Friction.Chezy, bridge.Friction,
                                    bridge.GroundLayerEnabled ? bridge.GroundLayerRoughness : bridge.Friction);
                            }
                            break;
                    }
                    categories.Add(structureCategory);
                }
            });
            
            if (File.Exists(FileWriterTestHelper.ModelFileNames.Structures)) File.Delete(FileWriterTestHelper.ModelFileNames.Structures);
            new IniFileWriter().WriteIniFile(categories, FileWriterTestHelper.ModelFileNames.Structures);
        }

        private static void AddFrictionData(DelftIniCategory category, Friction frictionType, double friction, double groundLayerRoughness)
        {
            category.AddProperty(StructureRegion.BedFrictionType.Key, ((int)frictionType).ToString(), StructureRegion.BedFrictionType.Description);
            category.AddProperty(StructureRegion.BedFriction.Key, friction, StructureRegion.BedFriction.Description, StructureRegion.BedFriction.Format);
            category.AddProperty(StructureRegion.GroundFrictionType.Key, ((int)frictionType).ToString(), StructureRegion.GroundFrictionType.Description); // This may be removed, but for now just duplicate
            category.AddProperty(StructureRegion.GroundFriction.Key, groundLayerRoughness, StructureRegion.GroundFriction.Description, StructureRegion.GroundFriction.Format);
        }

        private static void AddStructure(this IBranch branch, IStructure1D structure)
        {
            // Note: DeltaShell always wraps structures into CompositeStructures
            var composite = new CompositeBranchStructure()
            {
                Branch = branch,
                Network = branch.Network,
                Chainage = structure.Chainage,
                Geometry = (IGeometry)structure.Geometry?.Clone()
            };
            composite.Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion) branch.Network, composite);
            branch.BranchFeatures.Add(composite);
            HydroNetworkHelper.AddStructureToComposite(composite, structure);
        }
    }
}