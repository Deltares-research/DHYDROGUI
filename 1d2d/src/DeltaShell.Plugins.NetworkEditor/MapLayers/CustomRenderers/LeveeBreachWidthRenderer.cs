using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;
using SharpMap.Rendering;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers
{
    public class LeveeBreachWidthRenderer : FeatureCoverageRenderer
    {
        static IGeometryFactory geometryFactory = new GeometryFactory();

        public override bool Render(IFeature feature, Graphics g, ILayer layer)
        {
            // Mostly a copy of FeatureCoverageRenderer. 
            var coverageLayer = layer as FeatureCoverageLayer;
            if (coverageLayer == null) return false;

            var coverage = coverageLayer.FeatureCoverageToRender;
            var map = coverageLayer.Map;

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

                DrawFeatureCoverage(g, featureToRender, values[i], map, coverageLayer);
            }
            
            return true;
        }

        private void DrawFeatureCoverage(Graphics g, IFeature featureToRender, double value, IMap map, FeatureCoverageLayer coverageLayer)
        {
            var leveeBreach = featureToRender as ILeveeBreach;
            if (leveeBreach == null) return;
            var geometry = leveeBreach.Geometry;
            var leveeBreachPoint = new Coordinate(leveeBreach.BreachLocationX, leveeBreach.BreachLocationY);

            if (GeometryForFeatureDelegate != null)
            {
                geometry = GeometryForFeatureDelegate(featureToRender);
            }

            var lengthIndexedLine = new LengthIndexedLine(geometry);
            var offsetToBreach = lengthIndexedLine.Project(leveeBreachPoint);
            var breachWidth = value;

            // Breach end points
            var leftRunningBreachOffset = offsetToBreach - breachWidth * 0.5;
            var rightRunningBreachOffset = offsetToBreach + breachWidth * 0.5;

            var leftRunningBreachLocation = ExtractPoint(lengthIndexedLine, leftRunningBreachOffset);
            var rightRunningBreachLocation = ExtractPoint(lengthIndexedLine, rightRunningBreachOffset);

            // Breach line
            var breachLine = CreateBreachLine(leftRunningBreachLocation, rightRunningBreachLocation, lengthIndexedLine);

            // Transform
            if (coverageLayer.CoordinateTransformation != null)
            {
                leftRunningBreachLocation = GeometryTransform.TransformGeometry(leftRunningBreachLocation, coverageLayer.CoordinateTransformation.MathTransform);
                rightRunningBreachLocation = GeometryTransform.TransformGeometry(rightRunningBreachLocation, coverageLayer.CoordinateTransformation.MathTransform);
                breachLine = GeometryTransform.TransformGeometry(breachLine, coverageLayer.CoordinateTransformation.MathTransform);
            }

            // Render
            RenderBreachLine(g, map, breachLine, coverageLayer);
            RenderBreachEndPoints(g, map, leftRunningBreachLocation, coverageLayer, rightRunningBreachLocation);
        }

        private static void RenderBreachEndPoints(Graphics g, IMap map, IGeometry leftRunningBreachLocation, FeatureCoverageLayer coverageLayer, IGeometry rightRunningBreachLocation)
        {
            var pointStyle = AreaLayerStyles.BreachWidthPointStyle;
            VectorRenderingHelper.RenderGeometry(g, map, leftRunningBreachLocation, pointStyle, VectorLayer.DefaultPointSymbol, coverageLayer.ClippingEnabled);
            VectorRenderingHelper.RenderGeometry(g, map, rightRunningBreachLocation, pointStyle, VectorLayer.DefaultPointSymbol, coverageLayer.ClippingEnabled);
        }

        private static void RenderBreachLine(Graphics g, IMap map, IGeometry breachLine, FeatureCoverageLayer coverageLayer)
        {
            var lineStyle = AreaLayerStyles.BreachWidthLineStyle;
            VectorRenderingHelper.RenderGeometry(g, map, breachLine, lineStyle, VectorLayer.DefaultPointSymbol, coverageLayer.ClippingEnabled);
        }

        private static IGeometry CreateBreachLine(IGeometry leftRunningBreachLocation, IGeometry rightRunningBreachLocation, LengthIndexedLine lengthIndexedLine)
        {
            var leftCoordinate = new Coordinate(leftRunningBreachLocation.Coordinate.X, leftRunningBreachLocation.Coordinate.Y);
            var rightCoordinate = new Coordinate(rightRunningBreachLocation.Coordinate.X, rightRunningBreachLocation.Coordinate.Y);

            var leftRunningCoordinateIndex = lengthIndexedLine.IndexOf(leftCoordinate);
            var rightRunningCoordinateIndex = lengthIndexedLine.IndexOf(rightCoordinate);

            var breachLine = lengthIndexedLine.ExtractLine(leftRunningCoordinateIndex, rightRunningCoordinateIndex);
            return breachLine;
        }

        private static IGeometry ExtractPoint(LengthIndexedLine lengthIndexedLine, double offset)
        {
            var temp = lengthIndexedLine.ExtractPoint(offset);
            return new Point(temp.X, temp.Y);
        }
    }
}