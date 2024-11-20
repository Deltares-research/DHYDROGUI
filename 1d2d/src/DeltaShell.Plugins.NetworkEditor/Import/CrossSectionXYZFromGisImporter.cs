using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class CrossSectionXYZFromGisImporter: NetworkFeatureFromGisImporterBase
    {
        private class XYComparer : IEqualityComparer<Coordinate>
        {
            public bool Equals(Coordinate first, Coordinate second)
            {
                if (first == null)
                {
                    return second == null;
                }
                if (second == null)
                {
                    return false;
                }
                return first.X == second.X && first.Y == second.Y;
            }

            public int GetHashCode(Coordinate obj)
            {
                return obj.GetHashCode();
            }
        }

        private enum ImportDataType
        {
            ShapeFile,
            Sql,
            Unknown,
            None
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionXYZFromGisImporter));

        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingZ;

        public CrossSectionXYZFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");
            propertyMappingZ = new PropertyMapping("Z-values", true, false);

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingZ);

            base.FeatureFromGisImporterSettings.FeatureType = "Cross Sections XYZ";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
        }

        ImportDataType DataType
        {
            get
            { 
                var provider = SelectedFileBasedFeatureProviders.FirstOrDefault();
            
                if (provider == null)
                {
                    return ImportDataType.None;
                }
                if (provider is OgrFeatureProvider)
                {
                    return ImportDataType.Sql;
                }
                if (provider is ShapeFile)
                {
                    return ImportDataType.ShapeFile;
                }
                return ImportDataType.Unknown;
            }
        }

        public override FeatureFromGisImporterSettings FeatureFromGisImporterSettings
        {
            get
            {
                return base.FeatureFromGisImporterSettings;
            }
            set
            {
                propertyMappingName = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingName.PropertyName);
                propertyMappingLongName = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLongName.PropertyName);
                propertyMappingDescription = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingDescription.PropertyName);
                propertyMappingZ = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingZ.PropertyName);

                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingZ.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "XYZ cross-section from GIS importer"; }
        }

        private void ImportItemFromShapeFile()
        {
            var features = GetFeatures();
            
            foreach (var feature in features)
            {
                ImportCrossSection(feature, null);
            }
        }

        private void ImportItemFromSql()
        {
            var sql = GetSqlFromPropertiesMapping();
            var order = " ORDER BY [" + FeatureFromGisImporterSettings.TableName + "." +
                        FeatureFromGisImporterSettings.ColumnNameID + "],[" + propertyMappingZ.MappingColumn.TableName +
                        ".OBJECTID]";

            var features = GetFeaturesBySql(sql + order);
            IFeature previousFeature = null;
            string previousName = null;
            var coordinates = new List<Coordinate>();

            foreach (var feature in features)
            {
                var name = feature.Attributes[propertyMappingName.MappingColumn.Alias].ToString();

                if (!Equals(name, previousName)) //new profile
                {
                    try
                    {
                        ImportCrossSection(previousFeature, new LineString(coordinates.ToArray()));
                    }
                    catch (Exception e)
                    {
                        log.Warn("Skipped import of cross section", e);
                    }
                    
                    coordinates.Clear();
                }
                try
                {
                    var zValue = propertyMappingZ.MappingColumn.IsNullValue
                                    ? feature.Geometry.Coordinate.Z
                                    : Convert.ToDouble(feature.Attributes[propertyMappingZ.MappingColumn.Alias]);

                    coordinates.Add(new Coordinate(feature.Geometry.Coordinate.X, feature.Geometry.Coordinate.Y, zValue));
                }
                catch (Exception e)
                {
                    log.Warn("Skipped profile coordinate", e);
                }                
                previousFeature = feature;
                previousName = name;
            }
            try
            {
                ImportCrossSection(previousFeature, new LineString(coordinates.ToArray()));
            }
            catch (Exception e)
            {
                log.Warn("Skipped import of cross section", e);
            }
        }

        public override object ImportItem(string path, object target = null)
        {
            switch (DataType)
            {
                case ImportDataType.ShapeFile:
                    ImportItemFromShapeFile();
                    break;
                case ImportDataType.Sql:
                    ImportItemFromSql();
                    break;
            }
            return HydroNetwork;
        }

        
        private void ImportCrossSection(IFeature feature, IGeometry lineString)
        {
            if (feature == null)
            {
                return;
            }
            if (lineString != null)
            {
                if (lineString.Coordinates.Count() < 2)
                {
                    throw new ArgumentException("too few coordinates read for XYZ profile construction");
                }
                feature.Geometry = new LineString(lineString.Coordinates.Distinct(new XYComparer()).ToArray());
            }
            var crossSection = AddOrUpdateCrossSectionFromHydroNetwork(feature, propertyMappingName.MappingColumn.Alias,
                                                                       CrossSectionType.GeometryBased);
            if (crossSection == null) return;

            var longNameKey = propertyMappingLongName.MappingColumn.Alias;
            if (longNameKey != null)
            {
                crossSection.LongName = feature.Attributes[longNameKey].ToString();
            }
            var descriptionKey = propertyMappingDescription.MappingColumn.Alias;
            if (descriptionKey != null)
            {
                crossSection.Description = feature.Attributes[descriptionKey] + "(" + crossSection.Name + ")";
            }
        }
    }
}
