using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public static class StructureRegion
    {
        public const string Header = "Structure";

        public static class StructureTypeName
        {
            public const string Pump = "pump";
            public const string Weir = "weir";
            public const string Gate = "gate";
            public const string UniversalWeir = "universalWeir";
            public const string RiverWeir = "riverWeir";
            public const string AdvancedWeir = "advancedWeir";
            public const string Orifice = "orifice";
            public const string GeneralStructure = "generalstructure";
            public const string LeveeBreach = "dambreak";
            public const string Culvert = "culvert";
            public const string InvertedSiphon = "invertedSiphon";
            public const string Siphon = "siphon";
            public const string Bridge = "bridge";
            public const string BridgePillar = "bridgePillar";
            public const string ExtraResistanceStructure = "extraresistance";
            public const string CompoundStructure = "compound";
        }
        
        #region Common Structure Elements
        public static readonly ConfigurationSetting Id = new ConfigurationSetting(key: "id", description: "Unique definition id");
        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Given name in the user interface (optional)");
        public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "branchId", description: "Branch id");
        public static readonly ConfigurationSetting Chainage = new ConfigurationSetting(key: "chainage", description: "Chainage on the branch (m)");
        public static readonly ConfigurationSetting NumberOfCoordinates = new ConfigurationSetting(key: "numCoordinates", description: "Number of values in xCoordinates and yCoordinates. This value should be greater or equal 2");
        public static readonly ConfigurationSetting XCoordinates = new ConfigurationSetting(key: "xCoordinates", description: "x-coordinates of the location of the structure.");
        public static readonly ConfigurationSetting YCoordinates = new ConfigurationSetting(key: "yCoordinates", description: "y-coordinates of the location of the structure.");
        
        public static readonly ConfigurationSetting Compound = new ConfigurationSetting(key: "compound", description:
            "When compound is equal or less than to 0 the structure is a single structure. " +
            "In case a value greater than 0 is given, the structure is a part of a compound structure. " +
            "All structures with the same compound id are considered to be elements of the same compound structure.");
        public static readonly ConfigurationSetting CompoundName = new ConfigurationSetting(key: "compoundName", description: "Type of structure");
        public static readonly ConfigurationSetting DefinitionType = new ConfigurationSetting(key: "type", description: "");
        public static readonly ConfigurationSetting AllowedFlowDir = new ConfigurationSetting(key: "allowedflowdir", description: "0=Both, 1=Positive, 2=Negative, 3=None");
        // [Common Culvert and Common Bridge]
        public static readonly ConfigurationSetting CsDefId = new ConfigurationSetting(key: "csDefId", description: "Id of Cross-Section Definition");
        // [Common Culvert and Standard Bridge]
        public static readonly ConfigurationSetting Length = new ConfigurationSetting(key: "length", description: "Length (m)");
        public static readonly ConfigurationSetting InletLossCoeff = new ConfigurationSetting(key: "inletlosscoeff", description: "Inlet loss coefficient (-)");
        public static readonly ConfigurationSetting OutletLossCoeff = new ConfigurationSetting(key: "outletlosscoeff", description: "Outlet loss coefficient (-)");
        public static readonly ConfigurationSetting FrictionType = new ConfigurationSetting(key: "FrictionType", description:
            "Friction type, possible values are: " +
            "Chezy = 1, " +
            "Manning = 4, " +
            "Nikuradse = 5, " +
            "Strickler = 6, " +
            "WhiteColebrook = 7, " +
            "BosBijkerk = 9");
        public static readonly ConfigurationSetting Friction = new ConfigurationSetting(key: "Friction", description: "Friction Value");
        public static readonly ConfigurationSetting BedFrictionType = new ConfigurationSetting(key: "bedFrictionType", description:
            "Friction type, possible values are: " +
            "Chezy = 1, " +
            "Manning = 4, " +
            "Nikuradse = 5, " +
            "Strickler = 6, " +
            "WhiteColebrook = 7, " +
            "BosBijkerk = 9");
        public static readonly ConfigurationSetting BedFriction = new ConfigurationSetting(key: "bedFriction", description: "Friction Value");
        public static readonly ConfigurationSetting GroundFrictionType = new ConfigurationSetting(key: "groundFrictionType", description: "Friction type for ground layer");
        public static readonly ConfigurationSetting GroundFriction = new ConfigurationSetting(key: "groundFriction", description: "Friction value for ground layer");
        #endregion

        #region Pump Elements
        public static readonly ConfigurationSetting Orientation = new ConfigurationSetting(key: "orientation", description: "Pump orientation.");
        public static readonly ConfigurationSetting Direction = new ConfigurationSetting(key: "controlSide", description:
            "ABS(direction): " +
            "1: Suction side control " +
            "2: Delivery side control " +
            "3: Suction and Delivery side control");
        public static readonly ConfigurationSetting NrStages = new ConfigurationSetting(key: "numStages", description: "Number of stages in pump");
        public static readonly ConfigurationSetting Capacity = new ConfigurationSetting(key: "capacity", description: "Pump capacity (m3/s)");
        public static readonly ConfigurationSetting StartLevelSuctionSide = new ConfigurationSetting(key: "startlevelsuctionside", description: "Start level suction side (m AD)");
        public static readonly ConfigurationSetting StopLevelSuctionSide = new ConfigurationSetting(key: "stoplevelsuctionside", description: "Stop level suction side (m AD)");
        public static readonly ConfigurationSetting StartLevelDeliverySide = new ConfigurationSetting(key: "startleveldeliveryside", description: "Start level at delivery side (m AD)");
        public static readonly ConfigurationSetting StopLevelDeliverySide = new ConfigurationSetting(key: "stopleveldeliveryside", description: "Stop level at delivery side (m AD)");
        public static readonly ConfigurationSetting ReductionFactorLevels = new ConfigurationSetting(key: "numReductionLevels", description: "Number of levels in reduction table");
        public static readonly ConfigurationSetting Head = new ConfigurationSetting(key: "head", description: "Head");
        public static readonly ConfigurationSetting ReductionFactor = new ConfigurationSetting(key: "reductionfactor", description: "Reduction factor (-)");
        public static readonly ConfigurationSetting PolylineFile = new ConfigurationSetting(key: "polylinefile", description: "*.pli; Polyline geometry definition for 2D structure");
        #endregion

        #region Common Weir Elements
        // [Simple Weir & Universal Weir & Orifice]
        public static readonly ConfigurationSetting CorrectionCoeff = new ConfigurationSetting(key: "corrCoeff", description: "Correction coefficient (-)", defaultValue:"1");
        public static readonly ConfigurationSetting DischargeCoeff = new ConfigurationSetting(key: "dischargecoeff", description: "Discharge coefficient (-)");
        // [Simple Weir & River Weir & Advanced Weir & Orifice]
        public static readonly ConfigurationSetting CrestWidth = new ConfigurationSetting(key: "crestWidth", description: "Width of weir (m)");
        // [Simple Weir & Universal Weir & River Weir & Advanced Weir & Orifice]
        public static readonly ConfigurationSetting CrestLevel = new ConfigurationSetting(key: "crestLevel", description: "Crest level of weir (m AD)");
        #endregion

        #region Simple Weir Elements
        public static readonly ConfigurationSetting LatDisCoeff = new ConfigurationSetting(key: "latdiscoeff", description: "Lateral discharge coefficient (-)");
        #endregion

        #region Gate Elements

        public static readonly ConfigurationSetting GateSillLevel = new ConfigurationSetting(key: "SillWidth", description: "");
        public static readonly ConfigurationSetting GateSillWidth = new ConfigurationSetting(key: "GateSillLevel", description: "");
        public static readonly ConfigurationSetting GateCrestLevel = new ConfigurationSetting(key: "crestLevel", description: "");
        public static readonly ConfigurationSetting GateCrestWidth = new ConfigurationSetting(key: "crestWidth", description: "");
        public static readonly ConfigurationSetting GateLowerEdgeLevel = new ConfigurationSetting(key: "gateLowerEdgeLevel", description: "");
        public static readonly ConfigurationSetting GateOpeningWidth = new ConfigurationSetting(key: "OpeningWidth", description: "");
        public static readonly ConfigurationSetting GateHorizontalOpeningDirection = new ConfigurationSetting(key: "HorizontalOpeningDirection", description: "");

        #endregion

        #region Universal Weir Elements
        public static readonly ConfigurationSetting LevelsCount = new ConfigurationSetting(key: "numLevels", description:"Number of YZ-Values");
        public static readonly ConfigurationSetting YValues = new ConfigurationSetting(key: "yValues", description: "y-values as used in the computational core (m)");
        public static readonly ConfigurationSetting ZValues = new ConfigurationSetting(key: "zValues", description: "z-values as used in the computational core (m)");
        public static readonly ConfigurationSetting FreeSubmergedFactor = new ConfigurationSetting(key: "freesubmergedfactor", description: "Normally 0.667 (2/3) (-)");
        #endregion

        #region River Weir Elements
        public static readonly ConfigurationSetting PosCwCoef = new ConfigurationSetting(key: "poscwcoef", description: "Coefficient for positive direction (-)");
        public static readonly ConfigurationSetting PosSlimLimit = new ConfigurationSetting(key: "posslimlimit", description: "Submergence limit for positive direction (-)");
        public static readonly ConfigurationSetting NegCwCoef = new ConfigurationSetting(key: "negcwcoef", description: "Coefficient for negative direction (-)");
        public static readonly ConfigurationSetting NegSlimLimit = new ConfigurationSetting(key: "negslimlimit", description: "Submergence limit for negative direction (-)");
        public static readonly ConfigurationSetting PosSfCount = new ConfigurationSetting(key: "possfcount", description: "Number of table rows for positive flow direction");
        public static readonly ConfigurationSetting PosSf = new ConfigurationSetting(key: "possf", description: "Values for (h2 - z) / (h1 - z) (-)");
        public static readonly ConfigurationSetting PosRed = new ConfigurationSetting(key: "posred", description: "Reduction factors (-)");
        public static readonly ConfigurationSetting NegSfCount = new ConfigurationSetting(key: "negsfcount", description: "Number of table rows for negative flow direction");
        public static readonly ConfigurationSetting NegSf = new ConfigurationSetting(key: "negsf", description: "Values for (h2 - z) / (h1 - z) (-)");
        public static readonly ConfigurationSetting NegRed = new ConfigurationSetting(key: "negred", description: "Reduction factors (-)");
        #endregion

        #region Advanced Weir Elements
        public static readonly ConfigurationSetting NPiers = new ConfigurationSetting(key: "npiers", description: "Number of piers");
        public static readonly ConfigurationSetting PosHeight = new ConfigurationSetting(key: "posheight", description: "Upstream face height for positive flow direction (m)");
        public static readonly ConfigurationSetting PosDesignHead = new ConfigurationSetting(key: "posdesignhead", description: "Weir design head for positive flow direction (m)");
        public static readonly ConfigurationSetting PosPierContractCoef = new ConfigurationSetting(key: "pospiercontractcoef", description: "Pier contraction coefficient for positive flow direction (-)");
        public static readonly ConfigurationSetting PosAbutContractCoef = new ConfigurationSetting(key: "posabutcontractcoef", description: "Abutment contraction coefficient for positive flow direction (-)");
        public static readonly ConfigurationSetting NegHeight = new ConfigurationSetting(key: "negheight", description: "Upstream face height for negative flow direction (m)");
        public static readonly ConfigurationSetting NegDesignHead = new ConfigurationSetting(key: "negdesignhead", description: "Weir design head for negative flow direction (m)");
        public static readonly ConfigurationSetting NegPierContractCoef = new ConfigurationSetting(key: "negpiercontractcoef", description: "Pier contraction coefficient for negative flow direction (-)");
        public static readonly ConfigurationSetting NegAbutContractCoef = new ConfigurationSetting(key: "negabutcontractcoef", description: "Abutment contraction coefficient for negative flow direction (-)");
        #endregion

        #region Orifice Elements
        public static readonly ConfigurationSetting OpenLevel = new ConfigurationSetting(key: "openlevel", description: "Gate height (m)");
        public static readonly ConfigurationSetting ContractionCoeff = new ConfigurationSetting(key: "contractioncoeff", description: "Contraction coefficient (-)");
        public static readonly ConfigurationSetting LatContrCoeff = new ConfigurationSetting(key: "latcontrcoeff", description: "Lateral contraction coefficient (-)");
        public static readonly ConfigurationSetting UseLimitFlowPos = new ConfigurationSetting(key: "uselimitflowpos", description: "0 = unlimited, 1 = limited");
        public static readonly ConfigurationSetting LimitFlowPos = new ConfigurationSetting(key: "limitflowpos", description: "Maximum positive flow (m3/s)");
        public static readonly ConfigurationSetting UseLimitFlowNeg = new ConfigurationSetting(key: "uselimitflowneg", description: "0 = unlimited, 1 = limited");
        public static readonly ConfigurationSetting LimitFlowNeg = new ConfigurationSetting(key: "limitflowneg", description: "Maximum negative flow (m3/s)");
        #endregion

        #region General Structure Elements
        public static readonly ConfigurationSetting WidthLeftW1 = new ConfigurationSetting(key: KnownGeneralStructureProperties.WidthLeftW1.GetDescription(), description: "Width left side of structure (m)");
        public static readonly ConfigurationSetting WidthLeftWsdl = new ConfigurationSetting(key: KnownGeneralStructureProperties.WidthLeftWsdl.GetDescription(), description: "Width structure left side (m)");
        public static readonly ConfigurationSetting WidthCenter = new ConfigurationSetting(key: KnownGeneralStructureProperties.WidthCenter.GetDescription(), description: "Width structure centre (m)");
        public static readonly ConfigurationSetting WidthRightWsdr = new ConfigurationSetting(key: KnownGeneralStructureProperties.WidthRightWsdr.GetDescription(), description: "Width structure right side (m)");
        public static readonly ConfigurationSetting WidthRightW2 = new ConfigurationSetting(key: KnownGeneralStructureProperties.WidthRightW2.GetDescription(), description: "Width right side of structure (m)");
        public static readonly ConfigurationSetting LevelLeftZb1 = new ConfigurationSetting(key: KnownGeneralStructureProperties.LevelLeftZb1.GetDescription(), description: "Bed level left side of structure (m AD)");
        public static readonly ConfigurationSetting LevelLeftZbsl = new ConfigurationSetting(key: KnownGeneralStructureProperties.LevelLeftZbsl.GetDescription(), description: "Bed level left side structure (m AD)");
        public static readonly ConfigurationSetting LevelCenter = new ConfigurationSetting(key: KnownGeneralStructureProperties.LevelCenter.GetDescription(), description: "Bed level at centre of structure (m AD)");
        public static readonly ConfigurationSetting LevelRightZbsr = new ConfigurationSetting(key: KnownGeneralStructureProperties.LevelRightZbsr.GetDescription(), description: "Bed level right side structure (m AD)");
        public static readonly ConfigurationSetting LevelRightZb2 = new ConfigurationSetting(key: KnownGeneralStructureProperties.LevelRightZb2.GetDescription(), description: "Bed level right side of structure (m AD)");
        public static readonly ConfigurationSetting GateHeight = new ConfigurationSetting(key: KnownGeneralStructureProperties.GateHeight.GetDescription(), description: "Gate lower edge level (m AD)");
        public static readonly ConfigurationSetting PosFreeGateFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient.GetDescription(), description: "Positive free gate flow (-)");
        public static readonly ConfigurationSetting PosDrownGateFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient.GetDescription(), description: "Positive drowned gate flow (-)");
        public static readonly ConfigurationSetting PosFreeWeirFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription(), description: "Positive free weir flow (-)");
        public static readonly ConfigurationSetting PosDrownWeirFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient.GetDescription(), description: "Positive drowned weir flow (-)");
        public static readonly ConfigurationSetting PosContrCoefFreeGate = new ConfigurationSetting(key: KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate.GetDescription(), description: "Positive flow contraction coefficient (-)");
        public static readonly ConfigurationSetting NegFreeGateFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient.GetDescription(), description: "Negative free gate flow (-)");
        public static readonly ConfigurationSetting NegDrownGateFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient.GetDescription(), description: "Negative drowned gate flow (-)");
        public static readonly ConfigurationSetting NegFreeWeirFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient.GetDescription(), description: "Negative free weir flow (-)");
        public static readonly ConfigurationSetting NegDrownWeirFlowCoeff = new ConfigurationSetting(key: KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient.GetDescription(), description: "Negative drowned weir flow (-)");
        public static readonly ConfigurationSetting NegContrCoefFreeGate = new ConfigurationSetting(key: KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate.GetDescription(), description: "Negative flow contraction coefficient (-)");
        public static readonly ConfigurationSetting ExtraResistance = new ConfigurationSetting(key: KnownGeneralStructureProperties.ExtraResistance.GetDescription(), description: "Extra resistance (-)");
        public static readonly ConfigurationSetting GateDoorHeight = new ConfigurationSetting(key: KnownGeneralStructureProperties.GateDoorHeightGeneralStructure.GetDescription(), description: "Gate opening height (m)");
        #endregion

        #region Common Culvert Elements
        public static readonly ConfigurationSetting LeftLevel = new ConfigurationSetting(key: "leftLevel", description: "Left bed level (m AD)");
        public static readonly ConfigurationSetting RightLevel = new ConfigurationSetting(key: "rightLevel", description: "Right bed level (m AD)");
        public static readonly ConfigurationSetting ValveOnOff = new ConfigurationSetting(key: "valveOnOff", description: "Flag for having valve or not (0=no valve, 1=valve)");
        public static readonly ConfigurationSetting IniValveOpen = new ConfigurationSetting(key: "valveOpeningHeight", description: "Initial valve opening height (m)");
        public static readonly ConfigurationSetting LossCoeffCount = new ConfigurationSetting(key: "numLossCoeff", description: "Number of rows in table");
        public static readonly ConfigurationSetting RelativeOpening = new ConfigurationSetting(key: "relOpening", description: "Relative valve opening (0.0 — 1.0)");
        public static readonly ConfigurationSetting LossCoefficient = new ConfigurationSetting(key: "lossCoeff", description: "Loss coefficients (-)");
        #endregion

        #region Siphon & Inverted Siphon Elements
        public static readonly ConfigurationSetting BendLossCoef = new ConfigurationSetting(key: "bendLossCoeff", description: "Bend loss coefficient (-)");
        #endregion

        #region Siphon Elements
        public static readonly ConfigurationSetting TurnOnLevel = new ConfigurationSetting(key: "turnOnLevel", description: "Start level of operation of siphon (m AD)");
        public static readonly ConfigurationSetting TurnOffLevel = new ConfigurationSetting(key: "turnOffLevel", description: "Stop level of operation of siphon (m AD)");
        #endregion

        #region Common Bridge Elements
        public static readonly ConfigurationSetting BedLevel = new ConfigurationSetting(key: "bedlevel", description: "Lowest point of bridge profile (crest) (m AD)");
        #endregion

        #region Bridge Pillar Elements
        public static readonly ConfigurationSetting PillarWidth = new ConfigurationSetting(key: "pillarwidth", description: "Total width of pillars in flow direction (m)");
        public static readonly ConfigurationSetting FormFactor = new ConfigurationSetting(key: "formfactor", description: "Shape factor (-)");
        #endregion

        #region Extra Resistance Elements
        public static readonly ConfigurationSetting NumValues = new ConfigurationSetting(key: "numValues", description: "Number of values");
        public static readonly ConfigurationSetting Levels = new ConfigurationSetting(key: "levels", description: "Water levels (m AD)");
        public static readonly ConfigurationSetting Ksi = new ConfigurationSetting(key: "ksi", description: "KSI-values (s2/m5)", format:"G6");
        #endregion

        #region Levee Breach Elements
        
        public static readonly ConfigurationSetting BreachLocationX = new ConfigurationSetting(key: "StartLocationX");
        public static readonly ConfigurationSetting BreachLocationY = new ConfigurationSetting(key: "StartLocationY");
        public static readonly ConfigurationSetting StartTimeBreachGrowth = new ConfigurationSetting(key: "T0");
        public static readonly ConfigurationSetting BreachGrowthActivated = new ConfigurationSetting(key: "State");
        public static readonly ConfigurationSetting Algorithm = new ConfigurationSetting(key: "Algorithm", description: "# 1 VdKnaap ,2 Verheij-vdKnaap");
        public static readonly ConfigurationSetting InitialCrestLevel = new ConfigurationSetting(key: "CrestLevelIni");
        public static readonly ConfigurationSetting MinimumCrestLevel = new ConfigurationSetting(key: "CrestLevelMin");
        public static readonly ConfigurationSetting InitalBreachWidth = new ConfigurationSetting(key: "BreachWidthIni");
        public static readonly ConfigurationSetting TimeToReachMinimumCrestLevel = new ConfigurationSetting(key: "TimeToBreachToMaximumDepth");
        public static readonly ConfigurationSetting Factor1 = new ConfigurationSetting(key: "F1");
        public static readonly ConfigurationSetting Factor2 = new ConfigurationSetting(key: "F2");
        public static readonly ConfigurationSetting CriticalFlowVelocity = new ConfigurationSetting(key: "Ucrit");
        public static readonly ConfigurationSetting TimeFileName = new ConfigurationSetting(key: "DambreakLevelsAndWidths");

        #endregion
        #region compound
        public static readonly ConfigurationSetting NumberOfCompoundStructures = new ConfigurationSetting(key: "numStructures", description: "Number of individual structures in compound structure");
        public static readonly ConfigurationSetting StructureIds = new ConfigurationSetting(key: "structureIds", description: "Semicolon separated list of structure ids.");
        

        #endregion
    }
}
