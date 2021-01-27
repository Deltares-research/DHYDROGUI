using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
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

        public static readonly ConfigurationSetting WidthLeftW1 = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream2Width.GetDescription(), "Width left side of structure (m)");
        public static readonly ConfigurationSetting WidthLeftWsdl = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream1Width.GetDescription(), "Width structure left side (m)");
        public static readonly ConfigurationSetting WidthCenter = new ConfigurationSetting(KnownGeneralStructureProperties.CrestWidth.GetDescription(), "Width structure centre (m)");
        public static readonly ConfigurationSetting WidthRightWsdr = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream1Width.GetDescription(), "Width structure right side (m)");
        public static readonly ConfigurationSetting WidthRightW2 = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream2Width.GetDescription(), "Width right side of structure (m)");
        public static readonly ConfigurationSetting LevelLeftZb1 = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream2Level.GetDescription(), "Bed level left side of structure (m AD)");
        public static readonly ConfigurationSetting LevelLeftZbsl = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream1Level.GetDescription(), "Bed level left side structure (m AD)");
        public static readonly ConfigurationSetting LevelCenter = new ConfigurationSetting(KnownGeneralStructureProperties.CrestLevel.GetDescription(), "Bed level at centre of structure (m AD)");
        public static readonly ConfigurationSetting LevelRightZbsr = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream1Level.GetDescription(), "Bed level right side structure (m AD)");
        public static readonly ConfigurationSetting LevelRightZb2 = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream2Level.GetDescription(), "Bed level right side of structure (m AD)");
        public static readonly ConfigurationSetting GateHeight = new ConfigurationSetting(KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(), "Gate lower edge level (m AD)");
        public static readonly ConfigurationSetting PosFreeGateFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient.GetDescription(), "Positive free gate flow (-)");
        public static readonly ConfigurationSetting PosDrownGateFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient.GetDescription(), "Positive drowned gate flow (-)");
        public static readonly ConfigurationSetting PosFreeWeirFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription(), "Positive free weir flow (-)");
        public static readonly ConfigurationSetting PosDrownWeirFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient.GetDescription(), "Positive drowned weir flow (-)");
        public static readonly ConfigurationSetting PosContrCoefFreeGate = new ConfigurationSetting(KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate.GetDescription(), "Positive flow contraction coefficient (-)");
        public static readonly ConfigurationSetting NegFreeGateFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient.GetDescription(), "Negative free gate flow (-)");
        public static readonly ConfigurationSetting NegDrownGateFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient.GetDescription(), "Negative drowned gate flow (-)");
        public static readonly ConfigurationSetting NegFreeWeirFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient.GetDescription(), "Negative free weir flow (-)");
        public static readonly ConfigurationSetting NegDrownWeirFlowCoeff = new ConfigurationSetting(KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient.GetDescription(), "Negative drowned weir flow (-)");
        public static readonly ConfigurationSetting NegContrCoefFreeGate = new ConfigurationSetting(KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate.GetDescription(), "Negative flow contraction coefficient (-)");
        public static readonly ConfigurationSetting ExtraResistance = new ConfigurationSetting(KnownGeneralStructureProperties.ExtraResistance.GetDescription(), "Extra resistance (-)");
        public static readonly ConfigurationSetting GateDoorHeight = new ConfigurationSetting(KnownGeneralStructureProperties.GateHeight.GetDescription(), "Gate opening height (m)");

        #endregion
    }
}