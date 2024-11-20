using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model
{
    /// <summary>
    /// Monitoring output level enumeration
    /// </summary>
    public enum MonitoringOutputLevel
    {
        [Description("None")]
        None,

        [Description("Points")]
        Points,

        [Description("Areas")]
        Areas,

        [Description("Points and areas")]
        PointsAndAreas
    }
}