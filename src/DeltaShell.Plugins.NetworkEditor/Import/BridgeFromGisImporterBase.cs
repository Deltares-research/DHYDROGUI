using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
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
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.Name);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.LongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.Description);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.Level);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.Length);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(BridgeDefaultPropertyMappings.FrictionValue);
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

        private PropertyMapping PropertyMappingName => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.Name.PropertyName);
        private PropertyMapping PropertyMappingLongName => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.LongName.PropertyName);
        private PropertyMapping PropertyMappingDescription => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.Description.PropertyName);
        private PropertyMapping PropertyMappingLevel => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.Level.PropertyName);
        private PropertyMapping PropertyMappingLength => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.Length.PropertyName);
        private PropertyMapping PropertyMappingFrictionValue => FeatureFromGisImporterSettings.PropertiesMapping.First(pm => pm.PropertyName == BridgeDefaultPropertyMappings.FrictionValue.PropertyName);

        private void ImportBridge(IFeature feature, string columnNameName, string columnNameDescription)
        {
            var bridge = AddOrUpdateBranchFeatureFromNetwork<IBridge>(feature, columnNameName, Bridge.CreateDefault);

            if (bridge == null)
            {
                log.ErrorFormat("Failed to import bridge \"{0}\".", feature);
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
                log.ErrorFormat("Exception ocurred during import of bridge \"{0}\": {1}", bridge.Name, e);
                // Resume regular control flow. 
            }
        }
    }
}