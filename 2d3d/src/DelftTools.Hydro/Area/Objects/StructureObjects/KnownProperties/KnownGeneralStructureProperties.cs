using System.ComponentModel;

namespace DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties
{
    public enum KnownGeneralStructureProperties
    {
        [Description("Upstream2Width")]
        Upstream2Width,

        [Description("Upstream1Width")]
        Upstream1Width,

        [Description(KnownStructureProperties.CrestWidth)]
        CrestWidth,

        [Description("Downstream1Width")]
        Downstream1Width,

        [Description("Downstream2Width")]
        Downstream2Width,

        [Description("Upstream2Level")]
        Upstream2Level,

        [Description("Upstream1Level")]
        Upstream1Level,

        [Description(KnownStructureProperties.CrestLevel)]
        CrestLevel,

        [Description("Downstream1Level")]
        Downstream1Level,

        [Description("Downstream2Level")]
        Downstream2Level,

        [Description("pos_freegateflowcoeff")]
        PositiveFreeGateFlowCoefficient,

        [Description("pos_drowngateflowcoeff")]
        PositiveDrownGateFlowCoefficient,

        [Description("pos_freeweirflowcoeff")]
        PositiveFreeWeirFlowCoefficient,

        [Description("pos_drownweirflowcoeff")]
        PositiveDrownWeirFlowCoefficient,

        [Description("pos_contrcoeffreegate")]
        PositiveContractionCoefficientFreeGate,

        [Description("neg_freegateflowcoeff")]
        NegativeFreeGateFlowCoefficient,

        [Description("neg_drowngateflowcoeff")]
        NegativeDrownGateFlowCoefficient,

        [Description("neg_freeweirflowcoeff")]
        NegativeFreeWeirFlowCoefficient,

        [Description("neg_drownweirflowcoeff")]
        NegativeDrownWeirFlowCoefficient,

        [Description("neg_contrcoeffreegate")]
        NegativeContractionCoefficientFreeGate,

        [Description("extraresistance")]
        ExtraResistance,

        [Description(KnownStructureProperties.GateLowerEdgeLevel)]
        GateLowerEdgeLevel,

        [Description(KnownStructureProperties.GateHeight)]
        GateHeight,

        [Description(KnownStructureProperties.GateOpeningHorizontalDirection)]
        GateOpeningHorizontalDirection,

        [Description(KnownStructureProperties.GateOpeningWidth)]
        GateOpeningWidth
    }
}