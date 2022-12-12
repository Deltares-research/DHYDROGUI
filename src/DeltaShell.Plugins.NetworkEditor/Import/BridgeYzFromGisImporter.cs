using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Class for the YZ bridge from Gis importer.
    /// </summary>
    public class BridgeYzFromGisImporter : BridgeFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BridgeYzFromGisImporter));
        private readonly YzFromGisImporter yzFromGisImporter;

        public BridgeYzFromGisImporter()
        {
            yzFromGisImporter = new YzFromGisImporter();
            base.FeatureFromGisImporterSettings.FeatureType = "Bridges (YZ profile)";
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.YValues);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.ZValues);
        }

        public override string Name => "Bridge YZ from GIS importer";

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingYValues.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingZValues.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        protected override BridgeType BridgeType => BridgeType.YzProfile;

        protected override void ConvertBridgeProperties(IFeature feature, string columnNameDescription, IBridge bridge)
        {
            try
            {
                base.ConvertBridgeProperties(feature, columnNameDescription, bridge);
                ConvertBridgePropertiesYz(feature, bridge);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception ocurred during import of bridge \"{0}\": {1}", bridge.Name, e);
            }
        }

        private PropertyMapping PropertyMappingYValues => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.YValues.PropertyName);
        private PropertyMapping PropertyMappingZValues => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.ZValues.PropertyName);

        private void ConvertBridgePropertiesYz(IFeature feature, IBridge bridge)
        {
            IList<double> yValues = yzFromGisImporter.ConvertPropertyMappingToList(feature.Attributes[PropertyMappingYValues.MappingColumn.Alias].ToString());
            IList<double> zValues = yzFromGisImporter.ConvertPropertyMappingToList(feature.Attributes[PropertyMappingZValues.MappingColumn.Alias].ToString());
            yzFromGisImporter.ConvertYzProperties(bridge.YZCrossSectionDefinition, yValues, zValues);
        }
    }
}