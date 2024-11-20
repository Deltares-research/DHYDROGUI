using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Distribution type used for a <see cref="MeteoData"/>
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum MeteoDataDistributionType
    {
        [ResourcesDescription(typeof(Resources), "MeteoDataDistributionType_Global")]
        Global,
        
        [ResourcesDescription(typeof(Resources), "MeteoDataDistributionType_Per_Catchment")]
        PerFeature,

        [ResourcesDescription(typeof(Resources), "MeteoDataDistributionType_Per_Station")]
        PerStation
    }
}