using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public enum DryweatherFlowDistributionType
    {
        [Description("Daily")]
        Daily,

        [Description("Variable")]
        Variable,

        [Description("Constant")]
        Constant,
    }

    public enum DischargeType
    {
        [Description("None")]
        None,

        [Description("VWD")]
        DryWeatherFlow,

        [Description("LAT")]
        Lateral
    }

    public enum NwrwSurfaceType
    {
        [Description("GVH_HEL")]
        ClosedPavedWithSlope,
        [Description("GVH_VLA")]
        ClosedPavedFlat,
        [Description("GVH_VLU")]
        ClosedPavedFlatStretch,
        [Description("OVH_HEL")]
        OpenPavedWithSlope,
        [Description("OVH_VLA")]
        OpenPavedFlat,
        [Description("OVH_VLU")]
        OpenPavedFlatStretched,
        [Description("DAK_HEL")]
        RoofWithSlope,
        [Description("DAK_VLA")]
        RoofFlat,
        [Description("DAK_VLU")]
        RoofFlatStretched,
        [Description("ONV_HEL")]
        UnpavedWithSlope,
        [Description("ONV_VLA")]
        UnpavedFlat,
        [Description("ONV_VLU")]
        UnpavedFlatStretched
    }

}
