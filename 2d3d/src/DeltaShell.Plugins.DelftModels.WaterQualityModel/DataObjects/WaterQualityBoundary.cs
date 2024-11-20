using System.ComponentModel;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects
{
    public class WaterQualityBoundary : Feature2D, IHasLocationAliases
    {
        [FeatureAttribute(Order = 1)]
        [ReadOnly(true)]
        public override string Name { get; set; }

        [DisplayName("Location Aliases")]
        [FeatureAttribute(ExportName = "aliases", Order = 2)]
        public virtual string LocationAliases { get; set; }
    }
}