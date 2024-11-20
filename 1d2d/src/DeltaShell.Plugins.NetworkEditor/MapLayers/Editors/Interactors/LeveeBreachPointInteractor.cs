using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class LeveeBreachPointInteractor : FeaturePointInteractor
    {
        public LeveeBreachPointInteractor(ILayer layer, IFeature feature, IEditableObject editableObject) : base(layer, feature, ((VectorLayer)layer).Style, editableObject)
        {
        }

        public override void Stop(SnapResult snapResult)
        {
            if (snapResult == null) // cancel
            {
                TargetFeature = null;
                return;
            }

            var feature2DPoint = SourceFeature as Feature2DPoint;
            if (feature2DPoint == null) return;
            
            base.Stop();
            feature2DPoint.Geometry = TargetFeature.Geometry;
        }
    }
}