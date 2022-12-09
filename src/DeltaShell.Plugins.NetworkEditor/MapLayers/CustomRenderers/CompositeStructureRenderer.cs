using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;
using SharpMap.Rendering;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    /// <summary>
    /// Custom renderer to draw structureFeatures. The StructureFeature represent a collection of
    /// structures at a location of a branch. The custom renderer draws polygon below the collection 
    /// of structures. If a StructureFeature contains only 1 structure it is invisible.
    /// </summary>
    public class CompositeStructureRenderer : IFeatureRenderer, ICloneable, IDisposable
    {
        private readonly IDictionary<IFeature, IGeometry> customGeometries = new Dictionary<IFeature, IGeometry>();
        private readonly IDictionary<IFeature, int> structureCounts = new Dictionary<IFeature, int>();
        private Envelope lastEnvelope;
        private readonly StructureRenderer structureRenderer;

        public CompositeStructureRenderer(StructureRenderer structureRenderer)
        {
            this.structureRenderer = structureRenderer;
        }

        #region IFeatureRenderer Members

        /// <summary>
        /// Renders a polygon that represent the structureFeature.
        /// The current implementation uses a polygon that stretches the width of the visible structures.
        /// s  s  s  s        - 4 structures
        /// [--------]        - structureFeature polygon
        /// Since the structureFeature polygon is in device coordinates based unlike the feature itself 
        /// which is in worldcoordinates it must beupdated after the zoomlevel has changed.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="g"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public virtual bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            if ((null == lastEnvelope)
                || (lastEnvelope.Width != layer.Map.Envelope.Width)
                || (lastEnvelope.Height != layer.Map.Envelope.Height))
            {
                lastEnvelope = layer.Map.Envelope.Clone();
                customGeometries.Clear();
                structureCounts.Clear();
            }

            var compositePointFeature = (ICompositeNetworkPointFeature) feature;
            
            if (IsBranchWithSingleFeature(compositePointFeature))
            {
                return true;
            }

            if (IsNodeWithoutFeatures(compositePointFeature))
            {
                return false;
            }

            DrawPolygon(feature, g, layer, compositePointFeature);
            return compositePointFeature.NetworkFeatureType == NetworkFeatureType.Branch; 
        }

        public virtual IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            var intersectedFeatures = new List<IFeature>();

            foreach (IFeature feature in layer.DataSource.Features)
            {
                IGeometry geometry = GetRenderedFeatureGeometry(feature, layer);
                var envelope = geometry?.EnvelopeInternal?.Copy();
                envelope?.ExpandBy(layer.Map.PixelSize * 5);

                if (envelope != null && envelope.Intersects(box))
                {
                    intersectedFeatures.Add(feature);
                }
            }
            return intersectedFeatures;
        }

        public IEnumerable<IFeature> GetFeatures(IGeometry geometry, ILayer layer)
        {
            var intersectedFeatures = new List<IFeature>();

            foreach (IFeature feature in layer.DataSource.Features)
            {
                IGeometry geometryFeature = GetRenderedFeatureGeometry(feature, layer);
                var envelope = geometryFeature?.EnvelopeInternal?.Copy();
                envelope?.ExpandBy(layer.Map.PixelSize * 5);
                if (envelope != null)
                {
                    var coordinates = new[]
                    {
                        new Coordinate(envelope.MinX, envelope.MinY),
                        new Coordinate(envelope.MinX, envelope.MaxY),
                        new Coordinate(envelope.MaxX, envelope.MaxY),
                        new Coordinate(envelope.MaxX, envelope.MinY),
                        new Coordinate(envelope.MinX, envelope.MinY)
                    };
                    var envelopeGeometry = new LinearRing(coordinates);
                    if (envelopeGeometry.Intersects(geometry))
                    {
                        intersectedFeatures.Add(feature);
                    }
                }
            }
            return intersectedFeatures;
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            var compositeStructure = (ICompositeNetworkPointFeature) feature;
            if (IsNodeWithoutFeatures(compositeStructure))
            {
                return feature.Geometry;
            }

            IGeometry geometry;

            if (!customGeometries.ContainsKey(feature))
            {
                if (null != structureRenderer)
                {
                    foreach (var structure in compositeStructure.GetPointFeatures())
                    {
                        structureRenderer.InvalidateStructure(structure);
                    }
                }
                geometry = GenerateCustomGeometry(feature, (VectorLayer) layer);
            }
            else
            {
                geometry = customGeometries[feature];
                var oldCoordinate = (Coordinate) geometry.UserData;

                // Update the geometry if feature has moved or #structures in 
                // structureFeature has changed
                if ((!feature.Geometry.Coordinates[0].Equals2D(oldCoordinate)))
                {
                    geometry = GenerateCustomGeometry(feature, (VectorLayer) layer);
                }
                else if (structureCounts[feature] != compositeStructure.GetPointFeatures().Count())
                {
                    if (null != structureRenderer)
                    {
                        foreach (var structure in compositeStructure.GetPointFeatures())
                        {
                            structureRenderer.InvalidateStructure(structure);
                        }
                    }
                    geometry = GenerateCustomGeometry(feature, (VectorLayer) layer);
                }
            }
            customGeometries[feature] = geometry;
            structureCounts[feature] = compositeStructure.GetPointFeatures().Count();
            return geometry;
        }

        private static bool IsNodeWithoutFeatures(ICompositeNetworkPointFeature compositePointFeature)
        {
            return compositePointFeature.NetworkFeatureType == NetworkFeatureType.Node && 
                   !compositePointFeature.GetPointFeatures().Any();
        }

        private static bool IsBranchWithSingleFeature(ICompositeNetworkPointFeature compositePointFeature)
        {
            return compositePointFeature.NetworkFeatureType == NetworkFeatureType.Branch && compositePointFeature.GetPointFeatures().Count() <= 1;
        }

        private void DrawPolygon(IFeature feature, Graphics g, ILayer layer, ICompositeNetworkPointFeature compositePointFeature)
        {
            var vectorLayer = (VectorLayer) layer;

            customGeometries[feature] = GetRenderedFeatureGeometry(feature, vectorLayer);
            structureCounts[feature] = compositePointFeature.GetPointFeatures().Count();

            var polygon = (IPolygon) customGeometries[feature];
            VectorRenderingHelper.DrawPolygon(g, polygon, Brushes.GreenYellow, Pens.Red, false, vectorLayer.Map);
        }

        private static IGeometry GenerateCustomGeometry(IFeature feature, VectorLayer layer)
        {
            var compositeStructure = (ICompositeNetworkPointFeature) feature;
            var org = layer.Map.ImageToWorld(new PointF(0, 0));
            var range = layer.Map.ImageToWorld(new PointF(layer.Style.Symbol.Width * compositeStructure.GetPointFeatures().GroupBy(s => s.GetType()).Count(), layer.Style.Symbol.Height));
            var anchor = feature.Geometry.Coordinates[0];

            var width = range.X - org.X;
            var halfHeight = (range.Y - org.Y) / 2;

            int upwardTranslationFactor;
            int downwardTranslationFactor;
            PointFeatureRenderingHelper.DetermineTranslationFactorForComposite(compositeStructure.NetworkFeatureType, out upwardTranslationFactor, out downwardTranslationFactor);

            var vertices = new List<Coordinate>
            {
                new Coordinate(anchor.X - width / 2, anchor.Y + downwardTranslationFactor * halfHeight),
                new Coordinate(anchor.X - width / 2, anchor.Y + upwardTranslationFactor * halfHeight),
                new Coordinate(anchor.X + width / 2, anchor.Y + upwardTranslationFactor * halfHeight),
                new Coordinate(anchor.X + width / 2, anchor.Y + downwardTranslationFactor * halfHeight)
            };

            vertices.Add((Coordinate) vertices[0].Clone());

            var newLinearRing = GeometryFactory.CreateLinearRing(vertices.ToArray());
            var polygon = GeometryFactory.CreatePolygon(newLinearRing, null);

            if (layer.CoordinateTransformation != null)
            {
                polygon = GeometryTransform.TransformPolygon(polygon, layer.CoordinateTransformation.MathTransform);
            }

            // Store original position to detect when feature has moved.
            polygon.UserData = feature.Geometry.Coordinates[0].Clone();
            return polygon;
        }

        

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clones the custom renderer. This allows the Network Editor to use custom renderers for
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new CompositeStructureRenderer(null);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // remove unnecessary references
            customGeometries.Clear();
            structureCounts.Clear();
        }

        #endregion
    }
}