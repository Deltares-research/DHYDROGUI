using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Converters.Geometries;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class StructureRenderer : IFeatureRenderer, ICloneable, IDisposable
    {
        private readonly IDictionary<IFeature, IGeometry> customGeometries = new Dictionary<IFeature, IGeometry>();
        private Envelope lastEnvelope;

        #region IFeatureRenderer Members
        
        /// <summary>
        /// Renders a structure that can be connected.
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
            if ((null == lastEnvelope)
                || (lastEnvelope.Width != layer.Map.Envelope.Width)
                || (lastEnvelope.Height != layer.Map.Envelope.Height))
            {
                lastEnvelope = layer.Map.Envelope.Clone();
                customGeometries.Clear();
            }

            var vectorLayer = (VectorLayer)layer;
            customGeometries[feature] = GetRenderedFeatureGeometry(feature, layer);
            
            var polygon = (IPolygon)customGeometries[feature];
            var point = GeometryFactory.CreatePoint(polygon.EnvelopeInternal.Centre);

            var style = GetCurrentStyle(vectorLayer,feature);
            
            var structure = feature as IStructure1D;
            var styleSymbol = (Bitmap)style.Symbol.Clone();

            if (structure != null && structure.ParentStructure != null)
            {
                var numberOfSameStructures = structure.ParentStructure.Structures.Count(s => s.GetType() == structure.GetType());
                if (numberOfSameStructures != 1)
                {
                    using (var graphics = Graphics.FromImage(styleSymbol))
                    using (var font = new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular))
                    {
                        var numbOfStructuresString = numberOfSameStructures.ToString();
                        var messageSize = graphics.MeasureString(numbOfStructuresString, font);

                        using (var blackBrush = new SolidBrush(Color.Black))
                        using (var whiteBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                        using (var blackPen = new Pen(Color.Black))
                        {
                            graphics.FillEllipse(whiteBrush, new RectangleF(new PointF(0, 0), messageSize));
                            graphics.DrawEllipse(blackPen, new RectangleF(new PointF(0, 0), messageSize));
                            graphics.DrawString(numbOfStructuresString, font, blackBrush, 0, 0);
                        }
                    }
                }
            }

            VectorRenderingHelper.DrawPoint(g, point, styleSymbol, style.SymbolScale, style.SymbolOffset, style.SymbolRotation, layer.Map);
            return true;
        }

        private static VectorStyle GetCurrentStyle(VectorLayer vectorLayer,IFeature feature)
        {
            return (VectorStyle) (vectorLayer.Theme != null ? vectorLayer.Theme.GetStyle(feature) : vectorLayer.Style);
        }

        /// <summary>
        /// Calculates a geometry for structuce that is an offset in StructureFeature
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        private IGeometry GenerateCustomGeometry(IFeature feature, VectorLayer layer)
        {
            var structure = (IStructure1D)feature;
            var compositeStructure = structure.ParentStructure;

            // structure can be disconnected during
            var index = 0;
            var structureFeatureCount = 1;
            if (null != compositeStructure)
            {
                var grouping = compositeStructure.Structures.GroupBy(s => s.GetType());
                var structureGrouping = grouping.Select(t => t.Key).ToList();
                index = structureGrouping.IndexOf(structure.GetType());
                structureFeatureCount = structureGrouping.Count;
            }

            return GenerateCustomGeometry(feature, layer, index, structureFeatureCount);
        }

        private static IGeometry GenerateCustomGeometry(IFeature feature, VectorLayer layer, int index, int structureFeatureCount)
        {
            var org = layer.Map.ImageToWorld(new PointF(0, 0));
            var style = GetCurrentStyle(layer,feature);

            var range = layer.Map.ImageToWorld(new PointF(style.Symbol.Width, style.Symbol.Height));
            var anchor = feature.Geometry.Coordinates[0];
            
            var halfWidth = (range.X - org.X) / 2;
            var d = anchor.X - (halfWidth * (structureFeatureCount - 1)) + (2 * halfWidth * index) - halfWidth;

            var vertices = new List<Coordinate>();
            
            var halfHeight = (range.Y - org.Y) / 2;
            vertices.Add(new Coordinate(d, anchor.Y - 1 * halfHeight));
            vertices.Add(new Coordinate(d, anchor.Y + 1 * halfHeight));
            vertices.Add(new Coordinate(d + (2 * halfWidth), anchor.Y + 1 * halfHeight));
            vertices.Add(new Coordinate(d + (2 * halfWidth), anchor.Y - 1 * halfHeight));
            vertices.Add((Coordinate)vertices[0].Clone());
            
            var newLinearRing = GeometryFactory.CreateLinearRing(vertices.ToArray());
            var polygon = GeometryFactory.CreatePolygon(newLinearRing, null);

            if (layer.CoordinateTransformation != null)
            {
                polygon = GeometryTransform.TransformPolygon(polygon, layer.CoordinateTransformation.MathTransform);
            }

            // Store original position to detect when feature has moved.
            polygon.UserData = polygon.EnvelopeInternal.Centre.Clone();
            return polygon;
        }

        public IGeometry GetRenderedFeatureGeometry(IFeature feature, ILayer layer)
        {
            IGeometry geometry;
            if (!customGeometries.ContainsKey(feature))
            {
                geometry = GenerateCustomGeometry(feature, (VectorLayer)layer);
            }
            else
            {
                // Update the geometry if feature has moved
                geometry = customGeometries[feature];
                var oldCoordinate = (Coordinate)geometry.UserData;

                if ((!feature.Geometry.Coordinates[0].Equals2D(oldCoordinate)))
                {
                    geometry = GenerateCustomGeometry(feature, (VectorLayer)layer);
                }
            }

            return geometry;
        }

        public IEnumerable<IFeature> GetFeatures(IGeometry geometry, ILayer layer)
        {
            return GetFeatures(geometry.EnvelopeInternal, layer);
        }

        public IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            var intersectedFeatures = new List<IFeature>();

            foreach (IFeature feature in layer.DataSource.Features)
            {
                var geometry = GetRenderedFeatureGeometry(feature, layer);
                if (geometry.EnvelopeInternal.Intersects(box))
                {
                    intersectedFeatures.Add(feature);
                }
            }

            return intersectedFeatures;
        }

        #endregion

        #region ICloneable Members
        public object Clone()
        {
            return new StructureRenderer();
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // remove unnecessary references
            customGeometries.Clear();
        }
        #endregion

        public void InvalidateStructure(IStructure1D structure)
        {
            if (customGeometries.ContainsKey(structure))
                customGeometries.Remove(structure);
        }
        public void Reset()
        {
            customGeometries.Clear();
        }
    }
}