using System;
using System.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Snapping;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping
{
    /// <summary>
    /// Snaps a node to a branch (on add), or to nothing / node (on move)
    /// </summary>
    public class HydroNodeSnapRule : SnapRule
    {
        public override SnapResult Execute(IFeature sourceFeature, Tuple<IFeature, ILayer>[] candidates, IGeometry sourceGeometry, IList<IFeature> snapTargets, Coordinate worldPos, Envelope envelope, int trackingIndex)
        {
            var isMoving = sourceFeature != null; //existing node: moving
             
            if (isMoving)
            {
                var point = new Point(worldPos);
                return new SnapResult(point.Coordinates[0], null, null, point, 0, 0) { Rule = this };
            }

            return base.Execute(sourceFeature, candidates, sourceGeometry, snapTargets, worldPos, envelope, trackingIndex);
        }
    }
}
