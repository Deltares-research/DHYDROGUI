using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class LateralSourceFromGisImporter:NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LateralSourceFromGisImporter));
        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;

        public LateralSourceFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);

            base.FeatureFromGisImporterSettings.FeatureType = "Lateral sources";
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
                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "LateralSource from GIS importer"; }
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
                ImportLateralSource(feature,
                           propertyMappingName.MappingColumn.Alias,
                           propertyMappingLongName.MappingColumn.Alias,
                           propertyMappingDescription.MappingColumn.Alias
                    );

            }
            return HydroNetwork;
        }

        private void ImportLateralSource(IFeature feature, string columnNameName, string columnNameLongName, string columnNameDescription)
        {
            var lateralSource = GetLateralSourceFromNetwork(feature, columnNameName);

            if (lateralSource == null)
            {
                return;
            }

            if (columnNameLongName != null)
            {
                lateralSource.LongName = feature.Attributes[columnNameLongName].ToString();
            }
            if (columnNameDescription != null)
            {
                lateralSource.Description = feature.Attributes[columnNameDescription].ToString();
            }
        }

        private ILateralSource GetLateralSourceFromNetwork(IFeature feature, string columnNameName)
        {
            var lateralSourceName = feature.Attributes[columnNameName].ToString();
            var lateralSource = HydroNetwork.LateralSources.Where(w => w.Name == lateralSourceName).FirstOrDefault();
            var nearestBranch = NetworkHelper.GetNearestBranch(HydroNetwork.Branches, feature.Geometry, SnappingTolerance);


            if (lateralSource == null && nearestBranch == null)
            {
                return null;
            }

            if (lateralSource == null)
            {
                lateralSource = LateralSource.CreateDefault(nearestBranch);
                lateralSource.Name = lateralSourceName;
                NetworkHelper.AddBranchFeatureToBranch(lateralSource, nearestBranch, 0);
            }

            var coordinate = GeometryHelper.GetNearestPointAtLine((ILineString)lateralSource.Branch.Geometry,
                                                                  feature.Geometry.Coordinate, SnappingTolerance);

            if (coordinate == null)
            {
                log.ErrorFormat("Could not import {0}. {0} has probably moved to another branch. Make a new ID in the source data for this item.", lateralSource.Name);
                return lateralSource;
            }

            lateralSource.Geometry = new Point(coordinate);
            NetworkHelper.UpdateBranchFeatureChainageFromGeometry(lateralSource);
            return lateralSource;
        }
    }
}
