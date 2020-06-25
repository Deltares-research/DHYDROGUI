using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class CatchmentFromGisImporter : BasinFeatureFromGisImporterBase
    {
        private readonly PropertyMapping propertyMappingName;
        private readonly PropertyMapping propertyMappingLongName;
        private readonly PropertyMapping propertyMappingDescription;
        private readonly PropertyMapping propertyMappingCatchmentType;

        public CatchmentFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name") {IsRequired = true, IsUnique =  true};
            propertyMappingCatchmentType = new PropertyMapping("CatchmentType") { IsRequired = false, IsUnique = false };
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingCatchmentType);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);

            base.FeatureFromGisImporterSettings.FeatureType = "Catchments";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
        }

        public override string Name
        {
            get { return "Catchment from GIS importer"; }
        }

        public override object ImportItem(string path, object target = null)
        {
            var features = GetFeatures();
            var drainageBasin = target as IDrainageBasin ?? DrainageBasin;

            ICoordinateTransformation coordinateTransformation = null;
            var sourceCoordinateSystem = GetCoordinateSystem();
            if (sourceCoordinateSystem != null)
            {
                if (drainageBasin.CoordinateSystem != null)
                {
                    coordinateTransformation =
                        new OgrCoordinateSystemFactory().CreateTransformation(sourceCoordinateSystem,
                            drainageBasin.CoordinateSystem);
                }
                else if (drainageBasin.Catchments.Count == 0)
                {
                    drainageBasin.CoordinateSystem = sourceCoordinateSystem;
                }
            }

            CatchmentToGisFeatureMapping.Clear();

            var nameColumnName = propertyMappingName.MappingColumn.Alias;

            var orderedFeatures = features.OrderBy(f => f.Attributes[nameColumnName].ToString());

            foreach (var feature in orderedFeatures)
            {
                if (coordinateTransformation != null)
                {
                    feature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry,coordinateTransformation.MathTransform);
                }

                ImportCatchment(feature, drainageBasin, feature.Attributes[nameColumnName].ToString(), propertyMappingLongName.MappingColumn.Alias, propertyMappingDescription.MappingColumn.Alias, propertyMappingCatchmentType.MappingColumn.Alias);
            }

            return drainageBasin;
        }

        public readonly IDictionary<Catchment, IFeature> CatchmentToGisFeatureMapping = new Dictionary<Catchment, IFeature>();

        private void ImportCatchment(IFeature feature, IDrainageBasin drainageBasin, string catchmentName, string columnNameLongName, string columnNameDescription, string columnNameCatchmentType)
        {
            var catchment = drainageBasin.Catchments.FirstOrDefault(c => c.Name == catchmentName);

            if (catchment == null)
            {
                catchment = new Catchment {Name = catchmentName};
                drainageBasin.Catchments.Add(catchment);
            }

            if (!string.IsNullOrEmpty(columnNameCatchmentType) && feature.Attributes.ContainsKey(columnNameCatchmentType))
            {
                try
                {
                    catchment.CatchmentType = CatchmentType.LoadFromString(feature.Attributes[columnNameCatchmentType].ToString());
                }
                catch (ArgumentException)
                {
                    catchment.CatchmentType = CatchmentType.None;
                }
            }
            else if (!string.IsNullOrEmpty(columnNameCatchmentType))
            {
                catchment.CatchmentType = CatchmentType.None;
            }
            
            CatchmentToGisFeatureMapping.Add(catchment,feature);

            catchment.Geometry = (IGeometry) feature.Geometry.Clone();

            if (columnNameLongName != null)
            {
                catchment.LongName = feature.Attributes[columnNameLongName].ToString();
            }

            if (columnNameDescription != null)
            {
                catchment.Description = feature.Attributes[columnNameDescription].ToString();
            }
        }
    }
}