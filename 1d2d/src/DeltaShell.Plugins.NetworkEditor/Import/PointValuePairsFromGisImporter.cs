using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class PointValuePairsFromGisImporter : FeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkCoverageFromGisImporter));

        private readonly PropertyMapping propertyMappingValue;
        private readonly IList<Tuple<IPoint, double>> pointValuePairList;

        public PointValuePairsFromGisImporter()
        {
            pointValuePairList = new List<Tuple<IPoint, double>>();

            propertyMappingValue = new PropertyMapping("Value", false, true);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingValue);
            base.FeatureFromGisImporterSettings.FeatureType = "Point";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
        }

        public override string Name
        {
            get { return "Coordinate value pairs from GIS"; }
        }

        private string MappedValueColumnAlias
        {
            get { return propertyMappingValue.MappingColumn.Alias; }
        }

        public IEnumerable<Tuple<IPoint, double>> PointValuePairs
        {
            get { return pointValuePairList; }
        }

        public double SnappingPrecision { get; set; }

        public override object ImportItem(string path, object target = null)
        {
            pointValuePairList.Clear();


            foreach (var feature in GetFeatures())
            {
                var x = feature.Geometry.Coordinate.X;
                var y = feature.Geometry.Coordinate.Y;
                var value = feature.Attributes[MappedValueColumnAlias];
                double parsedValue;
                if (!double.TryParse(value.ToString(), out parsedValue))
                {
                    log.ErrorFormat("Could not parse \'{0}\' in column \'{1}\' as a number. Cancelling import..", value, MappedValueColumnAlias);
                    return null;
                }

                pointValuePairList.Add(new Tuple<IPoint, double>(new Point(x, y), parsedValue));
            }

            return null;
        }

        
    }
}
