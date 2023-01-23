using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Properties;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Class for the ZW bridge from Gis importer.
    /// </summary>
    public class BridgeZwFromGisImporter : BridgeFromGisImporterBase, INetworkFeatureZwFromGisImporter
    {
        private const int standardLevels = 3;
        private static readonly ILog log = LogManager.GetLogger(typeof(BridgeZwFromGisImporter));
        private readonly ZwFromGisImporter zwFromGisImporter;

        public BridgeZwFromGisImporter()
        {
            base.FeatureFromGisImporterSettings.FeatureType = Resources.BridgeZwFromGisImporter_BridgeZwFromGisImporter_Bridges__ZW_profile_;

            var propertyMapping = new Dictionary<string, string> { { BridgeDefaultGisPropertyMappings.Width.PropertyName, BridgeDefaultGisPropertyMappings.Width.PropertyUnit } };

            zwFromGisImporter = new ZwFromGisImporter(propertyMapping);
            zwFromGisImporter.MakeNumberOfLevelPropertiesMapping(standardLevels, base.FeatureFromGisImporterSettings.PropertiesMapping);
        }

        public override string Name => Resources.BridgeZwFromGisImporter_Name_Bridge_ZW_from_GIS_importer;

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
            if (!PropertyMappingLevelsExistInSettings(featureFromGisImporterSettings, ZwFromGisImporter.LblLevel) ||
                !PropertyMappingLevelsExistInSettings(featureFromGisImporterSettings, BridgeDefaultGisPropertyMappings.Width.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        protected override BridgeType BridgeType => BridgeType.Tabulated;

        protected override void ConvertBridgeProperties(IFeature feature, string columnNameDescription, IBridge bridge)
        {
            try
            {
                base.ConvertBridgeProperties(feature, columnNameDescription, bridge);
                ConvertZwProperties(feature, bridge);
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.BridgeZwFromGisImporter_ConvertBridgeProperties_Exception_ocurred_during_import_of_bridge___0_____1_, bridge.Name, e);
            }
        }

        private bool PropertyMappingLevelsExistInSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings, string propertyName)
        {
            for (var i = 1; i <= zwFromGisImporter.NumberOfLevels; i++)
            {
                var expectedPropertyName = $"{propertyName} {i}";
                if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, expectedPropertyName))
                {
                    return false;
                }
            }

            return true;
        }

        private void ConvertZwProperties(IFeature feature, IBridge bridge)
        {
            var hfswData = new List<HeightFlowStorageWidth>();
            for (var i = 1; i <= zwFromGisImporter.NumberOfLevels; i++)
            {
                PropertyMapping propertyLevel = FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(p => p.PropertyName.Equals(ZwFromGisImporter.LblLevel + i, StringComparison.InvariantCulture));
                if (propertyLevel == null) continue;
                PropertyMapping propertyFlowWidth = FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(p => p.PropertyName.Equals($"{BridgeDefaultGisPropertyMappings.Width.PropertyName} {i}", StringComparison.InvariantCulture));
                if (propertyFlowWidth == null) continue;
                double level = Convert.ToDouble(feature.Attributes[propertyLevel.MappingColumn.Alias]);
                double width = Convert.ToDouble(feature.Attributes[propertyFlowWidth.MappingColumn.Alias]);

                while (hfswData.Any(data => data.Height.Equals(level)))
                {
                    level += 0.05;
                }

                hfswData.Add(new HeightFlowStorageWidth(level, width, width));
            }

            bridge.TabulatedCrossSectionDefinition.SetWithHfswData(hfswData);
        }
    }
}