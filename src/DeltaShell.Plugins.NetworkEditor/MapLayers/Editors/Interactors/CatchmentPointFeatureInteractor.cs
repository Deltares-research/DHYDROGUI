using System.Drawing;
using System.Linq;
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
            var generateComposite = VectorStyle != null 
                                        ? TrackerSymbolHelper.GenerateComposite(new Pen(Color.Transparent), new SolidBrush(Color.DarkBlue), VectorStyle.Symbol.Width, VectorStyle.Symbol.Height, 6, 6) 
                                        : TrackerSymbolHelper.GenerateSimple(new Pen(Color.Red, 2f), new SolidBrush(Color.White), 8, 8);

            var layerCustomRenderer = Layer.CustomRenderers?.FirstOrDefault();

            var coordinate = layerCustomRenderer != null
                                 ? layerCustomRenderer.GetRenderedFeatureGeometry(SourceFeature, Layer)
                                 : SourceFeature.Geometry.Centroid;

            Trackers.Add(new TrackerFeature(this, coordinate, 0, generateComposite));
            Trackers[0].Selected = true;
        }
    }
}   