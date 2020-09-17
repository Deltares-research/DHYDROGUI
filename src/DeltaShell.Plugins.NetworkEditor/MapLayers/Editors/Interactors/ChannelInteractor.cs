using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class ChannelInteractor : BranchInteractor
    {
        public ChannelInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject) {}

        public override void Add(IFeature feature)
        {
            if (Network.CoordinateSystem != null)
            {
                var channel = (Channel) SourceFeature;
                channel.GeodeticLength = GeodeticDistance.Length(Network.CoordinateSystem, channel.Geometry);
            }

            base.Add(feature);
        }

        public override void Delete()
        {
            var channel = (Channel) SourceFeature;

            var sourceNode = (HydroNode) channel.Source;
            var targetNode = (HydroNode) channel.Target;

            HydroLink[] links = channel.GetAllItemsRecursive().OfType<IHydroObject>().Where(o => o.Links != null).SelectMany(o => o.Links).ToArray();
            foreach (HydroLink link in links)
            {
                HydroRegion.RemoveLink(link);
            }

            base.Delete();

            // delete links to nodes which are deleted due to deleted branch - TODO: this must happen via related feature interactors / rules
            if (!Network.Nodes.Contains(sourceNode))
            {
                sourceNode.Links.ToArray().ForEach(HydroRegion.RemoveLink);
            }

            if (!Network.Nodes.Contains(targetNode))
            {
                targetNode.Links.ToArray().ForEach(HydroRegion.RemoveLink);
            }
        }

        public override void Stop()
        {
            base.Stop();

            // update links
            var channel = (Channel) SourceFeature;

            // source node links
            foreach (HydroLink link in ((HydroNode) channel.Source).Links.ToArray())
            {
                Coordinate nodeCoordinate = channel.Source.Geometry.Coordinate;
                IGeometry linkGeometry = link.Geometry;
                if (!Equals(linkGeometry.Coordinates.Last(), nodeCoordinate))
                {
                    linkGeometry.Coordinates[linkGeometry.Coordinates.Length - 1] = nodeCoordinate;
                    link.Geometry = linkGeometry; // force refresh
                }
            }

            // target node links
            foreach (HydroLink link in ((HydroNode) channel.Target).Links.ToArray())
            {
                Coordinate nodeCoordinate = channel.Target.Geometry.Coordinate;
                IGeometry linkGeometry = link.Geometry;
                if (!Equals(linkGeometry.Coordinates.Last(), nodeCoordinate))
                {
                    linkGeometry.Coordinates[linkGeometry.Coordinates.Length - 1] = nodeCoordinate;
                    link.Geometry = linkGeometry; // force refresh
                }
            }
        }
    }
}