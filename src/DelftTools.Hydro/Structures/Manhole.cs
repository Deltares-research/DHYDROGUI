using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Structures
{
    public class Manhole : Node, IManhole
    {
        private IGeometry geometry;
        private IEventedList<Compartment> compartments;
        private bool compartmentsChanging;

        private bool geometryRefreshFromCompartments;

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
                return !Compartments.Any() ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.X ?? 0.0);
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
                return !Compartments.Any() ? 0.0 : Compartments.Average(c => c.Geometry?.Coordinate.Y ?? 0.0);
            }
        }
        
        public override IGeometry Geometry
        {
            get { return geometry; }
            set
            {
                if(Equals(geometry, value)) return;
                if (geometry != null && !geometryRefreshFromCompartments)
                {
                    var diffX = value.Coordinate.X - geometry.Coordinate.X;
                    var diffY = value.Coordinate.Y - geometry.Coordinate.Y;

                    Compartments.ForEach(c =>
                    {
                        var oldCoordinate = c.Geometry.Coordinate;
                        c.Geometry = new Point(oldCoordinate.X + diffX, oldCoordinate.Y + diffY);
                    });
                }
                geometry = value;
            }
        }

        public IEventedList<Compartment> Compartments
        {
            get { return compartments; }
            set
            {
                if (value != null)
                {
                    value.ForEach(comp => comp.ParentManhole = this);
                    value.CollectionChanging += CompartmentCollectionChanging;
                    value.CollectionChanged += CompartmentCollectionChanged;
                }
                compartments = value;
                RefreshGeometry();
            }
        }

        private void CompartmentCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {

            var compartment = e.Item as Compartment;
            if (compartment == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    compartmentsChanging = true;
                    var compartmentNames = compartments.Select(c => c.Name);
                    if (compartmentNames.Contains(compartment.Name)) compartments.RemoveAllWhere(c => c.Name == compartment.Name);
                    compartmentsChanging = false;
                    break;
                case NotifyCollectionChangeAction.Remove:
                case NotifyCollectionChangeAction.Replace:
                    compartment.PropertyChanged -= OnCompartmentPropertyChanged;
                    break;
            }
        }


        private void CompartmentCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var compartment = e.Item as Compartment;
            if (compartment == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    compartment.PropertyChanged += OnCompartmentPropertyChanged;
                    compartment.ParentManhole = this;
                    if (compartment.Geometry == null) compartment.Geometry = Geometry;
                    break;
                case NotifyCollectionChangeAction.Replace:
                    compartment.PropertyChanged += OnCompartmentPropertyChanged;
                    break;
            }
            RefreshGeometry();
        }

        private void OnCompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(Compartment.Geometry)) return;
            RefreshGeometry();
        }

        private void RefreshGeometry()
        {
            geometryRefreshFromCompartments = true;
            Geometry = new Point(XCoordinate, YCoordinate);
            geometryRefreshFromCompartments = false;
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
