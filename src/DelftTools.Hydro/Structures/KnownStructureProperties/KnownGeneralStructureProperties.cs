using System.ComponentModel;

namespace DelftTools.Hydro.Structures.KnownStructureProperties
{
    public enum KnownGeneralStructureProperties
    {
        [Description("widthleftW1")] WidthLeftW1,
        [Description("widthleftWsdl")] WidthLeftWsdl,
        [Description("widthcenter")] WidthCenter,
        [Description("widthrightWsdr")] WidthRightWsdr,
        [Description("widthrightW2")] WidthRightW2,
        [Description("levelleftZb1")] LevelLeftZb1,
        [Description("levelleftZbsl")] LevelLeftZbsl,
        [Description("levelcenter")] LevelCenter,
        [Description("levelrightZbsr")] LevelRightZbsr,
        [Description("levelrightZb2")] LevelRightZb2,
        [Description("gateheight")] GateHeight,
        [Description("pos_freegateflowcoeff")] PositiveFreeGateFlowCoefficient,
        [Description("pos_drowngateflowcoeff")] PositiveDrownGateFlowCoefficient,
        [Description("pos_freeweirflowcoeff")] PositiveFreeWeirFlowCoefficient,
        [Description("pos_drownweirflowcoeff")] PositiveDrownWeirFlowCoefficient,
        [Description("pos_contrcoeffreegate")] PositiveContractionCoefficientFreeGate,
        [Description("neg_freegateflowcoeff")] NegativeFreeGateFlowCoefficient,
        [Description("neg_drowngateflowcoeff")] NegativeDrownGateFlowCoefficient,
        [Description("neg_freeweirflowcoeff")] NegativeFreeWeirFlowCoefficient,
        [Description("neg_drownweirflowcoeff")] NegativeDrownWeirFlowCoefficient,
        [Description("neg_contrcoeffreegate")] NegativeContractionCoefficientFreeGate,
        [Description("extraresistance")] ExtraResistance,
        [Description("gatedoorheight")] GateDoorHeightGeneralStructure
    }
}