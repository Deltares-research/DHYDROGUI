using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Snapping;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping
{
    public class CrossSectionSnapRule : SnapRule
    {
        public override SnapResult Execute(IFeature sourceFeature, Tuple<IFeature, ILayer>[] candidates, IGeometry sourceGeometry, IList<IFeature> snapTargets, Coordinate worldPos, Envelope envelope, int trackingIndex)
        {
            if (sourceFeature == null && NewFeatureLayer.FeatureEditor.CreateNewFeature != null)
            {
                sourceFeature = NewFeatureLayer.FeatureEditor.CreateNewFeature(NewFeatureLayer);
            }

            var crossSection = sourceFeature as ICrossSection;

            return crossSection == null || crossSection.GeometryBased
                ? new SnapResult(worldPos, null, null, null, -1, -1) {Rule = this}
                : base.Execute(sourceFeature, candidates, sourceGeometry, snapTargets, worldPos, envelope, trackingIndex);
        }
    }
}