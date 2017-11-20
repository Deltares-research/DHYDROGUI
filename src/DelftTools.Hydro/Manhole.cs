using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    public class Manhole : Node
    {
        private IEventedList<Compartment> compartments;
        private bool compartmentsChanging;

        public Manhole(string manholeId) : base(manholeId)
        {
            Compartments = new EventedList<Compartment>();
        }

        public IEventedList<Compartment> Compartments
        {
            get { return compartments; }
            set
            {
                value.ForEach(comp => comp.ParentManhole = this);
                value.CollectionChanging += CompartmentCollectionChanging;
                value.CollectionChanged += CompartmentCollectionChanged;
                compartments = value;
            }
        }

        /// <summary>
        /// The x-coordinate of this Manhole. This is equal tot the average of the x-coordinates
        /// of its compartments.
        /// </summary>
        public double XCoordinate
        {
            get
            {
                return Compartments.Count == 0 ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.X ?? 0.0);
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
                return Compartments.Count == 0 ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.Y ?? 0.0);
            }
        }

        public override IGeometry Geometry
        {
            get { return new Point(XCoordinate, YCoordinate); }
        }

        private void CompartmentCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if(compartmentsChanging || e.Action != NotifyCollectionChangeAction.Add) return;
            compartmentsChanging = true;

            var compartment = e.Item as Compartment;
            if (compartment == null) return;
            var compartmentNames = compartments.Select(c => c.Name);

            // Remove compartments that have the same name
            if (compartmentNames.Contains(compartment.Name)) compartments.RemoveAllWhere(c => c.Name == compartment.Name);
            
            compartmentsChanging = false;
        }

        private void CompartmentCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action != NotifyCollectionChangeAction.Add) return;

            var compartment = e.Item as Compartment;
            if(compartment ==  null) return;
            compartment.ParentManhole = this;
        }

        public Compartment GetCompartmentByName(string compartmentName)
        {
            return Compartments.FirstOrDefault(c => c.Name.Equals(compartmentName));
        }

        public bool ContainsCompartment(string compartmentName)
        {
            return Compartments.Any(c => c.Name.Equals(compartmentName));
        }
    }
}
