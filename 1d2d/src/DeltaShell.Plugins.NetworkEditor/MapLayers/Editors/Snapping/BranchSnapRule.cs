using System;
using System.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Snapping;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping
{
    /// <summary>
    /// Snaps a branch to a node
    /// </summary>
    public class BranchSnapRule : SnapRule
    {
        public override SnapResult Execute(IFeature sourceFeature, Tuple<IFeature, ILayer>[] candidates, IGeometry sourceGeometry, IList<IFeature> snapTargets, Coordinate worldPos, Envelope envelope, int trackingIndex)
        {
            if (sourceGeometry != null)
            {
                // If branch exist only allow snapping to node if the first or last coordinate is
                // moved. Thus prevent snapping of other points of the linestring.
                if ((trackingIndex == 0) || (trackingIndex == (sourceGeometry.Coordinates.Length - 1)))
                    return base.Execute(sourceFeature, candidates, sourceGeometry, snapTargets, worldPos, envelope, trackingIndex);
            }
            else
            {
                // If branch does not exist allow snapping to node. This allows the newlinetool to
                // snap the first coordinate of a lineString to snap before the linestring is created.
                return base.Execute(null, candidates, null, snapTargets, worldPos, envelope, -1);
            }
            return null;
        }
    }
}