using System;
using System.Collections.Generic;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Snapping;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping
{
    public class LeveeBreachSnapRule : SnapRule
    {
        public override SnapResult Execute(IFeature sourceFeature, Tuple<IFeature, ILayer>[] candidates, IGeometry sourceGeometry, IList<IFeature> snapTargets, Coordinate worldPos, Envelope envelope, int trackingIndex)
        {
            if (!(sourceFeature is Feature2DPoint feature2DPoint) || feature2DPoint.Attributes == null ||
                !feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) ||
                (LeveeBreachPointLocationType) feature2DPoint.Attributes[
                    LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE] !=
                LeveeBreachPointLocationType.BreachLocation ||
                !feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) ||
                !(feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE] is LeveeBreach leveeBreach))
                return new SnapResult(worldPos, null, null, null, -1, -1) {Rule = this};


            var myCandidates = new[] {new Tuple<IFeature, ILayer>(leveeBreach, NewFeatureLayer)};
            return base.Execute(sourceFeature, myCandidates, sourceGeometry, null, worldPos, envelope, trackingIndex);
        }
    }
}