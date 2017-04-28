using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public class HydroArea : RegionBase, IHydroRegion
    {

        public const string LandBoundariesPluralName = "Land Boundaries";
        public const string DryPointsPluralName = "Dry Points";
        public const string DryAreasPluralName = "Dry Areas";
        public const string ThinDamsPluralName = "Thin Dams";
        public const string FixedWeirsPluralName = "Fixed Weirs";
        public const string ObservationPointsPluralName = "Observation Points";
        public const string ObservationCrossSectionsPluralName = "Observation Cross-Sections";
        public const string PumpsPluralName = "Pumps";
        public const string WeirsPluralName = "Weirs";
        public const string GatesPluralName = "Gates";
        public const string EmbankmentsPluralName = "Embankments";

        public HydroArea()
        {
            Initialize();
        }

        private void Initialize()
        {
            Name = "Area";
            Links = new EventedList<HydroLink>();

            LandBoundaries = new EventedList<LandBoundary2D>();
            DryPoints = new EventedList<PointFeature>();
            DryAreas = new EventedList<Feature2DPolygon>();
            ThinDams = new EventedList<ThinDam2D>();
            FixedWeirs = new EventedList<FixedWeir>();
            ObservationPoints = new EventedList<Feature2DPoint>();
            ObservationCrossSections = new EventedList<ObservationCrossSection2D>();
            DumpingLocations = new EventedList<Feature2D>();
            DredgingLocations = new EventedList<Feature2D>();
            Embankments = new EventedList<Embankment>();

            Pumps = new EventedList<IPump>();
            Weirs = new EventedList<IWeir>();
            Gates = new EventedList<IGate>();
        }

        public virtual IEventedList<LandBoundary2D> LandBoundaries { get; protected set; }
        public virtual IEventedList<PointFeature> DryPoints { get; protected set; }
        public virtual IEventedList<Feature2DPolygon> DryAreas { get; protected set; }
        public virtual IEventedList<ThinDam2D> ThinDams { get; protected set; }
        public virtual IEventedList<FixedWeir> FixedWeirs { get; protected set; }
        public virtual IEventedList<Feature2DPoint> ObservationPoints { get; protected set; }
        public virtual IEventedList<ObservationCrossSection2D> ObservationCrossSections { get; protected set; }
        public virtual IEventedList<Feature2D> DumpingLocations { get; protected set; }
        public virtual IEventedList<Feature2D> DredgingLocations { get; protected set; }
        public virtual IEventedList<Embankment> Embankments { get; protected set; }

        public virtual IEventedList<IPump> Pumps { get; protected set; }
        public virtual IEventedList<IWeir> Weirs { get; protected set; }
        public virtual IEventedList<IGate> Gates { get; protected set; }

        #region IHydroRegion

        public virtual IEnumerable<IHydroObject> AllHydroObjects
        {
            get { return Enumerable.Empty<IHydroObject>(); }
        }

        public virtual IEventedList<HydroLink> Links { get; set; }

        public override IEnumerable<object> GetDirectChildren()
        {
            yield return Pumps; // Required to open view for the collection of pumps
            foreach (var pump in Pumps)
            {
                yield return pump;
            }
            yield return Weirs; // Required to open view for the collection of weirs
            foreach (var weir in Weirs)
            {
                yield return weir;
            }
            yield return Gates;
            foreach (var gate in Gates)
            {
                yield return gate;
            }

            foreach (var thinDam in ThinDams)
            {
                yield return thinDam;
            }
            foreach (var fixedWeir in FixedWeirs)
            {
                yield return fixedWeir;
            }
            foreach (var landBoundary in LandBoundaries)
            {
                yield return landBoundary;
            }
            yield return DryPoints;
            foreach (var dryPoint in DryPoints)
            {
                yield return dryPoint;
            }
            foreach (var dryArea in DryAreas)
            {
                yield return dryArea;
            }
            foreach (var observationPoint in ObservationPoints)
            {
                yield return observationPoint;
            }
            foreach (var observationCrossSection in ObservationCrossSections)
            {
                yield return observationCrossSection;
            }
            foreach (var dumpingLocation in DumpingLocations)
            {
                yield return dumpingLocation;
            }
            foreach (var dredgingLocation in DredgingLocations)
            {
                yield return dredgingLocation;
            }
            foreach (var embankment in Embankments)
            {
                yield return embankment;
            }
        }

        public virtual HydroLink AddNewLink(IHydroObject source, IHydroObject target)
        {
            return HydroRegion.AddNewLink(source, target);
        }

        public virtual void RemoveLink(IHydroObject source, IHydroObject target)
        {
            HydroRegion.RemoveLink(source, target);
        }

        public virtual bool CanLinkTo(IHydroObject source, IHydroObject target)
        {
            return HydroRegion.CanLinkTo(source, target);
        }

        public override object Clone()
        {
            var clone = (HydroArea)base.Clone();
            clone.Initialize();
            clone.Links = new EventedList<HydroLink>(Links);

            return clone;
        }

        #endregion
    }
}