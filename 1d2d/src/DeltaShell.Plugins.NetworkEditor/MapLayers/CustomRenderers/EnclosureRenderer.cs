using System;
using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class EnclosureRenderer : IFeatureRenderer
    {
        private static readonly IGeometryFactory geometryFactory = new GeometryFactory();
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnclosureRenderer));
        private VectorStyle style;

        public EnclosureRenderer()
        {
            // This style is a start: it will be adapted in the Render method. 
            style = new VectorStyle
            {
                GeometryType = typeof(IPolygon),
                Fill = new SolidBrush(Color.DarkGray),
                Outline = new Pen(Color.FromArgb(100, Color.CornflowerBlue), 4f),
            };
        }

        public bool Render(IFeature feature, Graphics graphics, ILayer layer)
        {
            if (! (feature is Feature2DPolygon))
            {
                throw new InvalidOperationException("Cannot render incompatible feature, should be an Enclosure.");
            }

            IGeometry geometry = GetRenderedFeatureGeometry(feature, layer);

            var viewEnvelope = layer.Map.Envelope;
            var newPol = new Polygon(new LinearRing(new[]{
                                      new Coordinate( viewEnvelope.MinX, viewEnvelope.MinY),  new Coordinate( viewEnvelope.MaxX, viewEnvelope.MinY),
                                      new Coordinate( viewEnvelope.MaxX, viewEnvelope.MaxY),  new Coordinate( viewEnvelope.MinX, viewEnvelope.MaxY),
                                      new Coordinate( viewEnvelope.MinX, viewEnvelope.MinY)
                                  }));

            var geoAsPol = geometry as Polygon;
            if( geoAsPol == null || ! geoAsPol.IsValid)
            {
                /* The log message is already being given (once) by the FlowFMMapLayerProvider, if we include a log message
                 here it will be continously shown as it is refreshed constantly.*/
                return false;
            }

            try
            {
                var newGeo = newPol.Difference(geoAsPol);
                // Draw the geometry. 
                VectorRenderingHelper.RenderGeometry(graphics, layer.Map, newGeo, style, null, true);
            }
            catch(Exception e)
            {
                /* This means something went terribly wrong while rendering geometry, should not happen */
                Log.DebugFormat("An error rendering the difference between two polygon prevents the new layer from being created. Exception: {0}", e.Message);
                Log.Error("An error rendering the enclosure geometry prevents the layer from showing it. Make sure the drawn enclosure is correct.");
                return false;
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
    }
}