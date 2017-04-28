using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class CrossSectionYZFromGisImporter: NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionYZFromGisImporter));
        
        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingShiftLevel;
        private PropertyMapping propertyMappingY;
        private PropertyMapping propertyMappingZ;

        public CrossSectionYZFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName", false, false);
            propertyMappingDescription = new PropertyMapping("Description", false, false);
            propertyMappingShiftLevel = new PropertyMapping("ShiftLevel", false, false);
            propertyMappingY = new PropertyMapping("Y'-values", false, false);
            propertyMappingZ = new PropertyMapping("Z-values", false, false);

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingShiftLevel);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingY);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingZ);

            base.FeatureFromGisImporterSettings.FeatureType = "Cross Sections Y'Z";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
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
                propertyMappingShiftLevel = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingShiftLevel.PropertyName);
                propertyMappingY = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingY.PropertyName);
                propertyMappingZ = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingZ.PropertyName);
                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingShiftLevel.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingY.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingZ.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "Y'Z cross-section from GIS importer"; }
        }

        public override object ImportItem(string path, object target = null)
        {
            IFeature previousFeature = null;
            string previousName = null;
            ICrossSectionDefinition definition = null;
            var yzCoordinates = new List<Coordinate>();
            var features = GetFeatures();
            foreach (var feature in features)
            {
                var name = feature.Attributes[propertyMappingName.MappingColumn.Alias].ToString();
                
                if (!Equals(name, previousName)) //new profile
                {
                    try
                    {
                        if (definition is CrossSectionDefinitionYZ && previousFeature != null) // finish previous definition
                        {
                            double shiftLevel = 0;
                            try
                            {
                               shiftLevel = Convert.ToDouble(previousFeature.Attributes[propertyMappingShiftLevel.MappingColumn.Alias]);
                            }
                            catch (Exception e)
                            {
                                log.Warn("Shift level readout failed: ", e);
                            }
                            if (yzCoordinates.Any())
                            {
                                SetYZValues((CrossSectionDefinitionYZ) definition, yzCoordinates, shiftLevel);
                                yzCoordinates.Clear();
                            }
                        }

                        var crossSection = AddOrUpdateCrossSectionFromHydroNetwork(feature, propertyMappingName.MappingColumn.Alias, CrossSectionType.YZ);
                        
                        definition = crossSection.Definition;
                    }
                    catch (Exception e)
                    {
                        log.Warn("Shape file cross section import was skipped: ", e);
                        previousName = name;
                        previousFeature = feature;
                        continue;
                    }
                }
                try
                {
                    var yValue = Convert.ToDouble(feature.Attributes[propertyMappingY.MappingColumn.Alias]);
                    var zValue = Convert.ToDouble(feature.Attributes[propertyMappingZ.MappingColumn.Alias]);
                    if (!yzCoordinates.Select(c => c.X).Contains(yValue))
                    {
                        yzCoordinates.Add(new Coordinate(yValue, zValue));
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Cross section profile import was skipped: " + e.Message + " -- continuing with default profile.");
                    previousName = name;
                    previousFeature = feature;
                    continue;
                }             
                previousName = name;
                previousFeature = feature;
            }

            if (definition is CrossSectionDefinitionYZ && previousFeature != null) // finish previous definition
            {
                double shiftLevel = 0;
                try
                {
                    shiftLevel = Convert.ToDouble(previousFeature.Attributes[propertyMappingShiftLevel.MappingColumn.Alias]);
                }
                catch (Exception e)
                {
                    log.Warn("Shift level readout failed: ", e);
                }
                if (yzCoordinates.Any())
                {
                    SetYZValues((CrossSectionDefinitionYZ) definition, yzCoordinates, shiftLevel);
                }
            }

            return HydroNetwork;
        }

        private static void SetYZValues(CrossSectionDefinitionYZ crossSectionDefinition, List<Coordinate> YZCoordinates, double shiftLevel)
        {
            crossSectionDefinition.BeginEdit(new DefaultEditAction("Set YZ values"));

            crossSectionDefinition.YZDataTable.Clear();

            YZCoordinates = YZCoordinates.OrderBy(c => c.X).ToList();

            foreach (var coordinate in YZCoordinates)
            {    
                crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(coordinate.X, coordinate.Y + shiftLevel, 0);
            }

            crossSectionDefinition.EndEdit();
        }
    }
}
