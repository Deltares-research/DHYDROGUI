using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class SimpleWeirFromGisImporter: NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SimpleWeirFromGisImporter));

        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingCrestWidth;
        private PropertyMapping propertyMappingCrestLevel;
        private PropertyMapping propertyMappingDC;
        private PropertyMapping propertyMappingLC;

        public SimpleWeirFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");
            propertyMappingCrestWidth = new PropertyMapping("Crest Width"){PropertyUnit = "m"};
            propertyMappingCrestLevel = new PropertyMapping("Crest Level") { PropertyUnit = "m" };
            propertyMappingDC = new PropertyMapping("Discharge Coefficient");
            propertyMappingLC = new PropertyMapping("Lateral Contraction");

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingCrestWidth);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingCrestLevel);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDC);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLC);

            base.FeatureFromGisImporterSettings.FeatureType = "Weirs (Simple)";
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
                propertyMappingCrestWidth = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingCrestWidth.PropertyName);
                propertyMappingCrestLevel = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingCrestLevel.PropertyName);
                propertyMappingDC= value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingDC.PropertyName);
                propertyMappingLC = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLC.PropertyName);

                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingCrestWidth.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingCrestLevel.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDC.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLC.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "Simple Weir from GIS importer"; }
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
                ImportWeir(feature,
                            propertyMappingName.MappingColumn.Alias,
                            propertyMappingLongName.MappingColumn.Alias,
                            propertyMappingDescription.MappingColumn.Alias,
                            propertyMappingCrestWidth.MappingColumn.Alias,
                            propertyMappingCrestLevel.MappingColumn.Alias,
                            propertyMappingDC.MappingColumn.Alias,
                            propertyMappingLC.MappingColumn.Alias);
            }
            return HydroNetwork;
        }

        private void ImportWeir(IFeature feature, string columnNameName, string columnNameDescription, string columnNameLongName, string columnNameCrestWidth, string columnNameCrestLevel, string columnNameDC, string columnNameLC)
        {
            var weir = AddOrUpdateBranchFeatureFromNetwork<IWeir>(feature, columnNameName, branch =>
            {
                var branchFeature = new Weir(true);
                BranchStructure.AddStructureToNetwork(branchFeature, branch);
                return branchFeature;
            });

            if(weir == null)
            {
                log.ErrorFormat("Failed to import weir \"{0}\".", feature);
                return;
            }

            try
            {
                var formula = (SimpleWeirFormula)weir.WeirFormula;

                if (columnNameLongName != null)
                {
                    weir.LongName = feature.Attributes[columnNameLongName].ToString();
                }
                if (columnNameDescription != null)
                {
                    weir.Description = feature.Attributes[columnNameDescription].ToString();
                }
                if (columnNameCrestWidth != null)
                {
                    weir.CrestWidth = Convert.ToDouble(feature.Attributes[columnNameCrestWidth]);
                }
                if (columnNameCrestLevel != null)
                {
                    weir.CrestLevel = Convert.ToDouble(feature.Attributes[columnNameCrestLevel]);
                }
                if (columnNameDC != null)
                {
                    formula.CorrectionCoefficient *= Convert.ToDouble(feature.Attributes[columnNameDC]);
                }
                if (columnNameLC != null)
                {
                    formula.CorrectionCoefficient = Convert.ToDouble(feature.Attributes[columnNameLC]);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception ocurred during import of weir \"{0}\": {1}", weir.Name, e);
                // Resume regular control flow. 
            }
        }
    }
}
