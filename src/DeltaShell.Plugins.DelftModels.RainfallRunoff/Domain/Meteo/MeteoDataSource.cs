using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// <see cref="MeteoDataSource"/> defines the possible sources for time-series used in the
    /// <see cref="MeteoData"/>.
    /// </summary>
    public enum MeteoDataSource
    {
        [ResourcesDescription(typeof(Resources), "MeteoDataSource_UserDefined")]
        UserDefined,
        [ResourcesDescription(typeof(Resources), "MeteoDataSource_LongTermAverage")]
        LongTermAverage,
        [ResourcesDescription(typeof(Resources), "MeteoDataSource_GuidelineSewerSystems")]
        GuidelineSewerSystems,
    }
}