using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Rendering;
using SharpMap.Styles;
using System.Collections.Generic;
using System.Drawing;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.CustomRenderers
{
    public class BoundaryRenderer : IFeatureRenderer
    {
        static IGeometryFactory geometryFactory = new GeometryFactory();

        public bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            var featureCoordiates = feature.Geometry.Coordinates;

            // if there is only one coordinate, or there are 2 coordinates and they are the same
            if ( featureCoordiates.Length == 1 || ( featureCoordiates.Length == 2 && featureCoordiates[0].Equals3D(featureCoordiates[1]) ) )
            {
                var pointCoordinate = new Point(feature.Geometry.Coordinate);
                VectorRenderingHelper.DrawPoint(g, pointCoordinate, Properties.Resources.boundary, 1, new PointF(0, 0), 0, layer.Map);

                var vectorStyle = new VectorStyle
                {
                    GeometryType = typeof (IPoint),
                    Symbol = Properties.Resources.boundary
                };

                VectorRenderingHelper.RenderGeometry(g, layer.Map, pointCoordinate, vectorStyle, Properties.Resources.boundary, false);

                return true;
            }
            else return false; // else revert to default behaviour in VectorLayer
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            return layer.CoordinateTransformation != null
                ? GeometryTransform.TransformGeometry(feature.Geometry, layer.CoordinateTransformation.MathTransform)
                : feature.Geometry;
        }

        public IEnumerable<IFeature> GetFeatures(IGeometry geometry, ILayer layer)
        {
            return GetFeatures(geometry.EnvelopeInternal, layer);
        }

        public IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            return layer.GetFeatures(geometryFactory.ToGeometry(box), false);
        }
    }
}
