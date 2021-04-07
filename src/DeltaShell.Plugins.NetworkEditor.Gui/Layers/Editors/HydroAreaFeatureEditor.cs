using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Editors
{
    public class HydroAreaFeatureEditor : Feature2DEditor
    {
        public HydroAreaFeatureEditor(HydroArea area) : base(area)
        {
            Area = area;
        }

        public HydroArea Area { get; private set; }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature) =>
            feature is Embankment
                ? new EmbankmentInteractor(layer, feature, Area)
                : base.CreateInteractor(layer, feature);
    }
}