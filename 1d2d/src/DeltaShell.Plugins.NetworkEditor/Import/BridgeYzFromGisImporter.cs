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
    /// Class for the YZ bridge from Gis importer.
    /// </summary>
    public class BridgeYzFromGisImporter : BridgeFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BridgeYzFromGisImporter));
        private readonly YzFromGisImporter yzFromGisImporter;

        public BridgeYzFromGisImporter()
        {
            yzFromGisImporter = new YzFromGisImporter();
            base.FeatureFromGisImporterSettings.FeatureType = Resources.BridgeYzFromGisImporter_BridgeYzFromGisImporter_Bridges__YZ_profile_;
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.YValues);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.ZValues);
        }

        public override string Name => Resources.BridgeYzFromGisImporter_Name_Bridge_YZ_from_GIS_importer;

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
                log.ErrorFormat(Resources.BridgeYzFromGisImporter_ConvertBridgeProperties_Exception_ocurred_during_import_of_bridge___0_____1_, bridge.Name, e);
            }
        }

        private PropertyMapping PropertyMappingYValues => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(BridgeDefaultGisPropertyMappings.YValues.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingZValues => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(BridgeDefaultGisPropertyMappings.ZValues.PropertyName, StringComparison.InvariantCulture));

        private void ConvertBridgePropertiesYz(IFeature feature, IBridge bridge)
        {
            IList<double> yValues = yzFromGisImporter.ConvertPropertyMappingToList(feature.Attributes[PropertyMappingYValues.MappingColumn.Alias].ToString());
            IList<double> zValues = yzFromGisImporter.ConvertPropertyMappingToList(feature.Attributes[PropertyMappingZValues.MappingColumn.Alias].ToString());
            bridge.YZCrossSectionDefinition.SetYzValues(yValues, zValues);
        }
    }
}