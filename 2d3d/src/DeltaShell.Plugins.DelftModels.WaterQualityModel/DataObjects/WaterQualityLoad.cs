using System.ComponentModel;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects
{
    public class WaterQualityLoad : NameablePointFeature, IHasLocationAliases
    {
        public const string LocationAliasExportName = "Aliases";
        public const string LoadTypeExportName = "LoadType";

        public WaterQualityLoad()
        {
            LoadType = string.Empty;
        }

        [DisplayName("Load type")]
        [FeatureAttribute(Order = 4, ExportName = LoadTypeExportName)]
        public virtual string LoadType { get; set; }

        [DisplayName("Location Aliases")]
        [FeatureAttribute(Order = 5, ExportName = LocationAliasExportName)]
        public virtual string LocationAliases { get; set; }
    }
}