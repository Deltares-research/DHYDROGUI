using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
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

            
            if (feature is Feature2DPoint feature2DPoint &&
                feature2DPoint.Attributes != null &&
                feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                (LeveeBreachPointLocationType)feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE] == 
                LeveeBreachPointLocationType.BreachLocation)
            {
                return new LeveeBreachPointInteractor(layer, feature, Area);
            }

            return base.CreateInteractor(layer, feature);
        }
    }
}