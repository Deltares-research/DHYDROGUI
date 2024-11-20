using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class LoadsImporter : NameablePointFeatureImporter<WaterQualityLoad>
    {
        public const string NewNameFormat = "Load {0}";

        public override string Name => "Dry waste loads from GIS importer";
        public override Bitmap Image => Resources.weight;

        protected override string NewNameFormatString => NewNameFormat;

        protected override WaterQualityLoad CreateFeature()
        {
            return new WaterQualityLoad();
        }

        protected override void ReadAttributes(WaterQualityLoad newFeature, IFeature feature,
                                               IEnumerable<WaterQualityLoad> list)
        {
            base.ReadAttributes(newFeature, feature, list);

            if (feature.Attributes.ContainsKey(WaterQualityLoad.LoadTypeExportName))
            {
                newFeature.LoadType = feature.Attributes[WaterQualityLoad.LoadTypeExportName] as string ?? string.Empty;
            }
            else
            {
                newFeature.LoadType = string.Empty;
            }

            if (feature.Attributes.ContainsKey(WaterQualityLoad.LocationAliasExportName))
            {
                newFeature.LocationAliases = feature.Attributes[WaterQualityLoad.LocationAliasExportName] as string ??
                                             string.Empty;
            }
            else
            {
                newFeature.LocationAliases = string.Empty;
            }
        }
    }
}