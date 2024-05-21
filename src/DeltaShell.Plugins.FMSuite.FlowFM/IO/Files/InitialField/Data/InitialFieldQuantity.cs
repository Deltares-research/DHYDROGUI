using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data
{
    /// <summary>
    /// Type of initial field quantity.
    /// </summary>
    public enum InitialFieldQuantity
    {
        /// <summary>
        /// No type defined
        /// </summary>
        [Description("")]
        None,

        [Description("bedlevel")]
        BedLevel,

        [Description("waterlevel")]
        WaterLevel,

        [Description("waterdepth")]
        WaterDepth,

        [Description("InterceptionLayerThickness")]
        InterceptionLayerThickness,

        [Description("PotentialEvaporation")]
        PotentialEvaporation,

        [Description("InfiltrationCapacity")]
        InfiltrationCapacity,

        [Description("HortonMaxInfCap")]
        HortonMaxInfCap,

        [Description("HortonMinInfCap")]
        HortonMinInfCap,

        [Description("HortonDecreaseRate")]
        HortonDecreaseRate,

        [Description("HortonRecoveryRate")]
        HortonRecoveryRate,

        [Description("frictioncoefficient")]
        FrictionCoefficient
    }
}