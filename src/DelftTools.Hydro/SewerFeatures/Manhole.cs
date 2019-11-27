using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Manhole : Node, IManhole, ISewerFeature
    {
        private IEventedList<ICompartment> compartments;

        public Manhole(string manholeId) : base(manholeId)
        {
            Compartments = new EventedList<ICompartment>();
            Links = new EventedList<HydroLink>();
            Geometry = new Point(0, 0);
        }

        /// <summary>
        /// The x-coordinate of this Manhole. This is equal tot the average of the x-coordinates
        /// of its compartments.
        /// </summary>
        public double XCoordinate
        {
            get
            {
                var point = Geometry as IPoint;
                return point?.X ?? 0;
            }
        }

        /// <summary>
        /// The y-coordinate of this Manhole. This is equal tot the average of the y-coordinates
        /// of its compartments.
        /// </summary>
        public double YCoordinate
        {
            get
            {
                var point = Geometry as IPoint;
                return point?.Y ?? 0;
            }
        }

        public IEventedList<ICompartment> Compartments
        {
            get { return compartments; }
            set
            {
                if (compartments != null)
                {
                    compartments.CollectionChanged -= CompartmentCollectionChanged;
                    compartments.CollectionChanging -= CompartmentCollectionChanging;
                }

                compartments = value;
                UpdateGeometry();

                if (compartments != null)
                {
                    compartments.ForEach(comp => comp.ParentManhole = this);
                    compartments.CollectionChanged += CompartmentCollectionChanged;
                    compartments.CollectionChanging += CompartmentCollectionChanging;
                }
            }
        }

        public ICompartment GetCompartmentByName(string compartmentName)
        {
            return Compartments.FirstOrDefault(c => c.Name.Equals(compartmentName));
        }

        public bool ContainsCompartmentWithName(string compartmentName)
        {
            return Compartments != null && Compartments.Any(c => c.Name.Equals(compartmentName));
        }

        public IEnumerable<IFeature> GetPointFeatures()
        {
            return compartments
                .OfType<OutletCompartment>()
                .Cast<IFeature>()
                .Concat(this.InternalStructures());
        }

        public NetworkFeatureType NetworkFeatureType { get; } = NetworkFeatureType.Node;

        private void CompartmentCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            var compartment = e.Item as Compartment;
            if (compartment == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    if (Compartments.Select(c => c.Geometry).Contains(compartment.Geometry))
                    {
                        var newCoordinateX = compartment.Geometry.Coordinate.X + 1;
                        var newCoordinateY = compartment.Geometry.Coordinate.Y;
                        compartment.Geometry = new Point(newCoordinateX, newCoordinateY);
                    }
                    break;
            }
        }

        private void CompartmentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var compartment = e.GetRemovedOrAddedItem() as Compartment;
            if (compartment == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    //It has a NoNotifyPropertyChanged, so it won't propagate again.
                    compartment.ParentManhole = null;
                    return;
                case NotifyCollectionChangedAction.Add:
                    var oldParentManhole = compartment.ParentManhole;
                    if (oldParentManhole != null && oldParentManhole != this && oldParentManhole.ContainsCompartmentWithName(compartment.Name))
                    {
                        oldParentManhole.Compartments.Remove(compartment);
                    }

                    compartment.ParentManhole = this;
                    UpdateGeometry();
                    break;
            }
        }

        private void UpdateGeometry()
        {
            if(!compartments.Any()) return;

            var compartmentsPresentWithoutGeometry = compartments.Any(c => c.Geometry == null);
            if (compartmentsPresentWithoutGeometry)
                CopyGeometryToCompartments();
            else
                SetGeometryToAverageOfItsCompartments();
        }

        private void SetGeometryToAverageOfItsCompartments()
        {
            var averageXCoordinate = compartments.Select(c => c.Geometry.Coordinate.X).Average();
            var averageYCoordinate = compartments.Select(c => c.Geometry.Coordinate.Y).Average();
            Geometry = new Point(averageXCoordinate, averageYCoordinate);
        }

        protected override void OnGeometryChanged()
        {
            compartments?.ForEach(c => c.Geometry = Geometry);
        }

        private void CopyGeometryToCompartments()
        {
            compartments?.ForEach(c => c.Geometry = Geometry);
        }

        #region IHydroNetworkFeature

        public virtual IHydroRegion Region
        {
            get { return HydroNetwork; }
        }

        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource
        {
            get { return false; }
        }

        public virtual bool CanBeLinkTarget
        {
            get { return !IsConnectedToMultipleBranches; }
        }

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
            return Region.CanLinkTo(this, target);
        }

        public virtual IHydroNetwork HydroNetwork
        {
            get { return (IHydroNetwork) network; }
        }

        public virtual string LongName { get; set; }

        #endregion

        #region Network is visiting us
        [EditAction]
        public void AddToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}