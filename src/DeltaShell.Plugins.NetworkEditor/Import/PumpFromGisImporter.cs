using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class PumpFromGisImporter:NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PumpFromGisImporter));
        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingCapacity;
        private PropertyMapping propertyMappingStartDelivery;
        private PropertyMapping propertyMappingStopDelivery;
        private PropertyMapping propertyMappingStartSuction;
        private PropertyMapping propertyMappingStopSuction;


        public PumpFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");
            propertyMappingCapacity = new PropertyMapping("Capacity");
            propertyMappingStartDelivery = new PropertyMapping("Delivery start");
            propertyMappingStopDelivery = new PropertyMapping("Delivery stop");
            propertyMappingStartSuction = new PropertyMapping("Suction start");
            propertyMappingStopSuction = new PropertyMapping("Suction stop");

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingCapacity);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingStartDelivery);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingStopDelivery);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingStartSuction);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingStopSuction);

            base.FeatureFromGisImporterSettings.FeatureType = "Pumps";
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
                propertyMappingName = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingName.PropertyName).First();
                propertyMappingLongName = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingLongName.PropertyName).First();
                propertyMappingDescription = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingDescription.PropertyName).First();
                propertyMappingCapacity = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingCapacity.PropertyName).First();
                propertyMappingStartDelivery = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingStartDelivery.PropertyName).First();
                propertyMappingStopDelivery = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingStopDelivery.PropertyName).First();
                propertyMappingStartSuction = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingStartSuction.PropertyName).First();
                propertyMappingStopSuction = value.PropertiesMapping.Where(pm => pm.PropertyName == propertyMappingStopSuction.PropertyName).First();
                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingCapacity.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingStartDelivery.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingStopDelivery.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingStartSuction.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingStopSuction.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "Pump from GIS importer"; }
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
                ImportPump(feature,
                           propertyMappingName.MappingColumn.Alias,
                           propertyMappingLongName.MappingColumn.Alias,
                           propertyMappingDescription.MappingColumn.Alias,
                           propertyMappingCapacity.MappingColumn.Alias,
                           propertyMappingStartDelivery.MappingColumn.Alias,
                           propertyMappingStopDelivery.MappingColumn.Alias,
                           propertyMappingStartSuction.MappingColumn.Alias,
                           propertyMappingStopSuction.MappingColumn.Alias
                    );

            }
            return HydroNetwork;
        }

        private void ImportPump(IFeature feature, string columnNameName, string columnNameLongName, string columnNameDescription, string columnNameCapacity, string columnNameStartDelivery, string columnNameStopDelivery, string columnNameStartSuction, string columnNameStopSuction)
        {
            var pump = AddOrUpdateBranchFeatureFromNetwork(feature, columnNameName, branch =>
            {
                var branchFeature = new Pump(true);
                BranchStructure.AddStructureToNetwork(branchFeature, branch);
                return branchFeature;
            });

            if (pump == null)
            {
                log.ErrorFormat("Failed to import pump \"{0}\".", columnNameName);
                return;
            }

            if (columnNameLongName != null)
            {
                pump.LongName = feature.Attributes[columnNameLongName].ToString();
            }
            if (columnNameDescription != null)
            {
                pump.Description = feature.Attributes[columnNameDescription].ToString();
            }
            if(columnNameCapacity != null)
            {
                pump.Capacity = Convert.ToDouble(feature.Attributes[columnNameCapacity]);
            }
            if(columnNameStartDelivery != null)
            {
                pump.StartDelivery = Convert.ToDouble(feature.Attributes[columnNameStartDelivery]);
            }
            if(columnNameStopDelivery != null)
            {
                pump.StopDelivery = Convert.ToDouble(feature.Attributes[columnNameStopDelivery]);
            }
            if(columnNameStartSuction != null)
            {
                pump.StartSuction = Convert.ToDouble(feature.Attributes[columnNameStartSuction]);
            }
            if (columnNameStopSuction != null)
            {
                pump.StopSuction = Convert.ToDouble(feature.Attributes[columnNameStopSuction]);
            }
        }
    }
}
