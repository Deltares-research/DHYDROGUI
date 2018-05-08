using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Rendering;
using SharpMap.Styles;
using SharpMap.Utilities;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class DamBreakRenderer : IFeatureRenderer
    {
        readonly static IGeometryFactory geometryFactory = new GeometryFactory();

        private VectorStyle breachLocationStyle;
        private VectorStyle damBreakLineStyle;

        public DamBreakRenderer(VectorStyle damBreakLineStyle, VectorStyle breachLocationStyle)
        {
            this.damBreakLineStyle = damBreakLineStyle;
            this.breachLocationStyle = breachLocationStyle;
        }



        public bool Render(IFeature feature, Graphics graphics, ILayer layer)
        {
            var damBreak = feature as DamBreak;
            if (damBreak == null)
            {
                throw new InvalidOperationException("Cannot render incompatible feature, should be an Dam break.");
            }

            var line = GetRenderedFeatureGeometry(feature, layer);

           // Draw line. 
           VectorRenderingHelper.RenderGeometry(graphics, layer.Map, line, damBreakLineStyle, null, true);

           // Draw breach location.
            var point = GetTransformedGeometry(damBreak.BreachLocation, layer);

            VectorRenderingHelper.RenderGeometry(graphics, layer.Map, point, breachLocationStyle, null, true);

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
