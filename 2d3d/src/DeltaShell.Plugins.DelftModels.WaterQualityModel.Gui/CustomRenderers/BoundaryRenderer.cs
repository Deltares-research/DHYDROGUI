using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Rendering;
using SharpMap.Styles;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.CustomRenderers
{
    public class BoundaryRenderer : IFeatureRenderer
    {
        private static IGeometryFactory geometryFactory = new GeometryFactory();

        public bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            Coordinate[] featureCoordiates = feature.Geometry.Coordinates;

            if (featureCoordiates.Length != 1 &&
                (featureCoordiates.Length != 2 || !featureCoordiates[0].Equals3D(featureCoordiates[1])))
            {
                return false; // revert to default behaviour in VectorLayer
            }

            // if there is only one coordinate, or there are 2 coordinates and they are the same
            IGeometry transformedGeometry = GetRenderedFeatureGeometry(feature, layer);
            var pointCoordinate = new Point(transformedGeometry.Coordinate);
            VectorRenderingHelper.DrawPoint(g, pointCoordinate, Resources.boundary, 1, new PointF(0, 0), 0, layer.Map);

            var vectorStyle = new VectorStyle
            {
                GeometryType = typeof(IPoint),
                Symbol = Resources.boundary
            };

            VectorRenderingHelper.RenderGeometry(g, layer.Map, pointCoordinate, vectorStyle, Resources.boundary, false);

            return true;
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            return layer.CoordinateTransformation != null
                       ? GeometryTransform.TransformGeometry(feature.Geometry,
                                                             layer.CoordinateTransformation.MathTransform)
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