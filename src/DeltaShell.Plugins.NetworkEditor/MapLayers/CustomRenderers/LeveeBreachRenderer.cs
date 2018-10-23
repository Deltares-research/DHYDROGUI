using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class LeveeBreachRenderer : IFeatureRenderer
    {
        readonly static IGeometryFactory geometryFactory = new GeometryFactory();

        private VectorStyle breachStyle;
        private VectorStyle leveeStyle;

        public LeveeBreachRenderer(VectorStyle leveeStyle, VectorStyle breachStyle)
        {
            this.leveeStyle = leveeStyle;
            this.breachStyle = breachStyle;
        }



        public bool Render(IFeature feature, Graphics graphics, ILayer layer)
        {
            var leveeBreach = feature as ILeveeBreach;
            if (leveeBreach == null)
            {
                throw new InvalidOperationException("Cannot render incompatible feature, should be a Levee breach.");
            }

            var line = GetRenderedFeatureGeometry(feature, layer);

           // Draw line. 
           VectorRenderingHelper.RenderGeometry(graphics, layer.Map, line, leveeStyle, null, true);

           // Draw breach location.
            var point = GetTransformedGeometry(leveeBreach.BreachLocation, layer);

            VectorRenderingHelper.RenderGeometry(graphics, layer.Map, point, breachStyle, null, true);

            return true;
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            return GetTransformedGeometry(feature.Geometry, layer);
        }

        private static IGeometry GetTransformedGeometry(IGeometry geometry, ILayer layer)
        {
            return layer.CoordinateTransformation != null
                ? GeometryTransform.TransformGeometry(geometry, layer.CoordinateTransformation.MathTransform)
                : geometry;
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
