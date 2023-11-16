using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Manhole : Node, IManhole, ISewerFeature, IItemContainer
    {
        private IEventedList<ICompartment> compartments;

        public Manhole():this("Manhole")
        {
        }
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
        [DisplayName("X coordinate")]
        [FeatureAttribute(Order = 5)]
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
        [DisplayName("Y coordinate")]
        [FeatureAttribute(Order = 5)]
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


        /// <summary>
        /// Get an internal compartment as candidate for an outlet
        /// </summary>
        /// <returns></returns>
        public ICompartment GetOutletCandidate()
        {
            foreach (var c in Compartments)
            {
                if (IncomingBranches.OfType<ISewerConnection>().Any(sw => sw.TargetCompartment == c) && OutgoingBranches.OfType<ISewerConnection>().All(sw => sw.SourceCompartment != c))
                {
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// Upgrade an internal component to outlet
        /// </summary>
        /// <param name="compartment"></param>
        /// <returns></returns>
        public OutletCompartment UpdateCompartmentToOutletCompartment(ICompartment compartment)
        {
            var outlet = new OutletCompartment(compartment);
            outlet.TakeConnectionsOverFrom(compartment);

            lock (Compartments)
            {
                Compartments.Remove(compartment);
                Compartments.Add(outlet);
            }

            var incomingBranches = IncomingBranches;
            incomingBranches
                .OfType<ISewerConnection>()
                .Where(sw => sw.TargetCompartment == compartment)
                .ForEach(sw => sw.TargetCompartment = outlet);
            IncomingBranches = incomingBranches;
            var outgoingBranches = OutgoingBranches;
            outgoingBranches //should not be the case: outlet with outgoing connection
                .OfType<ISewerConnection>()
                .Where(sw => sw.SourceCompartment == compartment)
                .ForEach(sw => sw.SourceCompartment = outlet);
            OutgoingBranches = outgoingBranches;

            return outlet;
        }
        
        /// <summary>
        /// Downgrade an internal outlet to compartment
        /// </summary>
        /// <param name="outlet"></param>
        /// <returns></returns>
        public Compartment DowngradeOutletCompartmentToCompartment(OutletCompartment outlet)
        {
            var compartment = new Compartment();
            compartment.CopyFrom(outlet);
            compartment.TakeConnectionsOverFrom(outlet);

            lock (Compartments)
            {
                Compartments.Remove(outlet);
                Compartments.Add(compartment);
            }

            var incomingBranches = IncomingBranches;
            incomingBranches
                .OfType<ISewerConnection>()
                .Where(sw => sw.TargetCompartment == outlet)
                .ForEach(sw => sw.TargetCompartment = compartment);
            IncomingBranches = incomingBranches;
            var outgoingBranches = OutgoingBranches;
            outgoingBranches 
                .OfType<ISewerConnection>()
                .Where(sw => sw.SourceCompartment == outlet)
                .ForEach(sw => sw.SourceCompartment = compartment);
            OutgoingBranches = outgoingBranches;

            return compartment;
        }

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

        public override object Clone()
        {
            var manhole = (Manhole)Activator.CreateInstance(GetType());

            manhole.Name = Name;
            manhole.Geometry = Geometry == null ? null : ((IGeometry)Geometry.Clone());
            manhole.Attributes = (IFeatureAttributeCollection)(Attributes != null ? Attributes.Clone() : null);
            lock (manhole.NodeFeatures)
            {
                foreach (var nodeFeature in NodeFeatures)
                {
                    manhole.NodeFeatures.Add((INodeFeature) nodeFeature.Clone());
                }
            }

            return manhole;
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

        public virtual Coordinate LinkingCoordinate => Geometry?.Coordinate;

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
        public void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            throw new NotImplementedException();
        }

        #endregion

        [DisplayName("Compartments")]
        [FeatureAttribute(Order = 2)]
        public virtual int CompartmentCount
        {
            get => compartments.Count;
        }

        public IEnumerable<object> GetDirectChildren()
        {
            return Compartments;
        }
    }
}