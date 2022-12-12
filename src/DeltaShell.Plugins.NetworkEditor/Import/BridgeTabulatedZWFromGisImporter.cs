using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
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
            base.FeatureFromGisImporterSettings.FeatureType = "Bridges (ZW profile)";

            var propertyMapping = new Dictionary<string, string> { { BridgeDefaultPropertyMappings.Width.PropertyName, BridgeDefaultPropertyMappings.Width.PropertyUnit } };

            zwFromGisImporter = new ZwFromGisImporter(propertyMapping);
            zwFromGisImporter.MakeNumberOfLevelPropertiesMapping(standardLevels, base.FeatureFromGisImporterSettings.PropertiesMapping);
        }

        public override string Name => "Bridge ZW from GIS importer";

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
                !PropertyMappingLevelsExistInSettings(featureFromGisImporterSettings, BridgeDefaultPropertyMappings.Width.PropertyName))
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
                log.ErrorFormat("Exception ocurred during import of bridge \"{0}\": {1}", bridge.Name, e);
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
                PropertyMapping propertyLevel = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == ZwFromGisImporter.LblLevel + i);
                PropertyMapping propertyFlowWidth = FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == $"{BridgeDefaultPropertyMappings.Width.PropertyName} {i}");

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