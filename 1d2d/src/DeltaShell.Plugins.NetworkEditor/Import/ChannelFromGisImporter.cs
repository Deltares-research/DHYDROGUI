using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using SharpMap.CoordinateSystems.Transformations;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    [Serializable]
    public class ChannelFromGisImporter: NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChannelFromGisImporter));

        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingNodeTo;
        private PropertyMapping propertyMappingNodeFrom;
        private PropertyMapping propertyMappingIsCustomLength;
        private PropertyMapping propertyMappingCustomLength;

        public ChannelFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");
            propertyMappingNodeFrom = new PropertyMapping("Source node");
            propertyMappingNodeTo = new PropertyMapping("Target node");
            propertyMappingIsCustomLength = new PropertyMapping("Is custom length");
            propertyMappingCustomLength = new PropertyMapping("Custom length");

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingNodeFrom);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingNodeTo);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingIsCustomLength);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingCustomLength);

            base.FeatureFromGisImporterSettings.FeatureType = "Channels";
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
                propertyMappingNodeFrom = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingNodeFrom.PropertyName);
                propertyMappingNodeTo = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingNodeTo.PropertyName);
                propertyMappingIsCustomLength = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingIsCustomLength.PropertyName);
                propertyMappingCustomLength = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingCustomLength.PropertyName);
                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingNodeFrom.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingNodeTo.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingIsCustomLength.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingCustomLength.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "Channel from GIS importer"; }
        }

        // add invokerequired to prevent NotifyPropertyChanged listeners to throw exception on cross thread 
        // handling; see TOOLS-4520
        [InvokeRequired]
        public override object ImportItem(string path, object target = null)
        {
            var features = GetFeatures();

            foreach (IFeature feature in features)
            {
                bool newChannel = false;

                var geometry = feature.Geometry as ILineString;
                if (geometry == null)
                {
                    Log.ErrorFormat("Wrong shape geometry for channel: {0}, should be Line String, cancelling import", feature.Geometry.GeometryType);
                    return HydroNetwork;
                }

                var featureName = feature.Attributes[propertyMappingName.MappingColumn.Alias].ToString();
                IChannel channel = HydroNetwork.Channels.FirstOrDefault(branch => branch.Name == featureName);
                if (channel == null)
                {
                    newChannel = true;
                    channel = Channel.CreateDefault(HydroNetwork);
                    channel.Name = featureName;
                }
                
                if (!newChannel)
                {
                    geometry = AdjustGeometryToFitNodeLocations(channel, geometry);
                }

                if (propertyMappingIsCustomLength.MappingColumn.Alias != null)
                {
                    string value = feature.Attributes[propertyMappingIsCustomLength.MappingColumn.Alias].ToString().ToLower();
                    if (value == "1" || value == "true")
                    {
                        channel.IsLengthCustom = true;
                    }
                }

                // Only set custom length when the the user indicated that the 
                if (propertyMappingCustomLength.MappingColumn.Alias != null)
                {
                    string valueString = feature.Attributes[propertyMappingCustomLength.MappingColumn.Alias].ToString().ToLower();
                    double customLength;
                    if (Double.TryParse(valueString, out customLength) && customLength > 0.0)
                    {
                        channel.Length = customLength;
                    }
                }

                UpdateGeometry(channel, geometry);

                if (propertyMappingDescription.MappingColumn.Alias != null)
                {
                    channel.Description = feature.Attributes[propertyMappingDescription.MappingColumn.Alias].ToString();
                }

                if (propertyMappingLongName.MappingColumn.Alias != null)
                {
                    channel.LongName = feature.Attributes[propertyMappingLongName.MappingColumn.Alias].ToString();
                }

                if (newChannel)
                {
                    NetworkHelper.AddChannelToHydroNetwork(HydroNetwork, channel);
                }

                INode node;
                string name;
                if (propertyMappingNodeTo.MappingColumn.Alias != null)
                {
                    name = feature.Attributes[propertyMappingNodeTo.MappingColumn.Alias].ToString();
                    node = HydroNetwork.Nodes.FirstOrDefault(n => n.Geometry.Coordinate.Equals(channel.Geometry.Coordinates.Last()));
                    if(node != null && name != "")
                    {
                        node.Name = name;
                    }
                }

                if (propertyMappingNodeFrom.MappingColumn.Alias != null)
                {
                    name = feature.Attributes[propertyMappingNodeFrom.MappingColumn.Alias].ToString();
                    node = HydroNetwork.Nodes.FirstOrDefault(n => n.Geometry.Coordinate.Equals(channel.Geometry.Coordinates.First()));
                    if (node != null && name != "")
                    {
                        node.Name = name;
                    }
                }
            }

            return HydroNetwork;
        }

        private double GetGeodeticDistance(ICoordinateSystem coordinateSystem, IGeometry channelGeometry)
        {
            if (coordinateSystem == null)
            {
                return double.NaN;
            }

            var geodeticDistance = new GeodeticDistance(coordinateSystem);


            var distance = 0.0;

            for (int index = 1; index < channelGeometry.Coordinates.Length; ++index)
                distance += geodeticDistance.Distance(channelGeometry.Coordinates[index - 1],
                                                      channelGeometry.Coordinates[index]);

            return distance;

        }

        private void UpdateGeometry(IBranch branch, ILineString newLineString)
        {
            double factor = branch.IsLengthCustom ? 1.0 : newLineString.Length / branch.Geometry.Length;
            branch.Geometry = newLineString;

            foreach (var branchFeature in branch.BranchFeatures)
            {
                if (branchFeature is ICrossSection)
                {
                    // if IsLengthCustom keep chainage else scale
                    var crossSection = (ICrossSection)branchFeature;
                    if (crossSection.Definition.GeometryBased)
                    {
                        continue;
                    }

                    if (!branch.IsLengthCustom)
                    {
                        crossSection.Chainage = BranchFeature.SnapChainage(crossSection.Branch.Length, branchFeature.Chainage * factor);
                    }

                    crossSection.Geometry = null; //clear cache: geometry will be re-calculated internally
                }
                else
                {
                    // if IsLengthCustom keep chainage else scale
                    if (!branch.IsLengthCustom)
                    {
                        branchFeature.Chainage = BranchFeature.SnapChainage(branchFeature.Branch.Length, branchFeature.Chainage * factor);
                    }
                    var mapChainage = branchFeature.Chainage;
                    if (branch.IsLengthCustom)
                    {
                        mapChainage = BranchFeature.SnapChainage(newLineString.Length, (newLineString.Length / branch.Length) * mapChainage);
                    }

                    var coordinate = GeometryHelper.LineStringCoordinate(newLineString, mapChainage);
                    branchFeature.Geometry = GeometryHelper.SetCoordinate(branchFeature.Geometry, 0, coordinate);
                }
            }
            branch.GeodeticLength = GetGeodeticDistance(HydroNetwork.CoordinateSystem, branch.Geometry);
        }

        private ILineString AdjustGeometryToFitNodeLocations(IChannel channel, ILineString newGeometry)
        {
            var geometry = newGeometry;

            if (channel.Source != null && channel.Source.Geometry != null && 
                channel.Target != null && channel.Target.Geometry != null && 
                newGeometry.Coordinates.Length >= 2)
            {
                var newSource = new Point(newGeometry.Coordinates.First());
                var newTarget = new Point(newGeometry.Coordinates.Last());

                if (!channel.Source.Geometry.IsWithinDistance(newSource, SnappingTolerance))
                {
                    Log.WarnFormat(
                        "New geometry for existing channel {0} does not match with geometry of its source node {1}. Geometry adjusted.",
                        channel.Name, channel.Source.Name);

                    var newCoordinates = new []{channel.Source.Geometry.Coordinate}.Concat(geometry.Coordinates).ToArray();
                    geometry = new LineString(newCoordinates);
                }

                if (!channel.Target.Geometry.IsWithinDistance(newTarget, SnappingTolerance))
                {
                    Log.WarnFormat(
                        "New geometry for existing channel {0} does not match with geometry of its target node {1}. Geometry adjusted.",
                        channel.Name, channel.Target.Name);
                    var newCoordinates = geometry.Coordinates.Concat(new[] {channel.Target.Geometry.Coordinate}).ToArray();
                    geometry = new LineString(newCoordinates);
                }
            }
            return geometry;
        }
    }
}
