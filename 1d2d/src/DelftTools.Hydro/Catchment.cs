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
    /// <summary>
    /// Represents a catchment entity in a hydrological system.
    /// </summary>
    [Entity]
    public class Catchment : Feature, ICopyFrom, IHydroObject, IComparable
    {
        private IPoint interiorPointCache;
        private bool settingDerivedGeometry;
        private CatchmentModelData modelData;

        /// <summary>
        /// Initializes a new instance of the <see cref="Catchment"/> class.
        /// </summary>
        public Catchment()
        {
            Name = "catchment";
            Attributes = new DictionaryFeatureAttributeCollection();
            Links = new EventedList<HydroLink>();
            CatchmentType = CatchmentType.None;
        }

        /// <summary>
        /// Gets or sets the model data of the catchment.
        /// </summary>
        /// <remarks>
        /// When setting a new value, we ensure that the catchment on this new model data is correctly pointing to this catchment instance.
        /// </remarks>
        public CatchmentModelData ModelData
        {
            get => modelData;
            set
            {
                if (modelData == value)
                {
                    return;
                }
                
                modelData = value;
                if (modelData != null)
                {
                    modelData.Catchment = this;
                }
                
            }
        }
        
        [DisplayName("Name")]
        [FeatureAttribute]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the long name of the catchment.
        /// </summary>
        [DisplayName("Long name")]
        [FeatureAttribute(Order = 3)]
        public virtual string LongName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the catchment type.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the description of the catchment.
        /// </summary>
        [FeatureAttribute(Order = 2)]
        public virtual string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the catchment's geometry is derived from its area size.
        /// </summary>
        public virtual bool IsGeometryDerivedFromAreaSize { get; set; } = true;

        /// <summary>
        /// Gets the interior point of the catchment.
        /// </summary>
        public virtual IPoint InteriorPoint
        {
            get
            {
                return interiorPointCache ?? (interiorPointCache = CalculateInteriorPoint());
            }
        }

        /// <summary>
        /// Gets the area size of the catchment's geometry.
        /// </summary>
        public virtual double GeometryArea
        {
            get
            {
                return Geometry?.Area ?? 0.0;
            }
        }

        /// <summary>
        /// Gets or sets the drainage basin associated with the catchment.
        /// </summary>
        [Aggregation]
        public virtual IDrainageBasin Basin { get; set; }

        /// <summary>
        /// Gets or sets the geometry of the catchment.
        /// </summary>
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

        /// <summary>
        /// Gets the region associated with the catchment.
        /// </summary>
        [Aggregation]
        public virtual IHydroRegion Region => Basin;

        /// <summary>
        /// Gets or sets the list of hydrological links associated with the catchment.
        /// </summary>
        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        /// <summary>
        /// Gets a value indicating whether the catchment can be a source of hydrological links.
        /// </summary>
        public virtual bool CanBeLinkSource => true;

        /// <summary>
        /// Gets a value indicating whether the catchment can be a target of hydrological links.
        /// </summary>
        public virtual bool CanBeLinkTarget
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the linking coordinate of the catchment.
        /// </summary>
        public virtual Coordinate LinkingCoordinate => InteriorPoint?.Coordinate;

        /// <summary>
        /// Sets the area size of the catchment.
        /// </summary>
        /// <param name="area">The new area size.</param>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IsGeometryDerivedFromAreaSize"/> is <c>false</c>.</exception>
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

        /// <summary>
        /// Creates a default catchment.
        /// </summary>
        /// <returns>The default catchment.</returns>
        public static Catchment CreateDefault()
        {
            var catchment = new Catchment();

            var factory = new GeometricShapeFactory
            {
                Centre = new Coordinate(20, 20),
                Size = 30
            };
            catchment.Geometry = factory.CreateCircle();
            catchment.IsGeometryDerivedFromAreaSize = true;

            return catchment;
        }

        /// <summary>
        /// Returns the string representation of the catchment.
        /// </summary>
        /// <returns>The name of the catchment.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Compares the catchment to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>
        /// A value indicating the relative order of the catchment compared to the specified object.
        /// </returns>
        public virtual int CompareTo(object obj)
        {
            if (obj is Catchment)
            {
                if (Equals(this, obj))
                {
                    return 0;
                }

                foreach (Catchment c in Basin.AllCatchments)
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

        /// <summary>
        /// Copies the values from the source object to the current instance.
        /// </summary>
        /// <param name="source">The source object to copy from.</param>
        public virtual void CopyFrom(object source)
        {
            var copyFrom = (Catchment)source;

            copyFrom.IsGeometryDerivedFromAreaSize = IsGeometryDerivedFromAreaSize;
            copyFrom.Attributes = (IFeatureAttributeCollection)Attributes.Clone();
            copyFrom.Basin = Basin;
            copyFrom.Description = Description;
            copyFrom.Links = new EventedList<HydroLink>(Links);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            var clone = new Catchment
            {
                Geometry = (IGeometry)Geometry?.Clone(),
                IsGeometryDerivedFromAreaSize = IsGeometryDerivedFromAreaSize,
                Name = Name,
                Attributes = (IFeatureAttributeCollection)Attributes.Clone(),
                Basin = Basin,
                Description = Description,
                Links = new EventedList<HydroLink>(Links),
                CatchmentType = CatchmentType,
                ModelData = (CatchmentModelData)ModelData?.Clone()
            };

            return clone;
        }

        /// <summary>
        /// Creates a hydrological link from the catchment to the specified target.
        /// </summary>
        /// <param name="target">The target object to link to.</param>
        /// <returns>The created hydrological link.</returns>
        public virtual HydroLink LinkTo(IHydroObject target)
        {
            HydroLink link = Region.AddNewLink(this, target);

            ModelData?.DoAfterLinking(target);

            return link;
        }

        /// <summary>
        /// Unlinks the catchment from the specified target object.
        /// </summary>
        /// <param name="target">The target object to unlink from.</param>
        public virtual void UnlinkFrom(IHydroObject target)
        {
            Region.RemoveLink(this, target);

            ModelData?.DoAfterUnlinking();
        }

        /// <summary>
        /// Determines whether the catchment can be linked to the specified target object.
        /// </summary>
        /// <param name="target">The target object to check for linkability.</param>
        /// <returns><c>True</c> if the catchment can be linked to the target object; otherwise, <c>false</c>.</returns>
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

        private IPoint CalculateInteriorPoint()
        {
            IPoint interiorPoint = CalculateInteriorPointCore();
            Links.ForEach(l =>
            {
                if (l.Geometry == null)
                {
                    return;
                }

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
            if (Geometry?.Coordinates == null)
            {
                return new Point(0, 0);
            }

            if (Geometry.Coordinates.Length > 150) //performance
            {
                return Geometry.Centroid;
            }

            if (Geometry.IsValid)
            {
                return Geometry.InteriorPoint;
            }

            return double.IsNaN(Geometry.Centroid.X) 
                       ? new Point(Geometry.Coordinate) 
                       : Geometry.Centroid;
        }

        private void UpdateDerivedGeometry(double newAreaSize, IPoint center = null)
        {
            center = center ?? GetCenter();

            settingDerivedGeometry = true;

            const double factorToAdjustForCircleApproximation = 1.0006582768034465388390272364538;
            newAreaSize *= factorToAdjustForCircleApproximation;

            double diameter = Math.Sqrt((4.0 * newAreaSize) / Math.PI);
            var factory = new GeometricShapeFactory
            {
                Centre = center.Coordinate,
                Size = diameter
            };
            Geometry = factory.CreateCircle();

            settingDerivedGeometry = false;
        }

        private IPoint GetCenter()
        {
            if (Geometry == null)
            {
                return new Point(0, 0);
            }

            return !double.IsNaN(Geometry.Envelope.Centroid.X) 
                       ? Geometry.Envelope.Centroid 
                       : new Point(Geometry.Coordinate);
        }
    }
}