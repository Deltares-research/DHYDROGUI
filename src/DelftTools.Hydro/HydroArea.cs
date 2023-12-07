using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
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
        public const string LeveeBreachName = "Levee breaches";
        public const string ObservationPointsPluralName = "Observation Points";
        public const string ObservationCrossSectionsPluralName = "Observation Cross-Sections";
        public const string PumpsPluralName = "Pumps";
        public const string WeirsPluralName = "Weirs";
        public const string GatesPluralName = "Gates";
        public const string EmbankmentsPluralName = "Embankments";
        public const string EnclosureName = "Enclosure";
        public const string RoofAreaName = "Roof Areas";
        public const string GullyName = "Gullies";
        public const string BridgePillarsPluralName = "Bridge Pillars";

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
            LeveeBreaches = new EventedList<Feature2D>();
            ObservationPoints = new EventedList<GroupableFeature2DPoint>();
            ObservationCrossSections = new EventedList<ObservationCrossSection2D>();
            DumpingLocations = new EventedList<GroupableFeature2D>();
            DredgingLocations = new EventedList<GroupableFeature2D>();
            Embankments = new EventedList<Embankment>();
            Enclosures = new EventedList<GroupableFeature2DPolygon>();
            BridgePillars = new EventedList<BridgePillar>();

            Pumps = new EventedList<Pump2D>();
            Weirs = new EventedList<Weir2D>();
            Gates = new EventedList<Gate2D>();

            RoofAreas = new EventedList<GroupableFeature2DPolygon>();
            Gullies = new EventedList<Gully>();
        }

        public virtual IEventedList<LandBoundary2D> LandBoundaries { get; protected set; }
        public virtual IEventedList<GroupablePointFeature> DryPoints { get; protected set; }
        public virtual IEventedList<GroupableFeature2DPolygon> DryAreas { get; protected set; }
        public virtual IEventedList<ThinDam2D> ThinDams { get; protected set; }
        public virtual IEventedList<FixedWeir> FixedWeirs { get; protected set; }
        public virtual IEventedList<Feature2D> LeveeBreaches { get; protected set; }
        public virtual IEventedList<GroupableFeature2DPoint> ObservationPoints { get; protected set; }
        public virtual IEventedList<ObservationCrossSection2D> ObservationCrossSections { get; protected set; }
        public virtual IEventedList<GroupableFeature2D> DumpingLocations { get; protected set; }
        public virtual IEventedList<GroupableFeature2D> DredgingLocations { get; protected set; }
        public virtual IEventedList<Embankment> Embankments { get; protected set; }
        public virtual IEventedList<GroupableFeature2DPolygon> Enclosures { get; protected set; }
        public virtual IEventedList<BridgePillar> BridgePillars { get; protected set; }

        public virtual IEventedList<Pump2D> Pumps { get; protected set; }
        public virtual IEventedList<Weir2D> Weirs { get; protected set; }
        public virtual IEventedList<Gate2D> Gates { get; protected set; }

        public virtual IEventedList<GroupableFeature2DPolygon> RoofAreas { get; protected set; }

        public virtual IEventedList<Gully> Gullies { get; protected set; }

        #region IHydroRegion

        public virtual IEnumerable<IHydroObject> AllHydroObjects
        {
            get { return Pumps.OfType<IHydroObject>().Concat(Weirs).Concat(Gates).Concat(LeveeBreaches.OfType<IHydroObject>()); }
        }

        public virtual IEventedList<HydroLink> Links { get; protected set; }


        public override IEnumerable<object> GetDirectChildren()
        {
            yield return Pumps; // Required to open view for the collection of pumps
            yield return Weirs; // Required to open view for the collection of weirs
            yield return Gates;
            yield return LeveeBreaches;
            yield return DryPoints;
            yield return RoofAreas;
            yield return Gullies;

            var structures2d = Pumps.OfType<IFeature>()
                .Concat(Weirs)
                .Concat(Gates)
                .Concat(ThinDams)
                .Concat(FixedWeirs)
                .Concat(LeveeBreaches)
                .Concat(LandBoundaries)
                .Concat(DryPoints)
                .Concat(DryAreas)
                .Concat(ObservationPoints)
                .Concat(ObservationCrossSections)
                .Concat(DumpingLocations)
                .Concat(DredgingLocations)
                .Concat(Embankments)
                .Concat(Enclosures)
                .Concat(RoofAreas)
                .Concat(Gullies)
                .Concat(BridgePillars);
            
            foreach (var structure2D in structures2d)
            {
                yield return structure2D;
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