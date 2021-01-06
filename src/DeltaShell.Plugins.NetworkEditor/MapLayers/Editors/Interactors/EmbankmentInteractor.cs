using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class EmbankmentInteractor : Feature2DLineInteractor
    {
        public EmbankmentInteractor(ILayer layer, IFeature feature, IEditableObject editableObject)
            : base(layer, feature, ((VectorLayer) layer).Style, editableObject) {}

        public override bool InsertTracker(Coordinate coordinate, SnapResult snapResult)
        {
            coordinate.Z = 0.0d;
            return base.InsertTracker(coordinate, snapResult);
        }
    }
}