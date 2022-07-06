using System;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.NetworkEditor.Properties;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.LinearReferencing;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    /// <summary>
    /// Custom renderer to draw not geometry based cross sections. These cross sections are 
    /// drawn as a perpendicular line to the branch
    /// CrossSectionRenderer assumes it is always added to a vectorlayer. The vectorlayer renderer only calls
    /// a custom renderer if a theme is added to the layer:
    ///   networkLayer.CrossSectionLayer.CustomRenderers.Add(new CrossSectionRenderer());
    ///   networkLayer.CrossSectionLayer.Theme = new SharpMap.Rendering.Thematics.CustomTheme(null);
    /// Since the actual cross section geometries are stored in the cross section feature there is a problem
    /// when more views of the same network are opened.
    /// Support for ICloneable added to draw the custom features during drag operations
    /// note: custom rendering in progress:
    ///  - color for cross section types hard codes; should be based on style -> map legend?
    /// </summary>
    public class CrossSectionRenderer : BranchFeatureRenderer
    {
        private const double minimumPixelLength = 16.0;

        public override IGeometry GetRenderedFeatureGeometry(IFeature crossSection, ILayer layer)
        {
            var geometry = UseDefaultLength
                               ? GetDefaultGeometry(crossSection as ICrossSection)
                               : crossSection.Geometry;

            return layer.CoordinateTransformation != null
                       ? GeometryTransform.TransformGeometry(geometry, layer.CoordinateTransformation.MathTransform)
                       : geometry;
        }

        /// <summary>
        /// Called for each feature that needs to be rendered. CrossSectionRenderer assumes it is always 
        /// added to a vectorlayer.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="g"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public override bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            if (!(feature is ICrossSection))
            {
                return false; 
            }
            
            //Linestring outlines is drawn by drawing the layer once with a thicker line
            //before drawing the "inline" on top.
            vectorLayer = layer as VectorLayer;
            if (vectorLayer == null)
            {
                return false;
            }
            var crossSection = (ICrossSection) feature;
            bool themeOn = vectorLayer.Theme != null;

            var currentGeometry = GetRenderedFeatureGeometry(crossSection, vectorLayer);
            
            if (currentGeometry is IPoint point)
            {
                VectorRenderingHelper.DrawPoint(g, point, Resources.CrossSectionSmallWithExclamation, 1, new PointF(0, 0), 0, layer.Map);
                return true;
            }

            var pixelLength = GetPixelLength(currentGeometry);
            
            if (pixelLength < minimumPixelLength)
            {
                currentGeometry = GeometryTransform.Scale(currentGeometry, minimumPixelLength/pixelLength);
            }
            else if (pixelLength > 3*minimumPixelLength && 
                     crossSection.Branch != null)
            {
                var branchGeometry = crossSection.Branch.Geometry;
                var indexLine = new LengthIndexedLine(branchGeometry);
                var mapOffset = NetworkHelper.MapChainage(crossSection.Branch, crossSection.Chainage);
                var intersection = indexLine.ExtractPoint(mapOffset);
                    
                VectorRenderingHelper.DrawCircle(g, new Point(intersection), 5, Brushes.DarkSlateGray, layer.Map);
            }
            
            var currentVectorStyle = themeOn
                                ? vectorLayer.Theme.GetStyle(crossSection) as VectorStyle
                                : vectorLayer.Style;

            if (null == currentVectorStyle)
            {
                return false;
            }
            switch (crossSection.CrossSectionType)
            {
                case CrossSectionType.YZ:
                    currentVectorStyle.Line.Color = Color.Silver;
                    break;
                case CrossSectionType.ZW:
                    currentVectorStyle.Line.Color = Color.Gray;
                    break;
                case CrossSectionType.GeometryBased:
                    currentVectorStyle.Line.Color = Color.OrangeRed;
                    break;
                case CrossSectionType.Standard:
                    currentVectorStyle.Line.Color = Color.Purple;
                    break;
                default:
                    currentVectorStyle.Line.Color = Color.OrangeRed;
                    break;
            }
            if (vectorLayer.Style.EnableOutline)
            {
                //Draw background of all line-outlines first
                if (!themeOn ||
                    (currentVectorStyle != null && currentVectorStyle.Enabled &&
                     currentVectorStyle.EnableOutline))
                {
                    switch (currentGeometry.GeometryType)
                    {
                        case "LineString":
                            VectorRenderingHelper.DrawLineString(g, currentGeometry as ILineString,
                                                                 currentVectorStyle.Outline, layer.Map);
                            break;
                        case "MultiLineString":
                            VectorRenderingHelper.DrawMultiLineString(g, currentGeometry as IMultiLineString,
                                                                      currentVectorStyle.Outline, layer.Map);
                            break;
                        default:
                            break;
                    }
                }
            }

            VectorRenderingHelper.RenderGeometry(g, layer.Map , currentGeometry, currentVectorStyle, 
                null, vectorLayer.ClippingEnabled);
            return true;
        }

        internal IGeometry GetDefaultGeometry(ICrossSection crossSection)
        {
            if (crossSection.Branch == null || crossSection.GeometryBased)
            {
                return crossSection.Geometry;
            }

            var halfLength = DefaultLength / 2.0;
            return CrossSectionHelper.CreatePerpendicularGeometry(crossSection.Branch.Geometry, crossSection.Chainage, halfLength * -1, halfLength, 0);
        }

        public bool UseDefaultLength { get; set; }

        public double DefaultLength { get; set; } = 10;

        private double GetPixelLength(IGeometry currentGeometry)
        {
            if (currentGeometry.Coordinates.Length > 1)
            {
                var pixelStart = vectorLayer.Map.WorldToImage(currentGeometry.Coordinates.First());
                var pixelEnd = vectorLayer.Map.WorldToImage(currentGeometry.Coordinates.Last());
                var dX = pixelStart.X - pixelEnd.X;
                var dY = pixelStart.Y - pixelEnd.Y;
                return Math.Sqrt(dX*dX + dY*dY);
            }
            return 0.0;
        }
        
        /// <summary>
        /// Clones the custom renderer. This allows the Network Editor to use custom renderers for
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new CrossSectionRenderer();
        }

    }
}