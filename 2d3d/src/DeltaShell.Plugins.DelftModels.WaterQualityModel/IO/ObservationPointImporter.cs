using System;
using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class ObservationPointImporter : NameablePointFeatureImporter<WaterQualityObservationPoint>
    {
        public const string NewNameFormat = "ObsPoint {0}";
        public override string Name => "Observation points from GIS importer";
        public override Bitmap Image => Resources.Observation;

        protected override string NewNameFormatString => NewNameFormat;

        protected override WaterQualityObservationPoint CreateFeature()
        {
            return new WaterQualityObservationPoint();
        }

        protected override void ReadAttributes(WaterQualityObservationPoint newFeature, IFeature feature,
                                               IEnumerable<WaterQualityObservationPoint> list)
        {
            base.ReadAttributes(newFeature, feature, list);

            const string attributeName = WaterQualityObservationPoint.ObservationPointTypeAttributeName;
            if (feature.Attributes.ContainsKey(attributeName))
            {
                var o = feature.Attributes[attributeName] as string;
                if (o != null)
                {
                    newFeature.ObservationPointType =
                        (ObservationPointType) Enum.Parse(typeof(ObservationPointType), o);
                }
                else
                {
                    newFeature.ObservationPointType = ObservationPointType.SinglePoint;
                }
            }
            else
            {
                newFeature.ObservationPointType = ObservationPointType.SinglePoint;
            }
        }
    }
}