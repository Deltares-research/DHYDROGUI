using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class DiffuseLateralSourceRenderer : BranchFeatureRenderer
    {
        public override IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            var vectorLayer = (VectorLayer) layer;
            var symbol = vectorLayer.Style.Symbol;
            var boxExpandedForImageSize = MapHelper.GetEnvelopeForImage(layer.Map, box.Centre, symbol.Width * 1.2, symbol.Height * 1.2);
            var potentialDiffuse = base.GetFeatures(box, layer)
                .OfType<ILateralSource>()
                .Where(ls => ls.IsDiffuse).Cast<IFeature>();
            var potentialNonDiffuse = base.GetFeatures(boxExpandedForImageSize, layer)
                .OfType<ILateralSource>()
                .Where(ls => !ls.IsDiffuse).Cast<IFeature>();

            return potentialNonDiffuse.Concat(potentialDiffuse);
        }

        public override bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            if (!(feature is ILateralSource))
            {
                return false;
            }

            vectorLayer = layer as VectorLayer;
            if (vectorLayer == null)
            {
                return false;
            }

            var lateralSource = (ILateralSource) feature;
            if (!lateralSource.IsDiffuse)
            {
                return false;
            }

            var themeOn = vectorLayer.Theme != null;
            var currentFeature = feature;
            var currentGeometry = GetRenderedFeatureGeometry(currentFeature, vectorLayer);

            var currentVectorStyle = themeOn
                                         ? vectorLayer.Theme.GetStyle(currentFeature) as VectorStyle
                                         : vectorLayer.Style;

            if (null == currentVectorStyle)
            {
                return false;
            }
            
            currentVectorStyle.Outline.Color = Color.MediumVioletRed;
            currentVectorStyle.Outline.Width = 6;
            currentVectorStyle.Outline.DashStyle = DashStyle.Dash;
            
            if (vectorLayer.Style.EnableOutline && (!themeOn || (currentVectorStyle.Enabled && currentVectorStyle.EnableOutline)))
            {
                // Draw background of all line-outlines first
                switch (currentGeometry.GeometryType)
                {
                    case "LineString":
                        VectorRenderingHelper.DrawLineString(g, currentGeometry as ILineString, currentVectorStyle.Outline, layer.Map);
                        break;
                    case "MultiLineString":
                        VectorRenderingHelper.DrawMultiLineString(g, currentGeometry as IMultiLineString, currentVectorStyle.Outline, layer.Map);
                        break;
                }
            }

            VectorRenderingHelper.RenderGeometry(g, layer.Map, currentGeometry, currentVectorStyle, null, vectorLayer.ClippingEnabled);

            return true;
        }

        public override object Clone()
        {
            return new DiffuseLateralSourceRenderer();
        }
    }
}
