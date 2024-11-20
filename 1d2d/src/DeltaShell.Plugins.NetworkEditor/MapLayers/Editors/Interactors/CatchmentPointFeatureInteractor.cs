using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors;
using SharpMap.Editors.Interactors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class CatchmentPointFeatureInteractor : PointInteractor
    {
        public CatchmentPointFeatureInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject)
        {
        }

        protected override void CreateTrackers()
        {
            Bitmap symbol = GetValidSymbol();

            var generateComposite = VectorStyle != null 
                                        ? TrackerSymbolHelper.GenerateComposite(new Pen(Color.DarkBlue), new SolidBrush(Color.LightSkyBlue), symbol.Width, symbol.Height, 8, 8) 
                                        : TrackerSymbolHelper.GenerateSimple(new Pen(Color.Red, 2f), new SolidBrush(Color.White), 8, 8);

            var layerCustomRenderer = Layer.CustomRenderers?.FirstOrDefault();

            var coordinate = layerCustomRenderer != null
                                 ? layerCustomRenderer.GetRenderedFeatureGeometry(SourceFeature, Layer)
                                 : SourceFeature.Geometry.Centroid;

            Trackers.Add(new TrackerFeature(this, coordinate, 0, generateComposite));
            Trackers[0].Selected = true;
        }

        private Bitmap GetValidSymbol()
        {
            if (SourceFeature is Catchment catchment && catchment.CatchmentType.Icon != null)
            {
                return catchment.CatchmentType.Icon;
            }

            return VectorStyle.Symbol;
        }
    }
}   