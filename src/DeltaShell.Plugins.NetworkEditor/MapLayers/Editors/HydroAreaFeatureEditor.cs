using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors
{
    public class HydroAreaFeatureEditor : Feature2DEditor
    {
        public HydroArea Area { get; private set; }

        public HydroAreaFeatureEditor(HydroArea area) : base(area)
        {
            Area = area;
        }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            if (feature is Embankment)
            {
                return new EmbankmentInteractor(layer, feature, Area);
            }

            return base.CreateInteractor(layer, feature);
        }
    }
}