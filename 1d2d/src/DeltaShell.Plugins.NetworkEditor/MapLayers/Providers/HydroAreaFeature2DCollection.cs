using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Features;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Providers
{
    public class HydroAreaFeature2DCollection : Feature2DCollection
    {
        private HydroArea area2D;

        public HydroAreaFeature2DCollection(HydroArea area2D)
        {
            Area2D = area2D;
            
            
        }

        

        public override ICoordinateSystem CoordinateSystem
        {
            get { return Area2D != null ? Area2D.CoordinateSystem : null; }
            set { } 
        }

        private HydroArea Area2D
        {
            get { return area2D; }
            set
            {
                var previousCoordinateSystem = CoordinateSystem;
                if (area2D != null)
                {
                    area2D.PropertyChanged -= HydroAreaOnPropertyChanged;
                }
            
                area2D = value;

                if (area2D != null)
                {
                    area2D.PropertyChanged += HydroAreaOnPropertyChanged;
                }

                if (area2D != null && area2D.CoordinateSystem != previousCoordinateSystem)
                    OnCoordinateSystemChanged();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            AddNewFeatureFromGeometryDelegate = null;
            Area2D = null;
        }

        private void HydroAreaOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ILeveeBreach leveeBreach &&
                (e.PropertyName == nameof(ILeveeBreach.BreachLocationX) ||
                e.PropertyName == nameof(ILeveeBreach.BreachLocationY) ||
                e.PropertyName == nameof(ILeveeBreach.WaterLevelUpstreamLocationX) ||
                e.PropertyName == nameof(ILeveeBreach.WaterLevelUpstreamLocationY) ||
                e.PropertyName == nameof(ILeveeBreach.WaterLevelDownstreamLocationX) ||
                e.PropertyName == nameof(ILeveeBreach.WaterLevelDownstreamLocationY) ||
                e.PropertyName == nameof(ILeveeBreach.WaterLevelFlowLocationsActive) ||
                e.PropertyName == nameof(ILeveeBreach.Name)))
            {
                FireFeaturesChanged();
                if (e.PropertyName == nameof(ILeveeBreach.Name))
                {
                    var feature2DPoints = Features.OfType<Feature2D>().Where(f2d =>
                        f2d.Attributes != null &&
                        f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                        ((ILeveeBreach) f2d.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE]).Equals(leveeBreach) &&
                        f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE));
                    foreach (var feature2DPoint in feature2DPoints)
                    {
                        var leveeBreachPointLocationType =
                            (LeveeBreachPointLocationType) feature2DPoint.Attributes[
                                LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE];
                        var leveePointLocationName = leveeBreach.Name + " : " + leveeBreachPointLocationType.GetDescription();
                        if(feature2DPoint.Name != leveePointLocationName)
                            feature2DPoint.Name = leveePointLocationName;
                    }
                }


                const double tolerance = 1e-5d;
                if (e.PropertyName == nameof(ILeveeBreach.BreachLocationX) ||
                   e.PropertyName == nameof(ILeveeBreach.BreachLocationY))
                {
                    var feature2DPoint = Features.OfType<Feature2D>().SingleOrDefault(f2d =>
                    f2d.Attributes != null && 
                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                    ((ILeveeBreach) f2d.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE]).Equals(leveeBreach) &&
                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                    ((LeveeBreachPointLocationType)f2d.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE]).Equals(LeveeBreachPointLocationType.BreachLocation));
                    if (feature2DPoint != null)
                    {
                        if (Math.Abs(leveeBreach.BreachLocationX - feature2DPoint.Geometry.Coordinate.X) > tolerance)
                            feature2DPoint.Geometry.Coordinate.X = leveeBreach.BreachLocationX;
                        if (Math.Abs(leveeBreach.BreachLocationY - feature2DPoint.Geometry.Coordinate.Y) > tolerance)
                            feature2DPoint.Geometry.Coordinate.Y = leveeBreach.BreachLocationY;
                    }
                }
                if (e.PropertyName == nameof(ILeveeBreach.WaterLevelUpstreamLocationX) ||
                   e.PropertyName == nameof(ILeveeBreach.WaterLevelUpstreamLocationY))
                {
                    var feature2DPoint = Features.OfType<Feature2D>().SingleOrDefault(f2d =>
                    f2d.Attributes != null && 
                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                    ((ILeveeBreach) f2d.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE]).Equals(leveeBreach) &&
                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                    ((LeveeBreachPointLocationType)f2d.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE]).Equals(LeveeBreachPointLocationType.WaterLevelUpstreamLocation));
                    if (feature2DPoint != null)
                    {
                        if (Math.Abs(leveeBreach.WaterLevelUpstreamLocationX - feature2DPoint.Geometry.Coordinate.X) > tolerance)
                            feature2DPoint.Geometry.Coordinate.X = leveeBreach.WaterLevelUpstreamLocationX;
                        if (Math.Abs(leveeBreach.WaterLevelUpstreamLocationY - feature2DPoint.Geometry.Coordinate.Y) > tolerance)
                            feature2DPoint.Geometry.Coordinate.Y = leveeBreach.WaterLevelUpstreamLocationY;
                    }
                }
                if (e.PropertyName == nameof(ILeveeBreach.WaterLevelDownstreamLocationX) ||
                   e.PropertyName == nameof(ILeveeBreach.WaterLevelDownstreamLocationY))
                {
                    var feature2DPoint = Features.OfType<Feature2D>().SingleOrDefault(f2d =>
                    f2d.Attributes != null && 
                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                    ((ILeveeBreach) f2d.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE]).Equals(leveeBreach) &&
                    f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                    ((LeveeBreachPointLocationType)f2d.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE]).Equals(LeveeBreachPointLocationType.WaterLevelDownstreamLocation));
                    if (feature2DPoint != null)
                    {
                        if (Math.Abs(leveeBreach.WaterLevelDownstreamLocationX - feature2DPoint.Geometry.Coordinate.X) > tolerance)
                            feature2DPoint.Geometry.Coordinate.X = leveeBreach.WaterLevelDownstreamLocationX;
                        if (Math.Abs(leveeBreach.WaterLevelDownstreamLocationY - feature2DPoint.Geometry.Coordinate.Y) > tolerance)
                            feature2DPoint.Geometry.Coordinate.Y = leveeBreach.WaterLevelDownstreamLocationY;

                    }
                }
            }
            if (e.PropertyName != "CoordinateSystem") return;
            OnCoordinateSystemChanged();
        }
    }
}