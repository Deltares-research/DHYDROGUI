using System;
using System.Linq;
using DelftTools.Hydro.Structures;
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
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.Height);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.Width);
            base.FeatureFromGisImporterSettings.FeatureType = "Bridges (rectangle profile)";
        }

        public override string Name => "Bridge from GIS importer";

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
                log.ErrorFormat("Exception ocurred during import of bridge \"{0}\": {1}", bridge.Name, e);
            }
        }

        private PropertyMapping PropertyMappingWidth => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.Width.PropertyName);
        private PropertyMapping PropertyMappingHeight => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.Height.PropertyName);
    }
}