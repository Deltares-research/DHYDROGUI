using System.ComponentModel;

namespace DelftTools.Hydro.Structures.KnownStructureProperties
{
    public enum KnownGeneralStructureProperties
    {
        [Description("upstream1Width")] Upstream1Width,
        [Description("upstream2Width")] Upstream2Width,
        [Description("crestWidth")] CrestWidth,
        [Description("downstream1Width")] Downstream1Width,
        [Description("downstream2Width")] Downstream2Width,
        [Description("upstream1Level")] Upstream1Level,
        [Description("upstream2Level")] Upstream2Level,
        [Description("crestLevel")] CrestLevel,
        [Description("downstream1Level")] Downstream1Level,
        [Description("downstream2Level")] Downstream2Level,
        [Description("gateLowerEdgeLevel")] GateLowerEdgeLevel,
        [Description("posFreeGateFlowCoeff")] PosFreeGateFlowCoeff,
        [Description("posDrownGateFlowCoeff")] PosDrownGateFlowCoeff,
        [Description("posFreeWeirFlowCoeff")] PosFreeWeirFlowCoeff,
        [Description("posDrownWeirFlowCoeff")] PosDrownWeirFlowCoeff,
        [Description("posContrCoefFreeGate")] PosContrCoefFreeGate,
        [Description("negFreeGateFlowCoeff")] NegFreeGateFlowCoeff,
        [Description("negDrownGateFlowCoeff")] NegDrownGateFlowCoeff,
        [Description("negFreeWeirFlowCoeff")] NegFreeWeirFlowCoeff,
        [Description("negDrownWeirFlowCoeff")] NegDrownWeirFlowCoeff,
        [Description("negContrCoefFreeGate")] NegContrCoefFreeGate,
        [Description("extraResistance")] ExtraResistance,
        [Description("gateHeight")] GateHeight,
        [Description("gateOpeningWidth")] GateOpeningWidth,
        [Description("crestLength")] CrestLength,
        [Description("useVelocityHeight")] UseVelocityHeight,
        [Description("gateOpeningHorizontalDirection")] GateOpeningHorizontalDirection
    }
}