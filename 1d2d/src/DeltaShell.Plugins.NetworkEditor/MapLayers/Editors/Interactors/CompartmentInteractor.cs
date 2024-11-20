using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class CompartmentInteractor : PointInteractor
    {
        public CompartmentInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject)
        {
        }

        protected override bool AllowDeletionCore()
        {
            return false;
        }

        protected override bool AllowMoveCore()
        {
            return false;
        }
        
        public override bool AllowSingleClickAndMove()
        {
            return false;
        }
    }
}