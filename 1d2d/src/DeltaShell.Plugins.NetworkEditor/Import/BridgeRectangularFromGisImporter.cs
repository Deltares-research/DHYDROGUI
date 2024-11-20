using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Properties;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Class for the rectangular bridge from Gis importer.
    /// </summary>
    public class BridgeRectangularFromGisImporter : BridgeFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BridgeRectangularFromGisImporter));

        public BridgeRectangularFromGisImporter()
        {
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.Height);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.Width);
            base.FeatureFromGisImporterSettings.FeatureType = Resources.BridgeRectangularFromGisImporter_BridgeRectangularFromGisImporter_Bridges__rectangle_profile_;
        }

        public override string Name => Resources.BridgeRectangularFromGisImporter_Name_Bridge_from_GIS_importer;

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingHeight.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingWidth.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        protected override BridgeType BridgeType => BridgeType.Rectangle;

        protected override void ConvertBridgeProperties(IFeature feature, string columnNameDescription, IBridge bridge)
        {
            try
            {
                base.ConvertBridgeProperties(feature, columnNameDescription, bridge);

                if (!PropertyMappingWidth.MappingColumn.IsNullValue)
                {
                    bridge.Width = Convert.ToDouble(feature.Attributes[PropertyMappingWidth.MappingColumn.Alias]);
                }

                if (!PropertyMappingHeight.MappingColumn.IsNullValue)
                {
                    bridge.Height = Convert.ToDouble(feature.Attributes[PropertyMappingHeight.MappingColumn.Alias]);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.BridgeRectangularFromGisImporter_ConvertBridgeProperties_Exception_ocurred_during_import_of_bridge___0_____1_, bridge.Name, e);
            }
        }

        private PropertyMapping PropertyMappingWidth => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(BridgeDefaultGisPropertyMappings.Width.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingHeight => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(BridgeDefaultGisPropertyMappings.Height.PropertyName, StringComparison.InvariantCulture));
    }
}