using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;


namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class Manhole : Node, IManhole, ICompositeNetworkPointFeature
    {
        private IEventedList<Compartment> compartments;

        public Manhole(string manholeId) : base(manholeId)
        {
            Geometry = new Point(0, 0);
            Compartments = new EventedList<Compartment>();
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

        public IEventedList<Compartment> Compartments
        {
            get { return compartments; }
            set
            {
                if (compartments != null)
                {
                    compartments.CollectionChanged -= CompartmentCollectionChanged;
                }

                compartments = value;

                if (compartments != null)
                {
                    compartments.ForEach(comp => comp.ParentManhole = this);
                    compartments.CollectionChanged += CompartmentCollectionChanged;
                }
            }
        }

        public Compartment GetCompartmentByName(string compartmentName)
        {
            return Compartments.FirstOrDefault(c => c.Name.Equals(compartmentName));
        }

        public bool ContainsCompartmentWithName(string compartmentName)
        {
            return Compartments != null && Compartments.Any(c => c.Name.Equals(compartmentName));
        }

        public IEnumerable<IFeature> GetPointFeatures()
        {
            var branchFeatures = this.InternalStructures();
            var outletCompartments = compartments.Where(c => c.IsOutletCompartment());

            var features = new List<IFeature>();
            features.AddRange(branchFeatures);
            features.AddRange(outletCompartments);
            return features;
        }

        public NetworkFeatureType NetworkFeatureType { get; } = NetworkFeatureType.Node;

        private void CompartmentCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var compartment = e.Item as Compartment;
            if (compartment == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Remove:
                    //It has a NoNotifyPropertyChanged, so it won't propagate again.
                    compartment.ParentManhole = null;
                    return;
                case NotifyCollectionChangeAction.Add:
                    var oldParentManhole = compartment.ParentManhole;
                    if (oldParentManhole != null && oldParentManhole != this && oldParentManhole.ContainsCompartmentWithName(compartment.Name))
                    {
                        oldParentManhole.Compartments.Remove(compartment);
                    }
                    compartment.ParentManhole = this;
                    break;
            }
        }
    }
}
