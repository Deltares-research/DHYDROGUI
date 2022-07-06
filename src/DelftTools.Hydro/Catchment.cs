using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace DelftTools.Hydro
{
    [Entity]
    public class Catchment : Feature, ICopyFrom, IHydroObject, IComparable
    {
        private IPoint interiorPointCache;
        private string longName = String.Empty;
        private string description = String.Empty;
        private bool settingDerivedGeometry;

        public Catchment()
        {
            Name = "catchment";
            Attributes = new DictionaryFeatureAttributeCollection();
            Links = new EventedList<HydroLink>();
            CatchmentType = CatchmentType.None;
        }

        [DisplayName("Name")]
        [FeatureAttribute(Order = 1)]
        public virtual string Name { get; set; }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 3)]
        public virtual string LongName
        {
            get { return longName; }
            set { longName = value; }
        }

        [Aggregation]
        public virtual CatchmentType CatchmentType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CatchmentType"/>.
        /// </summary>
        /// <remarks>
        /// This changes the underlying <see cref="CatchmentType"/>.
        /// </remarks>
        [FeatureAttribute]
        [DisplayName("Type")]
        public virtual CatchmentTypes CatchmentTypes
        {
            get => CatchmentType.Types;
            set => CatchmentType = CatchmentType.LoadFromEnum(value);
        }

        [FeatureAttribute(Order = 2)]
        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual bool IsGeometryDerivedFromAreaSize { get; set; } = true;

        public override IGeometry Geometry
        {
            get
            {
                return base.Geometry;
            }
            set
            {
                if (value is IPoint point)
                {
                    UpdateDerivedGeometry(GeometryArea, point);
                    return;
                }

                base.Geometry = value is ILineString && value.Coordinates.Length >= 3 
                                    ? new Polygon(new LinearRing(value.Coordinates)) 
                                    : value;

                interiorPointCache = null;

                if (!settingDerivedGeometry)
                {
                    IsGeometryDerivedFromAreaSize = false;
                }
            }
        }

        public virtual IPoint InteriorPoint
        {
            get { return interiorPointCache ?? (interiorPointCache = CalculateInteriorPoint()); }
        }

        public virtual void SetAreaSize(double area)
        {
            if (IsGeometryDerivedFromAreaSize)
            {
                UpdateDerivedGeometry(area);
            }
            else
            {
                throw new InvalidOperationException("Cannot set area size when geometry not derived.");
            }
        }

        public virtual double GeometryArea
        {
            get { return Geometry?.Area ?? 0.0; }
        }

        public override object Clone()
        {
            var clone = new Catchment
                {
                    Geometry = (IGeometry)Geometry?.Clone(),
                    IsGeometryDerivedFromAreaSize = IsGeometryDerivedFromAreaSize,
                    Name = Name,
                    Attributes = (IFeatureAttributeCollection) Attributes.Clone(),
                    Basin = Basin,
                    Description = Description,
                    Links = new EventedList<HydroLink>(Links),
                    CatchmentType = CatchmentType
                };
            
            return clone;
        }

        public virtual void CopyFrom(object source)
        {
            var copyFrom = (Catchment) source;

            copyFrom.IsGeometryDerivedFromAreaSize = IsGeometryDerivedFromAreaSize;
            copyFrom.Attributes = (IFeatureAttributeCollection) Attributes.Clone();
            copyFrom.Basin = Basin;
            copyFrom.Description = Description;
            copyFrom.Links = new EventedList<HydroLink>(Links);
        }

        [Aggregation]
        public virtual IDrainageBasin Basin { get; set; }

        [Aggregation]
        public virtual IHydroRegion Region { get { return Basin; } }

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource { get { return true; } }

        public virtual bool CanBeLinkTarget { get { return false; } }
        public virtual Coordinate LinkingCoordinate => InteriorPoint?.Coordinate;

        public virtual HydroLink LinkTo(IHydroObject target)
        {
            return Region.AddNewLink(this, target);
        }

        public virtual void UnlinkFrom(IHydroObject target)
        {
            Region.RemoveLink(this, target);
        }

        public virtual bool CanLinkTo(IHydroObject target)
        {
            if (CatchmentType.Equals(CatchmentType.Paved))
            {
                return CanLinkPavedCatchmentTo(target);
            }

            return Region.CanLinkTo(this, target);
        }

        private bool CanLinkPavedCatchmentTo(IHydroObject target)
        {
            if (TargetIsOpenWaterCatchment(target))
            {
                return true;
            }

            if (AlreadyHasLinkToTargetType(target))
            {
                return false;
            }

            return Region.CanLinkTo(this, target);
        }

        private bool AlreadyHasLinkToTargetType(IHydroObject target)
        {
            if (!Links.Any())
            {
                return false;
            }

            switch (target)
            {
                case WasteWaterTreatmentPlant _:
                    return Links.Any(link => link.Target is WasteWaterTreatmentPlant);
                case LateralSource _:
                case RunoffBoundary _:
                {
                    return Links.Any(link => link.Target is LateralSource || link.Target is RunoffBoundary);
                }
            }

            return false;
        }

        private static bool TargetIsOpenWaterCatchment(IHydroObject target)
        {
            return target is Catchment targetCatchment 
                   && targetCatchment.CatchmentType.Equals(CatchmentType.OpenWater);
        }

        public static Catchment CreateDefault()
        {
            var catchment = new Catchment();

            var factory = new GeometricShapeFactory {Centre = new Coordinate(20, 20), Size = 30};
            catchment.Geometry = factory.CreateCircle();
            catchment.IsGeometryDerivedFromAreaSize = true;

            return catchment;
        }

        private IPoint CalculateInteriorPoint()
        {
            var interiorPoint = CalculateInteriorPointCore();
            Links.ForEach(l =>
            {
                if(l.Geometry == null) return;
                l.Geometry = new LineString(new[]
                {
                    interiorPoint.Coordinate,
                    l.Geometry.Coordinates.Last()
                });
            });
            return new Point(interiorPoint.X, interiorPoint.Y, 0); //if Z is NaN we get in trouble later
        }
        private IPoint CalculateInteriorPointCore()
        {
            //do not touch unless you know what you're doing!!
            if (Geometry?.Coordinates == null) return new Point(0,0);
            
            if (Geometry.Coordinates.Length > 150) //performance
            {
                return Geometry.Centroid;
            }
            return Geometry.IsValid
                       ? Geometry.InteriorPoint
                       : (double.IsNaN(Geometry.Centroid.X)
                              ? new Point(Geometry.Coordinate)
                              : Geometry.Centroid);
        }

        private void UpdateDerivedGeometry(double newAreaSize, IPoint center = null)
        {
            center = center ?? GetCenter();

            settingDerivedGeometry = true;

            const double factorToAdjustForCircleApproximation = 1.0006582768034465388390272364538;
            newAreaSize *= factorToAdjustForCircleApproximation;

            double diameter = Math.Sqrt(4.0*newAreaSize/Math.PI);
            var factory = new GeometricShapeFactory {Centre = center.Coordinate, Size = diameter};
            Geometry = factory.CreateCircle();

            settingDerivedGeometry = false;
        }

        private IPoint GetCenter()
        {
            return Geometry != null
                       ? (!double.IsNaN(Geometry.Envelope.Centroid.X)
                              ? Geometry.Envelope.Centroid
                              : new Point(Geometry.Coordinate))
                       : new Point(0, 0);
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual int CompareTo(object obj)
        {
            if (obj is Catchment)
            {
                if (Equals(this, obj))
                    return 0;

                foreach(var c in Basin.AllCatchments)
                {
                    if (Equals(c, this))
                    {
                        return -1;
                    }
                    if (Equals(c, obj))
                    {
                        return 1;
                    }
                }
            }
            else if (obj is WasteWaterTreatmentPlant)
            {
                return -1;
            }
            throw new InvalidOperationException();
        }
    }
}