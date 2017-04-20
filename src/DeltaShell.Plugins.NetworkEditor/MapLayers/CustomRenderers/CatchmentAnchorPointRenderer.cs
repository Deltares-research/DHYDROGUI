using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class CatchmentAnchorPointRenderer : IFeatureRenderer
    {
        public bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            var catchment = (Catchment)feature;

            var geometry = GetRenderedFeatureGeometry(feature, layer);

            var catchmentType = catchment.CatchmentType;

            var symbol = catchmentType == null
                             ? Properties.Resources.catchment
                             : catchment.SubCatchments.Count == 0
                                   ? catchmentType.Icon
                                   : catchmentType.SoftIcon;

            var vectorStyle = new VectorStyle {Fill = Brushes.DarkCyan};
            if (symbol != null)
            {
                vectorStyle.Symbol = symbol;
            }

            VectorRenderingHelper.RenderGeometry(g, layer.Map, geometry, vectorStyle, null, true);

            return true;
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            var catchment = (Catchment) feature;
            var geometry = catchment.Geometry is IPoint
                                ? catchment.Geometry
                                : catchment.InteriorPoint;

            return layer.CoordinateTransformation != null
                        ? GeometryTransform.TransformGeometry(geometry, layer.CoordinateTransformation.MathTransform)
                        : geometry;
        }

        public IEnumerable<IFeature> GetFeatures(IGeometry geometry, ILayer layer)
        {
            return GetFeatures(geometry.EnvelopeInternal, layer).OfType<IFeature>().ToList();
        }

        public IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            return layer.DataSource.Features.Cast<Catchment>()
                        .Where(feature => box.Intersects(GetRenderedFeatureGeometry(feature, layer).EnvelopeInternal))
                        .Cast<IFeature>()
                        .ToList();
        }
    }
}