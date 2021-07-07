using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class BridgeFromGisImporter:NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BridgeFromGisImporter));
        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingLevel;
        private PropertyMapping propertyMappingWidth;
        private PropertyMapping propertyMappingHeight;
        private PropertyMapping propertyMappingLength;
        private PropertyMapping propertyMappingFrictionValue;

        public BridgeFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");
            propertyMappingLevel = new PropertyMapping("Bed level") { PropertyUnit = "m" };
            propertyMappingWidth = new PropertyMapping("Width") { PropertyUnit = "m" };
            propertyMappingHeight = new PropertyMapping("Height") {PropertyUnit = "m"};
            propertyMappingLength = new PropertyMapping("Length") {PropertyUnit = "m"};
            propertyMappingFrictionValue = new PropertyMapping("Roughness") { PropertyUnit = "Chezy (C) m^1/2*s^-1" };

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLevel);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingWidth);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingHeight);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLength);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingFrictionValue);

            base.FeatureFromGisImporterSettings.FeatureType = "Bridges (rectangle profile)";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
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
                propertyMappingLevel = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLevel.PropertyName);
                propertyMappingWidth = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingWidth.PropertyName);
                propertyMappingHeight = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingHeight.PropertyName);
                propertyMappingLength = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLength.PropertyName);
                propertyMappingFrictionValue = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingFrictionValue.PropertyName);
                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLevel.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingWidth.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingHeight.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLength.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingFrictionValue.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "Bridge from GIS importer"; }
        }

        // add invokerequired to prevent NotifyPropertyChanged listeners to throw exception on cross thread 
        // handling; see TOOLS-4520
        [InvokeRequired]
        public override object ImportItem(string path, object target = null)
        {
            if(SnappingTolerance == 0)
            {
                SnappingTolerance = 1;
            }

            var features = GetFeatures();

            foreach (IFeature feature in features)
            {
                ImportBridge(feature,
                           propertyMappingName.MappingColumn.Alias,
                           propertyMappingLongName.MappingColumn.Alias,
                           propertyMappingDescription.MappingColumn.Alias
                    );

            }
            return HydroNetwork;
        }

        private void ImportBridge(IFeature feature, string columnNameName, string columnNameLongName, string columnNameDescription)
        {
            IBridge bridge = AddOrUpdateBranchFeatureFromNetwork<IBridge>(feature, columnNameName, Bridge.CreateDefault);

            if (bridge == null)
            {
                log.ErrorFormat("Failed to import bridge \"{0}\".", feature);
                return;
            }

            bridge.BridgeType = BridgeType.Rectangle;
            bridge.FrictionType = BridgeFrictionType.Chezy;
            bridge.AllowNegativeFlow = true;
            bridge.AllowPositiveFlow = true;

            try
            {
                if (columnNameDescription != null)
                {
                    bridge.Description = feature.Attributes[columnNameDescription].ToString();
                }

                if (!propertyMappingLongName.MappingColumn.IsNullValue)
                {
                    bridge.LongName = feature.Attributes[propertyMappingLongName.MappingColumn.Alias].ToString();
                }
                if (!propertyMappingLevel.MappingColumn.IsNullValue)
                {
                    bridge.Shift = Convert.ToDouble(feature.Attributes[propertyMappingLevel.MappingColumn.Alias]);
                }
                if(!propertyMappingWidth.MappingColumn.IsNullValue)
                {
                    bridge.Width = Convert.ToDouble(feature.Attributes[propertyMappingWidth.MappingColumn.Alias]);
                }
                if(!propertyMappingHeight.MappingColumn.IsNullValue)
                {
                    bridge.Height = Convert.ToDouble(feature.Attributes[propertyMappingHeight.MappingColumn.Alias]);
                }
                if(!propertyMappingLength.MappingColumn.IsNullValue)
                {
                    bridge.Length = Convert.ToDouble(feature.Attributes[propertyMappingLength.MappingColumn.Alias]);
                }
                if (!propertyMappingFrictionValue.MappingColumn.IsNullValue)
                {
                    bridge.Friction = Convert.ToDouble(feature.Attributes[propertyMappingFrictionValue.MappingColumn.Alias]);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception ocurred during import of bridge \"{0}\": {1}", bridge.Name, e);
                // Resume regular control flow. 
            }
        }
    }
}

