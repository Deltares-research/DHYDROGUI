using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class CrossSectionZWFromGisImporter: NetworkFeatureFromGisImporterBase, INetworkFeatureZwFromGisImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionZWFromGisImporter));

        private int numberOfLevels;

        private const string lblLevel = "Level ";
        private const string lblFlowWidth = "Flow width ";
        private const string lblStorageWidth = "Storage width ";

        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingShiftLevel;

        public CrossSectionZWFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName", false, false);
            propertyMappingDescription = new PropertyMapping("Description", false, false);
            propertyMappingShiftLevel = new PropertyMapping("ShiftLevel", false, false);

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingShiftLevel);

            base.FeatureFromGisImporterSettings.FeatureType = "Cross Sections ZW";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();

            MakeNumberOfLevelPropertiesMapping(3);
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

                numberOfLevels = value.PropertiesMapping.Count(pm => pm.PropertyName.StartsWith(lblLevel));

                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName)||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingShiftLevel.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public int NumberOfLevels
        {
            get
            {
                return numberOfLevels;
            }
            set
            {
                MakeNumberOfLevelPropertiesMapping(value);
            }
        }

        public override string Name
        {
            get { return "Tabulated river cross-section from GIS importer"; }
        }

        public override object ImportItem(string path, object target = null)
        {
            var features = GetFeatures();

            foreach (var feature in features)
            {
                try
                {
                    ImportCrossSection(feature);
                }
                catch (Exception e)
                {
                    log.Warn("Import of cross section was skipped", e);
                }
            }
            return HydroNetwork;
        }

        private void ImportCrossSection(IFeature feature)
        {
            var crossSection = AddOrUpdateCrossSectionFromHydroNetwork(feature, propertyMappingName.MappingColumn.Alias,
                                                                       CrossSectionType.ZW);

            if (crossSection == null) return;

            

            var hfswData = new List<HeightFlowStorageWidth>();
            
            for (var i = 1 ; i <= numberOfLevels; i++)
            {
                var propertyLevel = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == lblLevel + i);
                var propertyFlowWidth = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == lblFlowWidth + i);
                var propertyStorageWidth = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == lblStorageWidth + i);

                var level = Convert.ToDouble(feature.Attributes[propertyLevel.MappingColumn.Alias]);
                var flowWidth = Convert.ToDouble(feature.Attributes[propertyFlowWidth.MappingColumn.Alias]);
                var storageWidth = Convert.ToDouble(feature.Attributes[propertyStorageWidth.MappingColumn.Alias]);

                while (hfswData.Any(data => data.Height == level))
                {
                    level += 0.05;
                }
                hfswData.Add(new HeightFlowStorageWidth(level, flowWidth + storageWidth, flowWidth));
            }

            var zwDefinition = (CrossSectionDefinitionZW)crossSection.Definition;
            zwDefinition.SetWithHfswData(hfswData);

            crossSection.Geometry = CrossSectionHelper.CreatePerpendicularGeometry(crossSection.Branch.Geometry,
                            crossSection.Chainage, crossSection.Definition.Width);
            
            //width should return 0 if no data for this to work as before
            var halfMaxWidth = crossSection.Definition.Width/2;
            var sectionType = HydroNetwork.CrossSectionSectionTypes.FirstOrDefault();
            if (!crossSection.Definition.IsProxy && sectionType != null)
            {
                crossSection.Definition.Sections.Clear();
                crossSection.Definition.Sections.Add(new CrossSectionSection
                                                         {
                                                             MinY = -halfMaxWidth,
                                                             MaxY = halfMaxWidth,
                                                             SectionType = sectionType
                                                         });
                crossSection.Definition.Thalweg = 0;
            }

            var shiftLevelKey = propertyMappingShiftLevel.MappingColumn.Alias;
            if (shiftLevelKey != null)
            {
                var shiftLevel = Convert.ToDouble(feature.Attributes[shiftLevelKey]);
                crossSection.Definition.ShiftLevel(shiftLevel);
            }
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

        private void MakeNumberOfLevelPropertiesMapping(int levels)
        {
            PropertyMapping propertyMapping;
            for(var i = numberOfLevels; i > levels; i--)
            {
                //level
                propertyMapping = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == lblLevel + i);
                FeatureFromGisImporterSettings.PropertiesMapping.Remove(propertyMapping);

                //flow width
                propertyMapping = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == lblFlowWidth + i);
                FeatureFromGisImporterSettings.PropertiesMapping.Remove(propertyMapping);

                //storage width
                propertyMapping = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == lblStorageWidth + i);
                FeatureFromGisImporterSettings.PropertiesMapping.Remove(propertyMapping);
            }
            for (var i = numberOfLevels ; i < levels; i++)
            {
                var j = i + 1;

                //level
                propertyMapping = new PropertyMapping(lblLevel + j, false, true) {PropertyUnit = "m"};
                FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMapping);

                //flow width
                propertyMapping = new PropertyMapping(lblFlowWidth + j, false, true) {PropertyUnit = "m"};
                FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMapping);

                //storage width
                propertyMapping = new PropertyMapping(lblStorageWidth + j, false, true) {PropertyUnit = "m"};
                FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMapping);
            }

            numberOfLevels = levels;
        }
    }
}
