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

        public Catchment()
        {
            Name = "catchment";
            Attributes = new DictionaryFeatureAttributeCollection();
            Links = new EventedList<HydroLink>();
            SubCatchments = new EventedList<Catchment>();
            CatchmentType = CatchmentType.None;
        }

        public virtual IEventedList<Catchment> SubCatchments { get; set; }

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
        [DisplayName("Type")]
        [FeatureAttribute]
        public virtual CatchmentType CatchmentType { get; set; }

        [FeatureAttribute(Order = 2)]
        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual bool IsGeometryDerivedFromAreaSize { get; set; }

        public override IGeometry Geometry
        {
            get
            {
                return base.Geometry;
            }
            set
            {
                if (Equals(CatchmentType, CatchmentType.NWRW) && value is IPoint point)
                {
                    UpdateDerivedGeometry(AreaSize, point);
                }
                else
                {
                    base.Geometry = value is ILineString && value.Coordinates.Length >= 3 
                                        ? new Polygon(new LinearRing(value.Coordinates)) 
                                        : value;
                }
                interiorPointCache = null;
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

        public virtual double AreaSize
        {
            get { return Geometry != null ? Geometry.Area : 0.0; }
        }

        public override object Clone()
        {
            var clone = new Catchment
                {
                    Geometry = Geometry != null ? (IGeometry) Geometry.Clone() : null,
                    IsGeometryDerivedFromAreaSize = IsGeometryDerivedFromAreaSize,
                    Name = Name,
                    Attributes = (IFeatureAttributeCollection) Attributes.Clone(),
                    Basin = Basin,
                    Description = Description,
                    Links = new EventedList<HydroLink>(Links),
                    SubCatchments = new EventedList<Catchment>(SubCatchments.Select(sc => (Catchment) sc.Clone())),
                    CatchmentType = CatchmentType // hopefully it is static for now, TODO: extend when dynamic catchment types are added
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

        public virtual bool CanBeLinkSource { get { return !CatchmentType.SubCatchmentTypes.Any(); } }

        public virtual bool CanBeLinkTarget { get { return false; } }

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
            if (CatchmentType.Equals(CatchmentType.Paved)
                && target is Catchment targetCatchment 
                && targetCatchment.CatchmentType.Equals(CatchmentType.OpenWater))
            {
                return true;
            }

            return Region.CanLinkTo(this, target);
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

        private void UpdateDerivedGeometry(double newAreaSize)
        {
            IPoint center = GetCenter();
            UpdateDerivedGeometry(newAreaSize, center);
        }
        private void UpdateDerivedGeometry(double newAreaSize, IPoint center)
        {
            const double factorToAdjustForCircleApproximation = 1.0006582768034465388390272364538;
            newAreaSize *= factorToAdjustForCircleApproximation;

            double diameter = Math.Sqrt(4.0*newAreaSize/Math.PI);
            var factory = new GeometricShapeFactory {Centre = center.Coordinate, Size = diameter};
            Geometry = factory.CreateCircle();
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
        
        //nhib ;-) \-)
        [NoNotifyPropertyChange]
        protected virtual string CatchmentTypeString
        {
            get { return CatchmentType != null ? CatchmentType.Name : CatchmentType.PavedTypeName; }
            set { CatchmentType = CatchmentType.LoadFromString(value); }
        }
        
        public virtual Catchment AddSubCatchment(CatchmentType catchmentType)
        {
            if (!CatchmentType.SubCatchmentTypes.Contains(catchmentType))
                throw new InvalidOperationException("This catchment cannot have sub catchments of given type");

            var delta = new Coordinate(0, 0);
            var offset = 100;
            switch (catchmentType.Name)
            {
                case CatchmentType.PavedTypeName:
                    delta = new Coordinate(-offset, 0);
                    break;
                case CatchmentType.UnpavedTypeName:
                    delta = new Coordinate(0, offset);
                    break;
                case CatchmentType.GreenhouseTypeName:
                    delta = new Coordinate(offset, 0);
                    break;
                case CatchmentType.OpenwaterTypeName:
                    delta = new Coordinate(0, -offset);
                    break;
                default:
                    throw new NotSupportedException("Unknow type to render as part of polder concept");
            }

            var geometry = new Point(InteriorPoint.X + delta.X, InteriorPoint.Y + delta.Y);

            var subCatchment = new Catchment
                {
                    Name = Name + "_" + catchmentType,
                    LongName = LongName + "_" + catchmentType, 
                    CatchmentType = catchmentType, 
                    Geometry = geometry, 
                    Basin = Basin
                };
            SubCatchments.Add(subCatchment);
            return subCatchment;
        }
    }
}