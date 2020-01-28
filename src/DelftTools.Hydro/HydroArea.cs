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
        public HydroArea()
        {
            Initialize();
        }

        private void Initialize()
        {
            Name = "Area";
            Links = new EventedList<HydroLink>();

            LandBoundaries = new EventedList<LandBoundary2D>();
            DryPoints = new EventedList<GroupablePointFeature>();
            DryAreas = new EventedList<GroupableFeature2DPolygon>();
            ThinDams = new EventedList<ThinDam2D>();
            FixedWeirs = new EventedList<FixedWeir>();
            ObservationPoints = new EventedList<GroupableFeature2DPoint>();
            ObservationCrossSections = new EventedList<ObservationCrossSection2D>();
            DumpingLocations = new EventedList<GroupableFeature2D>();
            DredgingLocations = new EventedList<GroupableFeature2D>();
            Embankments = new EventedList<Embankment>();
            Enclosures = new EventedList<GroupableFeature2DPolygon>();
            BridgePillars = new EventedList<BridgePillar>();

            Pumps = new EventedList<Pump2D>();
            Weirs = new EventedList<Weir2D>();
        }

        public virtual IEventedList<LandBoundary2D> LandBoundaries { get; protected set; }
        public virtual IEventedList<GroupablePointFeature> DryPoints { get; protected set; }
        public virtual IEventedList<GroupableFeature2DPolygon> DryAreas { get; protected set; }
        public virtual IEventedList<ThinDam2D> ThinDams { get; protected set; }
        public virtual IEventedList<FixedWeir> FixedWeirs { get; protected set; }
        public virtual IEventedList<GroupableFeature2DPoint> ObservationPoints { get; protected set; }
        public virtual IEventedList<ObservationCrossSection2D> ObservationCrossSections { get; protected set; }
        public virtual IEventedList<GroupableFeature2D> DumpingLocations { get; protected set; }
        public virtual IEventedList<GroupableFeature2D> DredgingLocations { get; protected set; }
        public virtual IEventedList<Embankment> Embankments { get; protected set; }
        public virtual IEventedList<GroupableFeature2DPolygon> Enclosures { get; protected set; }
        public virtual IEventedList<BridgePillar> BridgePillars { get; protected set; }

        public virtual IEventedList<Pump2D> Pumps { get; protected set; }
        public virtual IEventedList<Weir2D> Weirs { get; protected set; }

        #region IHydroRegion

        public virtual IEnumerable<IHydroObject> AllHydroObjects => Enumerable.Empty<IHydroObject>();

        public virtual IEventedList<HydroLink> Links { get; set; }

        public override IEnumerable<object> GetDirectChildren()
        {
            yield return Pumps; // Required to open view for the collection of pumps
            foreach (Pump2D pump in Pumps)
            {
                yield return pump;
            }

            yield return Weirs; // Required to open view for the collection of weirs
            foreach (Weir2D weir in Weirs)
            {
                yield return weir;
            }

            foreach (ThinDam2D thinDam in ThinDams)
            {
                yield return thinDam;
            }

            foreach (FixedWeir fixedWeir in FixedWeirs)
            {
                yield return fixedWeir;
            }

            foreach (LandBoundary2D landBoundary in LandBoundaries)
            {
                yield return landBoundary;
            }

            yield return DryPoints;
            foreach (GroupablePointFeature dryPoint in DryPoints)
            {
                yield return dryPoint;
            }

            foreach (GroupableFeature2DPolygon dryArea in DryAreas)
            {
                yield return dryArea;
            }

            foreach (GroupableFeature2DPoint observationPoint in ObservationPoints)
            {
                yield return observationPoint;
            }

            foreach (ObservationCrossSection2D observationCrossSection in ObservationCrossSections)
            {
                yield return observationCrossSection;
            }

            foreach (GroupableFeature2D dumpingLocation in DumpingLocations)
            {
                yield return dumpingLocation;
            }

            foreach (GroupableFeature2D dredgingLocation in DredgingLocations)
            {
                yield return dredgingLocation;
            }

            foreach (Embankment embankment in Embankments)
            {
                yield return embankment;
            }

            foreach (GroupableFeature2DPolygon enclosure in Enclosures)
            {
                yield return enclosure;
            }

            foreach (BridgePillar bridgePillar in BridgePillars)
            {
                yield return bridgePillar;
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
            var clone = (HydroArea) base.Clone();
            clone.Initialize();
            clone.Links = new EventedList<HydroLink>(Links);

            return clone;
        }

        #endregion
    }
}