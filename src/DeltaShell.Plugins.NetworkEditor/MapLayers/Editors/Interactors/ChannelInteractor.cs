using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class ChannelInteractor : BranchInteractor
    {
        private readonly BranchNodeConnector branchNodeConnector = new BranchNodeConnector();
        private readonly BranchNodeDisconnector branchNodeDisconnector = new BranchNodeDisconnector();
        
        public ChannelInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject)
        {
        }

        public override void Add(IFeature feature)
        {
            if (Network.CoordinateSystem != null)
            {
                var channel = (Channel)SourceFeature;
                channel.GeodeticLength = GeodeticDistance.Length(Network.CoordinateSystem, channel.Geometry);
            }
            
            branchNodeConnector.ConnectNodes((IBranch)feature, Network);

            base.Add(feature);
        }

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            return HydroNetworkFeatureEditor.GetFeatureRelationInteractor(feature);
        }

        public override void Delete()
        {
            var channel = (Channel)SourceFeature;

            var sourceNode = (HydroNode)channel.Source;
            var targetNode = (HydroNode)channel.Target;

            var links = channel.GetAllItemsRecursive().OfType<IHydroObject>().Where(o => o.Links != null).SelectMany(o => o.Links).ToArray();
            foreach (var link in links)
            {
                HydroRegion.RemoveLink(link);
            }

            base.Delete();

            // delete links to nodes which are deleted due to deleted branch
            if (!Network.Nodes.Contains(sourceNode))
            {
                sourceNode.Links.ToArray().ForEach(HydroRegion.RemoveLink);
            }
            if (!Network.Nodes.Contains(targetNode))
            {
                targetNode.Links.ToArray().ForEach(HydroRegion.RemoveLink);
            }

            var hydroNetwork = (IHydroNetwork)Network;
            branchNodeDisconnector.DisconnectNodes(sourceNode, hydroNetwork);
            branchNodeDisconnector.DisconnectNodes(targetNode, hydroNetwork);
        }

        public override void Stop()
        {
            base.Stop();

            // update links
            var channel = (Channel)SourceFeature;

            // source node links
            foreach (var link in ((HydroNode)channel.Source).Links.ToArray())
            {
                var nodeCoordinate = channel.Source.Geometry.Coordinate;
                var linkGeometry = link.Geometry;
                if (!Equals(linkGeometry.Coordinates.Last(), nodeCoordinate))
                {
                    linkGeometry.Coordinates[linkGeometry.Coordinates.Length - 1] = nodeCoordinate;
                    link.Geometry = linkGeometry; // force refresh
                }
            }

            // target node links
            foreach (var link in ((HydroNode)channel.Target).Links.ToArray())
            {
                var nodeCoordinate = channel.Target.Geometry.Coordinate;
                var linkGeometry = link.Geometry;
                if (!Equals(linkGeometry.Coordinates.Last(), nodeCoordinate))
                {
                    linkGeometry.Coordinates[linkGeometry.Coordinates.Length - 1] = nodeCoordinate;
                    link.Geometry = linkGeometry; // force refresh
                }
            }
        }
    }
}