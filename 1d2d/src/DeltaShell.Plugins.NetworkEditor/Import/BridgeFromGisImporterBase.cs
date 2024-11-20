using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Properties;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Base class for the bridge from Gis importer.
    /// </summary>
    public abstract class BridgeFromGisImporterBase : NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BridgeFromGisImporterBase));

        protected BridgeFromGisImporterBase()
        {
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.Name);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.LongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.Description);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.Level);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.Length);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultGisPropertyMappings.FrictionValue);
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingLevel.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingLength.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingFrictionValue.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        // add invokerequired to prevent NotifyPropertyChanged listeners to throw exception on cross thread 
        // handling; see TOOLS-4520
        [InvokeRequired]
        public override object ImportItem(string path, object target = null)
        {
            if (SnappingTolerance == 0)
            {
                SnappingTolerance = 1;
            }

            IList<IFeature> features = GetFeatures();

            foreach (IFeature feature in features)
            {
                ImportBridge(feature,
                             PropertyMappingName.MappingColumn.Alias,
                             PropertyMappingDescription.MappingColumn.Alias
                );
            }

            return HydroNetwork;
        }

        protected abstract BridgeType BridgeType { get; }

        protected virtual void ConvertBridgeProperties(IFeature feature, string columnNameDescription, IBridge bridge)
        {
            if (columnNameDescription != null)
            {
                bridge.Description = feature.Attributes[columnNameDescription].ToString();
            }

            if (!PropertyMappingLongName.MappingColumn.IsNullValue)
            {
                bridge.LongName = feature.Attributes[PropertyMappingLongName.MappingColumn.Alias].ToString();
            }

            if (!PropertyMappingLevel.MappingColumn.IsNullValue)
            {
                bridge.Shift = Convert.ToDouble(feature.Attributes[PropertyMappingLevel.MappingColumn.Alias]);
            }

            if (!PropertyMappingLength.MappingColumn.IsNullValue)
            {
                bridge.Length = Convert.ToDouble(feature.Attributes[PropertyMappingLength.MappingColumn.Alias]);
            }

            if (!PropertyMappingFrictionValue.MappingColumn.IsNullValue)
            {
                bridge.Friction = Convert.ToDouble(feature.Attributes[PropertyMappingFrictionValue.MappingColumn.Alias]);
            }
        }

        private PropertyMapping PropertyMappingName => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals( BridgeDefaultGisPropertyMappings.Name.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingLongName => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals( BridgeDefaultGisPropertyMappings.LongName.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingDescription => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals( BridgeDefaultGisPropertyMappings.Description.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingLevel => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals( BridgeDefaultGisPropertyMappings.Level.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingLength => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals( BridgeDefaultGisPropertyMappings.Length.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingFrictionValue => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals( BridgeDefaultGisPropertyMappings.FrictionValue.PropertyName, StringComparison.InvariantCulture));

        private void ImportBridge(IFeature feature, string columnNameName, string columnNameDescription)
        {
            var bridge = AddOrUpdateBranchFeatureFromNetwork<IBridge>(feature, columnNameName, Bridge.CreateDefault);

            if (bridge == null)
            {
                log.ErrorFormat(Resources.BridgeFromGisImporterBase_ImportBridge_Failed_to_import_bridge___0___, feature);
                return;
            }

            bridge.BridgeType = BridgeType;
            bridge.FrictionType = BridgeFrictionType.Chezy;
            bridge.AllowNegativeFlow = true;
            bridge.AllowPositiveFlow = true;

            try
            {
                ConvertBridgeProperties(feature, columnNameDescription, bridge);
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.BridgeFromGisImporterBase_ImportBridge_Exception_ocurred_during_import_of_bridge___0_____1_, bridge.Name, e);
                // Resume regular control flow. 
            }
        }
    }
}