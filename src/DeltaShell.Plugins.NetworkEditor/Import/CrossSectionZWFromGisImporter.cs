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
        private const int standardLevels = 3;
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionZWFromGisImporter));

        private readonly ZwFromGisImporter zwFromGisImporter;

        public CrossSectionZWFromGisImporter()
        {
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(CrossSectionDefaultPropertyMappings.Name);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(CrossSectionDefaultPropertyMappings.LongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(CrossSectionDefaultPropertyMappings.Description);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(CrossSectionDefaultPropertyMappings.ShiftLevel);

            base.FeatureFromGisImporterSettings.FeatureType = "Cross Sections ZW";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();

            var propertyMapping = new Dictionary<string, string>
            {
                { CrossSectionDefaultPropertyMappings.FlowWidth.PropertyName, CrossSectionDefaultPropertyMappings.FlowWidth.PropertyUnit },
                { CrossSectionDefaultPropertyMappings.StorageWidth.PropertyName, CrossSectionDefaultPropertyMappings.StorageWidth.PropertyUnit }
            };

            zwFromGisImporter = new ZwFromGisImporter(propertyMapping);
            zwFromGisImporter.MakeNumberOfLevelPropertiesMapping(standardLevels, base.FeatureFromGisImporterSettings.PropertiesMapping);
        }

        public override string Name => "Tabulated river cross-section from GIS importer";

        public int NumberOfLevels
        {
            get
            {
                zwFromGisImporter.UpdateNumberOfLevels(base.FeatureFromGisImporterSettings.PropertiesMapping);
                return zwFromGisImporter.NumberOfLevels;
            }
            set => zwFromGisImporter.MakeNumberOfLevelPropertiesMapping(value, base.FeatureFromGisImporterSettings.PropertiesMapping);
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingShiftLevel.PropertyName) ||
                !PropertyMappingLevelsExistInSettings(featureFromGisImporterSettings, ZwFromGisImporter.LblLevel) ||
                !PropertyMappingLevelsExistInSettings(featureFromGisImporterSettings, CrossSectionDefaultPropertyMappings.FlowWidth.PropertyName) ||
                !PropertyMappingLevelsExistInSettings(featureFromGisImporterSettings, CrossSectionDefaultPropertyMappings.StorageWidth.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override object ImportItem(string path, object target = null)
        {
            IList<IFeature> features = GetFeatures();

            foreach (IFeature feature in features)
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

        private PropertyMapping PropertyMappingName => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == CrossSectionDefaultPropertyMappings.Name.PropertyName);
        private PropertyMapping PropertyMappingLongName => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == CrossSectionDefaultPropertyMappings.LongName.PropertyName);
        private PropertyMapping PropertyMappingDescription => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == CrossSectionDefaultPropertyMappings.Description.PropertyName);
        private PropertyMapping PropertyMappingShiftLevel => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == CrossSectionDefaultPropertyMappings.ShiftLevel.PropertyName);

        private bool PropertyMappingLevelsExistInSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings, string propertyName)
        {
            for (var i = 1; i <= zwFromGisImporter.NumberOfLevels; i++)
            {
                var expectedPropertyName = $"{propertyName.Trim()} {i}";
                if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, expectedPropertyName))
                {
                    return false;
                }
            }

            return true;
        }

        private void ImportCrossSection(IFeature feature)
        {
            ICrossSection crossSection = AddOrUpdateCrossSectionFromHydroNetwork(feature, PropertyMappingName.MappingColumn.Alias,
                                                                                 CrossSectionType.ZW);
            if (crossSection == null)
            {
                return;
            }

            if (crossSection.Definition is CrossSectionDefinitionZW zwDefinition)
            {
                zwDefinition.SetWithHfswData(ConvertAndGetHeightFlowStorageWidths(feature));
            }

            crossSection.Geometry = CrossSectionHelper.CreatePerpendicularGeometry(crossSection.Branch.Geometry,
                                                                                   crossSection.Chainage, crossSection.Definition.Width);

            SetSectionsAndThalweg(crossSection.Definition);

            string shiftLevelKey = PropertyMappingShiftLevel.MappingColumn.Alias;
            if (shiftLevelKey != null)
            {
                var shiftLevel = Convert.ToDouble(feature.Attributes[shiftLevelKey]);
                crossSection.Definition.ShiftLevel(shiftLevel);
            }

            string longNameKey = PropertyMappingLongName.MappingColumn.Alias;
            if (longNameKey != null)
            {
                crossSection.LongName = feature.Attributes[longNameKey].ToString();
            }

            string descriptionKey = PropertyMappingDescription.MappingColumn.Alias;
            if (descriptionKey != null)
            {
                crossSection.Description = feature.Attributes[descriptionKey] + "(" + crossSection.Name + ")";
            }
        }

        /// <summary>
        /// Set Sections and Thalweg of the given definition.
        /// </summary>
        /// <remarks>width should return 0 if no data for this to work as before.</remarks>
        /// <param name="definition">Cross Section Definition to be updated.</param>
        private void SetSectionsAndThalweg(ICrossSectionDefinition definition)
        {
            double halfMaxWidth = definition.Width / 2;
            CrossSectionSectionType sectionType = HydroNetwork.CrossSectionSectionTypes.FirstOrDefault();
            if (!definition.IsProxy && sectionType != null)
            {
                definition.Sections.Clear();
                definition.Sections.Add(new CrossSectionSection
                {
                    MinY = -halfMaxWidth,
                    MaxY = halfMaxWidth,
                    SectionType = sectionType
                });
                definition.Thalweg = 0;
            }
        }

        private List<HeightFlowStorageWidth> ConvertAndGetHeightFlowStorageWidths(IFeature feature)
        {
            var hfswData = new List<HeightFlowStorageWidth>();

            for (var i = 1; i <= zwFromGisImporter.NumberOfLevels; i++)
            {
                PropertyMapping propertyLevel = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == ZwFromGisImporter.LblLevel + i);
                PropertyMapping propertyFlowWidth = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == $"{CrossSectionDefaultPropertyMappings.FlowWidth.PropertyName} {i}");
                PropertyMapping propertyStorageWidth = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == $"{CrossSectionDefaultPropertyMappings.StorageWidth.PropertyName} {i}");

                var level = Convert.ToDouble(feature.Attributes[propertyLevel.MappingColumn.Alias]);
                var flowWidth = Convert.ToDouble(feature.Attributes[propertyFlowWidth.MappingColumn.Alias]);
                var storageWidth = Convert.ToDouble(feature.Attributes[propertyStorageWidth.MappingColumn.Alias]);

                while (hfswData.Any(data => data.Height == level))
                {
                    level += 0.05;
                }

                hfswData.Add(new HeightFlowStorageWidth(level, flowWidth + storageWidth, flowWidth));
            }

            return hfswData;
        }
    }
}