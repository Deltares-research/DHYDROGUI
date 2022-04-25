using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Utils;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class CatchmentFromGisImporter : FeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CatchmentFromGisImporter));

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
            var sourceCoordinateSystem = GetCoordinateSystem();

            var drainageBasin = target as IDrainageBasin ?? GetDrainageBasin();
            if (drainageBasin == null)
            {
                log.Error("Could not find drainage basin to import on.");
                return null;
            }

            if (sourceCoordinateSystem != null && drainageBasin.Catchments.Count == 0)
            {
                drainageBasin.CoordinateSystem = sourceCoordinateSystem;
            }

            var coordinateTransformation = GetCoordinateTransformation(sourceCoordinateSystem, drainageBasin);
            
            var catchmentLookup = drainageBasin.Catchments
                                               .ToDictionaryWithErrorDetails(drainageBasin.Name, catchment => catchment.Name);
            
            features.ForEach(feature => ImportCatchment(feature, drainageBasin, catchmentLookup, coordinateTransformation));

            return drainageBasin;
        }

        private static ICoordinateTransformation GetCoordinateTransformation(ICoordinateSystem sourceCoordinateSystem, IDrainageBasin drainageBasin)
        {
            if (sourceCoordinateSystem == null 
                || drainageBasin.CoordinateSystem == null 
                || sourceCoordinateSystem == drainageBasin.CoordinateSystem)
            {
                return null;
            }

            return new OgrCoordinateSystemFactory().CreateTransformation(sourceCoordinateSystem, drainageBasin.CoordinateSystem);
        }

        private void ImportCatchment(IFeature feature, IDrainageBasin drainageBasin, IDictionary<string, Catchment> catchmentLookup, ICoordinateTransformation coordinateTransformation)
        {
            var catchmentName = GetAttributeValue(propertyMappingName, feature);
            if (!IsFeatureValid(feature, catchmentName))
            {
                return;
            }

            var isExistingCatchment = catchmentLookup.TryGetValue(catchmentName, out Catchment catchment);

            if (!isExistingCatchment)
            {
                catchment = new Catchment { Name = catchmentName };
            }

            catchment.Geometry = coordinateTransformation != null
                                     ? GeometryTransform.TransformGeometry(feature.Geometry, coordinateTransformation.MathTransform)
                                     : (IGeometry)feature.Geometry.Clone();

            catchment.LongName = GetAttributeValue(propertyMappingLongName, feature) ?? catchment.LongName;
            catchment.Description = GetAttributeValue(propertyMappingDescription, feature) ?? catchment.Description;

            var catchmentTypeText = GetAttributeValue(propertyMappingCatchmentType, feature);
            if (catchmentTypeText != null)
            {
                var catchmentType = GetCatchmentType(catchmentTypeText, catchmentName);
                if (!Equals(catchmentType, catchment.CatchmentType))
                {
                    catchment.CatchmentType = catchmentType;
                }
            }

            if (!isExistingCatchment)
            {
                drainageBasin.Catchments.Add(catchment);
                catchmentLookup.Add(catchmentName, catchment);
            }
        }

        private static bool IsFeatureValid(IFeature feature, string catchmentName)
        {
            if (catchmentName == null)
            {
                log.Error("Could not import feature catchment name is not set.");
                return false;
            }

            if (feature.Geometry == null)
            {
                log.Error($"Could not import feature {catchmentName}, geometry is missing");
                return false;
            }

            return true;
        }

        private static CatchmentType GetCatchmentType(string catchmentTypeText, string catchmentName)
        {
            try
            {
                return CatchmentType.LoadFromString(catchmentTypeText);
            }
            catch (ArgumentException)
            {
                log.Error($"Could not convert \"{catchmentTypeText}\" to a catchment type for catchment {catchmentName}. Reverting to default catchment type None");
                return CatchmentType.None;
            }
        }

        private static string GetAttributeValue(PropertyMapping propertyMapping, IFeature feature)
        {
            // get mapped attribute name
            var attributeName = propertyMapping?.MappingColumn?.Alias;

            if (string.IsNullOrEmpty(attributeName) 
                || !feature.Attributes.TryGetValue(attributeName, out var attributeObject)) return null;

            return attributeObject.ToString();
        }

        private IDrainageBasin GetDrainageBasin()
        {
            return HydroRegion as IDrainageBasin ?? HydroRegion.SubRegions
                                                               .OfType<IDrainageBasin>()
                                                               .FirstOrDefault();
        }

    }
}