using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
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
                    area2D.CollectionChanged -= HydroAreaOnCollectionChanged;
                }
            
                area2D = value;

                if (area2D != null)
                {
                    area2D.PropertyChanged += HydroAreaOnPropertyChanged;
                    area2D.CollectionChanged += HydroAreaOnCollectionChanged;
                }

                if (area2D != null && area2D.CoordinateSystem != previousCoordinateSystem)
                    OnCoordinateSystemChanged();
            }
        }

        private void HydroAreaOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(!(sender is IEventedList<Feature2D>) || !(Features is IEventedList<Feature2D>)) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.GetRemovedOrAddedItem() is ILeveeBreach leveeBreach)
                    {
                        CreatePointFeatureOfThisLeveeBreach(
                            leveeBreach,
                            LeveeBreachPointLocationType.BreachLocation,
                            leveeBreach.BreachLocation);
                        CreatePointFeatureOfThisLeveeBreach(
                            leveeBreach,
                            LeveeBreachPointLocationType.WaterLevelUpstreamLocation,
                            leveeBreach.WaterLevelUpstreamLocation);
                        CreatePointFeatureOfThisLeveeBreach(
                            leveeBreach,
                            LeveeBreachPointLocationType.WaterLevelDownstreamLocation,
                            leveeBreach.WaterLevelDownstreamLocation);
                    }

                }
                    break;
                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.GetRemovedOrAddedItem() is ILeveeBreach leveeBreach)
                    {
                        var supportPoint2DFeatures = ((IEventedList<Feature2D>)Features)
                            .Where(f2d => f2d.Attributes != null && 
                                          f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) && 
                                          ((ILeveeBreach)f2d.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE])
                                          .Equals(leveeBreach)).ToList();
                        foreach (var supportPoint2DFeature in supportPoint2DFeatures)
                        {
                            supportPoint2DFeature.PropertyChanged -= Feature2DPointOnPropertyChanged;
                            Features.Remove(supportPoint2DFeature);
                        }
                    }

                }

                    break;
                case NotifyCollectionChangedAction.Replace:
                {
                    if (e.OldItems is IEventedList<Feature2D> oldFeature2Ds)
                    {
                        foreach (var f2d in oldFeature2Ds)
                        {
                            if (f2d.Attributes != null &&
                                f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE))
                            {
                                f2d.PropertyChanged -= Feature2DPointOnPropertyChanged;
                            }
                        }
                    }


                    if (e.NewItems is IEventedList<Feature2D> newFeature2Ds)
                    {
                        foreach (var f2d in newFeature2Ds)
                        {
                            if (f2d.Attributes != null &&
                                f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE))
                            {
                                f2d.PropertyChanged += Feature2DPointOnPropertyChanged;
                            }
                        }

                        foreach (var leveeBreach in newFeature2Ds.OfType<ILeveeBreach>())
                        {
                            CreatePointFeatureOfThisLeveeBreach(
                                leveeBreach,
                                LeveeBreachPointLocationType.BreachLocation,
                                leveeBreach.BreachLocation);
                            CreatePointFeatureOfThisLeveeBreach(
                                leveeBreach,
                                LeveeBreachPointLocationType.WaterLevelUpstreamLocation,
                                leveeBreach.WaterLevelUpstreamLocation);
                            CreatePointFeatureOfThisLeveeBreach(
                                leveeBreach,
                                LeveeBreachPointLocationType.WaterLevelDownstreamLocation,
                                leveeBreach.WaterLevelDownstreamLocation);
                        }
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                {
                    if (e.OldItems is IEventedList<Feature2D> oldFeature2Ds)
                    {
                        foreach (var f2d in oldFeature2Ds)
                        {
                            if (f2d.Attributes != null &&
                                f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE))
                            {
                                f2d.PropertyChanged -= Feature2DPointOnPropertyChanged;
                            }
                        }
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreatePointFeatureOfThisLeveeBreach(ILeveeBreach leveeFeature, LeveeBreachPointLocationType leveeBreachPointLocationType, IGeometry leveeFeatureBreachLocation)
        {
            if (((IEventedList<Feature2D>)Features).SingleOrDefault(lpf =>
                    lpf.Attributes != null &&
                    lpf.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                    lpf.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                    lpf.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE].Equals(leveeFeature) &&
                    (LeveeBreachPointLocationType)lpf.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE] ==
                    leveeBreachPointLocationType) == null)
            {
                var feature2DPoint = new Feature2DPoint
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
                Features.Add(feature2DPoint);
            }
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
                        if (Math.Abs(leveeBreach.BreachLocationX - locationX) > tolerance)
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
        public override void Dispose()
        {
            var points2DFeatures = Features.OfType<Feature2D>().Where(f2d =>
                f2d.Attributes != null &&
                f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE));
            foreach (var points2DFeature in points2DFeatures)
            {
                points2DFeature.PropertyChanged -= Feature2DPointOnPropertyChanged;
            }
            base.Dispose();
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