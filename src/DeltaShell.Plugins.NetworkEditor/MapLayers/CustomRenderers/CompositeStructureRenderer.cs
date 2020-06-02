using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Converters.Geometries;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;
using SharpMap.Rendering;

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
        private readonly StructureRenderer structureRenderer;
        private Envelope lastEnvelope;

        public CompositeStructureRenderer(StructureRenderer structureRenderer)
        {
            this.structureRenderer = structureRenderer;
        }

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
        public bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            if (null == lastEnvelope
                || lastEnvelope.Width != layer.Map.Envelope.Width
                || lastEnvelope.Height != layer.Map.Envelope.Height)
            {
                lastEnvelope = layer.Map.Envelope.Clone();
                customGeometries.Clear();
                structureCounts.Clear();
            }

            var compositeStructure = (ICompositeBranchStructure) feature;
            if (compositeStructure.Structures.Count <= 1)
            {
                return true;
            }

            var vectorLayer = (VectorLayer) layer;

            customGeometries[feature] = GetRenderedFeatureGeometry(feature, vectorLayer);
            structureCounts[feature] = compositeStructure.Structures.Count;

            var polygon = (IPolygon) customGeometries[feature];
            VectorRenderingHelper.DrawPolygon(g, polygon, Brushes.GreenYellow, Pens.Red, false, vectorLayer.Map);
            return true;
        }

        private static IGeometry GenerateCustomGeometry(IFeature feature, VectorLayer layer)
        {
            var compositeStructure = (ICompositeBranchStructure) feature;
            Coordinate org = layer.Map.ImageToWorld(new PointF(0, 0));
            Coordinate range = layer.Map.ImageToWorld(new PointF(layer.Style.Symbol.Width * compositeStructure.Structures.GroupBy(s => s.GetType()).Count(), layer.Style.Symbol.Height));
            Coordinate anchor = feature.Geometry.Coordinates[0];

            double width = range.X - org.X;
            double halfHeight = (range.Y - org.Y) / 2;

            var vertices = new List<Coordinate>
            {
                new Coordinate(anchor.X - (width / 2), anchor.Y + (2 * halfHeight)),
                new Coordinate(anchor.X - (width / 2), anchor.Y + (1 * halfHeight)),
                new Coordinate(anchor.X + (width / 2), anchor.Y + (1 * halfHeight)),
                new Coordinate(anchor.X + (width / 2), anchor.Y + (2 * halfHeight))
            };

            vertices.Add((Coordinate) vertices[0].Clone());

            ILinearRing newLinearRing = GeometryFactory.CreateLinearRing(vertices.ToArray());
            IPolygon polygon = GeometryFactory.CreatePolygon(newLinearRing, null);

            if (layer.CoordinateTransformation != null)
            {
                polygon = GeometryTransform.TransformPolygon(polygon, layer.CoordinateTransformation.MathTransform);
            }

            // Store original position to detect when feature has moved.
            polygon.UserData = feature.Geometry.Coordinates[0].Clone();
            return polygon;
        }

        public virtual IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            var intersectedFeatures = new List<IFeature>();

            foreach (IFeature feature in layer.DataSource.Features)
            {
                IGeometry geometry = GetRenderedFeatureGeometry(feature, layer);
                if (geometry.EnvelopeInternal.Intersects(box))
                {
                    intersectedFeatures.Add(feature);
                }
            }

            return intersectedFeatures;
        }

        public IEnumerable<IFeature> GetFeatures(IGeometry geometry, ILayer layer)
        {
            return GetFeatures(geometry.EnvelopeInternal, layer);
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            var compositeStructure = (ICompositeBranchStructure) feature;
            IGeometry geometry;

            if (!customGeometries.ContainsKey(feature))
            {
                if (null != structureRenderer)
                {
                    foreach (BranchStructure structure in compositeStructure.Structures)
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
                if (!feature.Geometry.Coordinates[0].Equals2D(oldCoordinate))
                {
                    geometry = GenerateCustomGeometry(feature, (VectorLayer) layer);
                }
                else if (structureCounts[feature] != compositeStructure.Structures.Count)
                {
                    if (null != structureRenderer)
                    {
                        foreach (IStructure1D structure in compositeStructure.Structures)
                        {
                            structureRenderer.InvalidateStructure(structure);
                        }
                    }

                    geometry = GenerateCustomGeometry(feature, (VectorLayer) layer);
                }
            }

            customGeometries[feature] = geometry;
            structureCounts[feature] = compositeStructure.Structures.Count;
            return geometry;
        }

        #endregion
    }
}