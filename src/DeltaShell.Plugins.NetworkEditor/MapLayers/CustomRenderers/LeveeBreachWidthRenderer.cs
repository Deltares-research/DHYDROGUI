using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class LeveeBreachWidthRenderer : FeatureCoverageRenderer
    {
        static IGeometryFactory geometryFactory = new GeometryFactory();

        public override bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            // Mostly a copy of FeatureCoverageRenderer. Only the rendering itself will be different
            var coverageLayer = layer as FeatureCoverageLayer;
            if (coverageLayer == null) return false;

            var coverage = coverageLayer.FeatureCoverageToRender;
            var map = coverageLayer.Map;

            // No theme? Can not render?
            if (coverageLayer.Theme == null) return false;

            // Features to render
            IList featuresToRender;
            IFeature[] coverageFeatures;
            double[] values;

            lock (coverage.Store)
            {
                featuresToRender = coverageLayer.GetFeatures(geometryFactory.ToGeometry(map.Envelope), false).ToArray();
                if (featuresToRender.Count <= 0) return true;

                values = coverage.Components[0].Values.Cast<double>().ToArray();
                if (values.Length == 0)
                {
                    coverageFeatures = new IFeature[] { };
                }
                else
                {
                    coverageFeatures = coverage.FeatureVariable.Values.Cast<IFeature>().ToArray();
                }
            }

            for (var i = 0; i < coverageFeatures.Length; ++i)
            {
                var featureToRender = coverageFeatures[i];
                if (!featuresToRender.Contains(featureToRender)) continue;

                var leveeBreach = featureToRender as LeveeBreach;
                if (leveeBreach == null) continue;
                var geometry = leveeBreach.Geometry;
                var leveeBreachPoint = new Coordinate(leveeBreach.BreachLocationX, leveeBreach.BreachLocationY);

                if (GeometryForFeatureDelegate != null)
                {
                    geometry = GeometryForFeatureDelegate(featureToRender);
                }
                // TODO Sil Cleanup this method
                //var style = AreaK //coverageLayer.Theme.GetStyle(values[i]) as VectorStyle;
                //if (style != null)
                //{
                //style.SymbolScale = 0.75F;
                //style.Line.Width = 4.0F;

                var lengthIndexedLine = new LengthIndexedLine(geometry);
                var offsetToBreach = lengthIndexedLine.Project(leveeBreachPoint);
                var breachWidth = values[i];

                // Breach end points
                var leftRunningBreachOffset = offsetToBreach - breachWidth * 0.5;
                var rightRunningBreachOffset = offsetToBreach + breachWidth * 0.5;

                var leftRunningBreachLocation = ExtractPoint(lengthIndexedLine, leftRunningBreachOffset);
                var rightRunningBreachLocation = ExtractPoint(lengthIndexedLine, rightRunningBreachOffset);

                // Breach line
                var leftCoordinate = new Coordinate(leftRunningBreachLocation.Coordinate.X, leftRunningBreachLocation.Coordinate.Y);
                var rightCoordinate = new Coordinate(rightRunningBreachLocation.Coordinate.X, rightRunningBreachLocation.Coordinate.Y);

                var leftRunningCoordinateIndex = lengthIndexedLine.IndexOf(leftCoordinate);
                var rightRunningCoordinateIndex = lengthIndexedLine.IndexOf(rightCoordinate);

                var breachLine = lengthIndexedLine.ExtractLine(leftRunningCoordinateIndex, rightRunningCoordinateIndex);

                // Transform
                if (layer.CoordinateTransformation != null)
                {

                    leftRunningBreachLocation = GeometryTransform.TransformGeometry(leftRunningBreachLocation, layer.CoordinateTransformation.MathTransform);
                    rightRunningBreachLocation = GeometryTransform.TransformGeometry(rightRunningBreachLocation, layer.CoordinateTransformation.MathTransform);
                    breachLine = GeometryTransform.TransformGeometry(breachLine, layer.CoordinateTransformation.MathTransform);
                }

                // Render
                var pointStyle = AreaLayerStyles.BreachWidthPointStyle;
                var lineStyle = AreaLayerStyles.BreachWidthLineStyle;
                VectorRenderingHelper.RenderGeometry(g, map, breachLine, lineStyle, VectorLayer.DefaultPointSymbol, coverageLayer.ClippingEnabled);
                VectorRenderingHelper.RenderGeometry(g, map, leftRunningBreachLocation, pointStyle, VectorLayer.DefaultPointSymbol, coverageLayer.ClippingEnabled);
                VectorRenderingHelper.RenderGeometry(g, map, rightRunningBreachLocation, pointStyle, VectorLayer.DefaultPointSymbol, coverageLayer.ClippingEnabled);
            }
            
            return true;
        }

        private static IGeometry ExtractPoint(LengthIndexedLine lengthIndexedLine, double offset)
        {
            var temp = lengthIndexedLine.ExtractPoint(offset);
            return new Point(temp.X, temp.Y);
        }
    }
}