using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class Manhole : Node, IManhole
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
                //return Geometry.Coordinate!Compartments.Any() ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.X ?? 0.0);
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
                // return !Compartments.Any() ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.Y ?? 0.0);
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

        private void CompartmentCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var compartment = e.Item as Compartment;
            if (compartment == null || e.Action != NotifyCollectionChangeAction.Add) return;

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
