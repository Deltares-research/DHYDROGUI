using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
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
        private VectorStyle waterLevelStreamStyle;

        public LeveeBreachRenderer(VectorStyle leveeStyle, VectorStyle breachStyle, VectorStyle waterLevelStreamStyle)
        {
            this.leveeStyle = leveeStyle;
            this.breachStyle = breachStyle;
            this.waterLevelStreamStyle = waterLevelStreamStyle;
        }

        public bool Render(IFeature feature, Graphics graphics, ILayer layer)
        {
            var leveeBreach = feature as ILeveeBreach;
            if (leveeBreach == null)
            {

                if (feature is Feature2DPoint feature2DPoint && RenderFeature2DPoint(graphics, layer, feature2DPoint)) return true;
                throw new InvalidOperationException("Cannot render incompatible feature, should be a Levee breach.");
            }

            // Draw line.
            var line = GetRenderedFeatureGeometry(feature, layer);
            VectorRenderingHelper.RenderGeometry(graphics, layer.Map, line, leveeStyle, null, true);
            return true;
        }

        private bool RenderFeature2DPoint(Graphics graphics, ILayer layer, Feature2DPoint feature2DPoint)
        {
            if (feature2DPoint.Attributes != null &&
                feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE] is LeveeBreach leveeFeature &&
                feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE))
            {
                var locationType = (LeveeBreachPointLocationType) feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE];
                switch (locationType)
                {
                    case LeveeBreachPointLocationType.BreachLocation:
                    {
                        // Draw breach location.
                        var pointLoc = GetTransformedGeometry(leveeFeature.BreachLocation, layer);
                        VectorRenderingHelper.RenderGeometry(graphics, layer.Map, pointLoc, breachStyle, null,
                            true);
                        return true;
                    }
                    case LeveeBreachPointLocationType.WaterLevelUpstreamLocation:
                    {
                        if (leveeFeature.WaterLevelFlowLocationsActive)
                        {
                            // Draw water level upstream point
                            var pointLoc = GetTransformedGeometry(leveeFeature.WaterLevelUpstreamLocation, layer);
                            var style = (VectorStyle) breachStyle.Clone();
                            style.Fill = new SolidBrush(Color.RoyalBlue);

                            // Draw water level upstream line.
                            VectorRenderingHelper.RenderGeometry(graphics, layer.Map, pointLoc, style, null,true);
                            var upLine = GetTransformedGeometry(
                                new LineString(new[]
                                {
                                    new Coordinate(leveeFeature.WaterLevelUpstreamLocationX, leveeFeature.WaterLevelUpstreamLocationY),
                                    new Coordinate(leveeFeature.BreachLocationX, leveeFeature.BreachLocationY)
                                }), layer);
                            VectorRenderingHelper.RenderGeometry(graphics, layer.Map, upLine, waterLevelStreamStyle, null, true);
                        }

                        return true;
                    }
                    case LeveeBreachPointLocationType.WaterLevelDownstreamLocation:
                    {
                        if (leveeFeature.WaterLevelFlowLocationsActive)
                        {
                            // Draw water level downstream point
                            var pointLoc = GetTransformedGeometry(leveeFeature.WaterLevelDownstreamLocation, layer);
                            var style = (VectorStyle) breachStyle.Clone();
                            style.Fill = new SolidBrush(Color.LightBlue);
                            VectorRenderingHelper.RenderGeometry(graphics, layer.Map, pointLoc, style,null, true);

                            // Draw water level downstream line. 
                            var downLine = GetTransformedGeometry(
                                new LineString(new[]
                                {
                                    new Coordinate(leveeFeature.BreachLocationX, leveeFeature.BreachLocationY),
                                    new Coordinate(leveeFeature.WaterLevelDownstreamLocationX, leveeFeature.WaterLevelDownstreamLocationY)
                                }), layer);
                            VectorRenderingHelper.RenderGeometry(graphics, layer.Map, downLine, waterLevelStreamStyle, null, true);
                        }

                        return true;
                    }
                }
            }

            return false;
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
            var leveeFeatures = layer.DataSource.Features.OfType<ILeveeBreach>();
            var leveePointFeatures = layer.DataSource.Features.OfType<Feature2DPoint>().ToList();
            var features = new List<IFeature>();
            foreach (var leveeFeature in leveeFeatures)
            {
                var extractBranchLocationPointFeatureOfThisLeveeBreach = ExtractPointFeatureOfThisLeveeBreach(geometry,
                    layer, leveeFeature, leveePointFeatures, LeveeBreachPointLocationType.BreachLocation,
                    leveeFeature.BreachLocation);
                if (extractBranchLocationPointFeatureOfThisLeveeBreach != null)
                    features.Add(extractBranchLocationPointFeatureOfThisLeveeBreach);
                var extractWaterLevelUpstreamPointFeatureOfThisLeveeBreach = ExtractPointFeatureOfThisLeveeBreach(
                    geometry, layer, leveeFeature, leveePointFeatures,
                    LeveeBreachPointLocationType.WaterLevelUpstreamLocation, leveeFeature.WaterLevelUpstreamLocation);
                if (extractWaterLevelUpstreamPointFeatureOfThisLeveeBreach != null)
                    features.Add(extractWaterLevelUpstreamPointFeatureOfThisLeveeBreach);
                var extractWaterLevelDownstreamPointFeatureOfThisLeveeBreach = ExtractPointFeatureOfThisLeveeBreach(
                    geometry, layer, leveeFeature, leveePointFeatures,
                    LeveeBreachPointLocationType.WaterLevelDownstreamLocation,
                    leveeFeature.WaterLevelDownstreamLocation);
                if (extractWaterLevelDownstreamPointFeatureOfThisLeveeBreach != null)
                    features.Add(extractWaterLevelDownstreamPointFeatureOfThisLeveeBreach);

            }

            foreach (var feature in GetFeatures(geometry.EnvelopeInternal, layer))
            {
                if (features.Contains(feature)) continue;
                features.Add(feature);
            }

            return features;
        }


        private IFeature ExtractPointFeatureOfThisLeveeBreach(IGeometry geometry, ILayer layer, ILeveeBreach leveeFeature, IEnumerable<Feature2DPoint> leveePointFeatures, LeveeBreachPointLocationType leveeBreachPointLocationType, IGeometry leveeFeatureBreachLocation)
        {
            if (leveeFeatureBreachLocation.Within(geometry))
            {
                var feature2DPoint = leveePointFeatures.FirstOrDefault(lpf =>
                    lpf.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                    lpf.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                    lpf.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE].Equals(leveeFeature) &&
                    (LeveeBreachPointLocationType) lpf.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE] ==
                    leveeBreachPointLocationType);
                if (feature2DPoint == null)
                {
                    feature2DPoint = new Feature2DPoint
                    {
                        Name = leveeFeature.Name + " : " + leveeBreachPointLocationType.GetDescription(),
                        Geometry = leveeFeatureBreachLocation,
                        Attributes = new DictionaryFeatureAttributeCollection()
                        {
                            {LeveeBreach.LEVEE_BREACH_FEATURE, leveeFeature},
                            {LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE, leveeBreachPointLocationType}
                        }
                    };
                    feature2DPoint.PropertyChanged += Feature2DPointOnPropertyChanged;
                    layer.DataSource.Features.Add(feature2DPoint);
                }
                return feature2DPoint;
            }
            return null;
        }


        private void Feature2DPointOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Feature2DPoint feature2DPoint &&
                feature2DPoint.Attributes != null &&
                feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE] is LeveeBreach leveeBreach &&
                feature2DPoint.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE))
            {
                var type = (LeveeBreachPointLocationType)feature2DPoint.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE];
                const double tolerance = 1e-5d;
                var locationX = feature2DPoint.X;
                var locationY = feature2DPoint.Y;
                switch (type)
                {
                    case LeveeBreachPointLocationType.BreachLocation:
                        if(Math.Abs(leveeBreach.BreachLocationX - locationX) > tolerance)
                            leveeBreach.BreachLocationX = locationX;
                        if (Math.Abs(leveeBreach.BreachLocationY - locationY) > tolerance)
                            leveeBreach.BreachLocationY = locationY;
                        break;
                    case LeveeBreachPointLocationType.WaterLevelUpstreamLocation:
                        if (Math.Abs(leveeBreach.WaterLevelUpstreamLocationX - locationX) > tolerance)
                            leveeBreach.WaterLevelUpstreamLocationX = locationX;
                        if (Math.Abs(leveeBreach.WaterLevelUpstreamLocationY - locationY) > tolerance)
                            leveeBreach.WaterLevelUpstreamLocationY = locationY;
                        break;
                    case LeveeBreachPointLocationType.WaterLevelDownstreamLocation:
                        
                        if (Math.Abs(leveeBreach.WaterLevelDownstreamLocationX - locationX) > tolerance)
                            leveeBreach.WaterLevelDownstreamLocationX = locationX;
                        if (Math.Abs(leveeBreach.WaterLevelDownstreamLocationY - locationY) > tolerance)
                            leveeBreach.WaterLevelDownstreamLocationY = locationY;
                        break;
                }
            }
        }

        public IEnumerable<IFeature> GetFeatures(Envelope box, ILayer layer)
        {
            return layer.GetFeatures(geometryFactory.ToGeometry(box), false);
        }
    }
}
