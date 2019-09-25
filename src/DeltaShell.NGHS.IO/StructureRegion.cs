using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO
{
    public static class StructureRegion
    {
        public static class StructureTypeName
        {
            public const string Pump = "pump";
            public const string Weir = "weir";
            public const string Gate = "gate";
            public const string GeneralStructure = "generalstructure";
        }
        
        #region General Structure Elements
        public static readonly ConfigurationSetting WidthLeftW1 = new ConfigurationSetting(key: KnownGeneralStructureProperties.Upstream2Width.GetDescription(), description: "Width left side of structure (m)");
        public static readonly ConfigurationSetting WidthLeftWsdl = new ConfigurationSetting(key: KnownGeneralStructureProperties.Upstream1Width.GetDescription(), description: "Width structure left side (m)");
        public static readonly ConfigurationSetting WidthCenter = new ConfigurationSetting(key: KnownGeneralStructureProperties.CrestWidth.GetDescription(), description: "Width structure centre (m)");
        public static readonly ConfigurationSetting WidthRightWsdr = new ConfigurationSetting(key: KnownGeneralStructureProperties.Downstream1Width.GetDescription(), description: "Width structure right side (m)");
        public static readonly ConfigurationSetting WidthRightW2 = new ConfigurationSetting(key: KnownGeneralStructureProperties.Downstream2Width.GetDescription(), description: "Width right side of structure (m)");
        public static readonly ConfigurationSetting LevelLeftZb1 = new ConfigurationSetting(key: KnownGeneralStructureProperties.Upstream2Level.GetDescription(), description: "Bed level left side of structure (m AD)");
        public static readonly ConfigurationSetting LevelLeftZbsl = new ConfigurationSetting(key: KnownGeneralStructureProperties.Upstream1Level.GetDescription(), description: "Bed level left side structure (m AD)");
        public static readonly ConfigurationSetting LevelCenter = new ConfigurationSetting(key: KnownGeneralStructureProperties.CrestLevel.GetDescription(), description: "Bed level at centre of structure (m AD)");
        public static readonly ConfigurationSetting LevelRightZbsr = new ConfigurationSetting(key: KnownGeneralStructureProperties.Downstream1Level.GetDescription(), description: "Bed level right side structure (m AD)");
        public static readonly ConfigurationSetting LevelRightZb2 = new ConfigurationSetting(key: KnownGeneralStructureProperties.Downstream2Level.GetDescription(), description: "Bed level right side of structure (m AD)");
        public static readonly ConfigurationSetting GateHeight = new ConfigurationSetting(key: KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(), description: "Gate lower edge level (m AD)");
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
        public static readonly ConfigurationSetting GateDoorHeight = new ConfigurationSetting(key: KnownGeneralStructureProperties.GateHeight.GetDescription(), description: "Gate opening height (m)");
        #endregion
    }
}
