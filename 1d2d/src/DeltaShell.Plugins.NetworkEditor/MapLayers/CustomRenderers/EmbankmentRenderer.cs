using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Hydro;
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
    public class EmbankmentRenderer : IFeatureRenderer
    {
        readonly static IGeometryFactory geometryFactory = new GeometryFactory();
        
        private static readonly Color minColor = Color.Blue;
        private static readonly Color maxColor = Color.Tomato;

        private VectorStyle style;

        public EmbankmentRenderer()
        {
            // This style is a start: it will be adapted in the Render method. 
            style = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Line = new Pen(Color.Blue, 2)
            };           
        }
        
        public bool Render(IFeature feature, Graphics graphics, ILayer layer)
        {
            var embankment = feature as Embankment;
            if (embankment == null)
            {
                throw new InvalidOperationException("Cannot render incompatible feature, should be an Embankment.");
            }

            IGeometry geometry = GetRenderedFeatureGeometry(feature, layer); 
            var coordinates = geometry.Coordinates;
            var allValues = geometry.Coordinates.Select(c => c.Z).ToArray();

            double minValue = allValues.Min();
            double maxValue = allValues.Max();
            if (maxValue - minValue <= 0.0001)
            {
                maxValue = minValue + 0.0001; 
            }

            for (int i = 0; i < coordinates.Length - 1; i++)
            {
                var coordinateBegin = coordinates[i];
                var coordinateEnd = coordinates[i + 1];
                var line = new LineString(new[] {coordinateBegin, coordinateEnd});

                var viewEnvelope = layer.Map.Envelope;
                
                if (!(viewEnvelope.Contains(coordinateBegin) || viewEnvelope.Contains(coordinateEnd) || viewEnvelope.Intersects(line.EnvelopeInternal)))
                {
                    // This line is outside view. Don't render. 
                    continue; 
                }

                IPoint pointBegin = new Point(coordinateBegin);
                IPoint pointEnd = new Point(coordinateEnd);
                PointF pointFBegin = Transform.TransformToImage(pointBegin, (Map)layer.Map);
                PointF pointFEnd = Transform.TransformToImage(pointEnd, (Map)layer.Map);

                double valueBegin = geometry.Coordinates[i].Z;
                double valueEnd = geometry.Coordinates[i + 1].Z; 

                double proportionBegin = (valueBegin - minValue)/(maxValue - minValue);
                double proportionEnd = (valueEnd - minValue)/(maxValue - minValue);

                // Create the gradient. 
                Color colorBegin = InterpolateColors(minColor, maxColor, proportionBegin);
                Color colorEnd = InterpolateColors(minColor, maxColor, proportionEnd);
                
                // If the coordinates are the same, System.Drawing.dll will give an OutOfMemoryException. 
                // See for instance: http://stackoverflow.com/questions/6506089/system-drawing-out-of-memory-exception
                if (Math.Abs(pointFBegin.X - pointFEnd.X) + Math.Abs(pointFBegin.Y - pointFEnd.Y) < 0.01)
                {
                    continue; 
                }

                style.Line.Brush = new LinearGradientBrush(pointFBegin, pointFEnd, colorBegin, colorEnd);

                // Draw the geometry. 
                VectorRenderingHelper.RenderGeometry(graphics, layer.Map, line, style, null, true);
            }

            return true; 
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

        private static Color InterpolateColors(Color color1, Color color2, double proportion)
        {
            byte a = Convert.ToByte(color1.A + proportion * (color2.A - color1.A));
            byte r = Convert.ToByte(color1.R + proportion * (color2.R - color1.R));
            byte g = Convert.ToByte(color1.G + proportion * (color2.G - color1.G));
            byte b = Convert.ToByte(color1.B + proportion * (color2.B - color1.B));
            return Color.FromArgb(a, r, g, b); 
        }
    }
}
