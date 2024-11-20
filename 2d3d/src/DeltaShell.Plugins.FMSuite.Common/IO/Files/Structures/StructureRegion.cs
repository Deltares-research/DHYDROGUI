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

        public static readonly ConfigurationSetting Upstream1Width = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream1Width.GetDescription(), "Upstream width 1 (m)");
        public static readonly ConfigurationSetting Upstream2Width = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream2Width.GetDescription(), "Upstream width 1 (m)");
        public static readonly ConfigurationSetting CrestWidth = new ConfigurationSetting(KnownGeneralStructureProperties.CrestWidth.GetDescription(), "Crest width (m)");
        public static readonly ConfigurationSetting Downstream1Width = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream1Width.GetDescription(), "Downstream width 1 (m)");
        public static readonly ConfigurationSetting Downstream2Width = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream2Width.GetDescription(), "Downstream width 2 (m)");
        public static readonly ConfigurationSetting Upstream1Level = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream1Level.GetDescription(), "Upstream level 1 (m AD)");
        public static readonly ConfigurationSetting Upstream2Level = new ConfigurationSetting(KnownGeneralStructureProperties.Upstream2Level.GetDescription(), "Upstream level 2 (m AD)");
        public static readonly ConfigurationSetting CrestLevel = new ConfigurationSetting(KnownGeneralStructureProperties.CrestLevel.GetDescription(), "Crest level (m AD)");
        public static readonly ConfigurationSetting Downstream1Level = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream1Level.GetDescription(), "Downstream level 1 (m AD)");
        public static readonly ConfigurationSetting Downstream2Level = new ConfigurationSetting(KnownGeneralStructureProperties.Downstream2Level.GetDescription(), "Downstream level 2 (m AD)");
        public static readonly ConfigurationSetting GateLowerEdgeLevel = new ConfigurationSetting(KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(), "Gate lower edge level (m AD)");
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
        public static readonly ConfigurationSetting GateHeight = new ConfigurationSetting(KnownGeneralStructureProperties.GateHeight.GetDescription(), "Gate opening height (m)");

        #endregion
    }
}