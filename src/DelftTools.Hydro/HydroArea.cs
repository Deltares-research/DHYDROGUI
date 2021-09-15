using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    /// <summary>
    /// <see cref="HydroArea"/> defines a single <see cref="IHydroRegion"/>
    /// describing all Hydro elements (i.e. elements that can be visualized on the map).
    /// </summary>
    /// <seealso cref="RegionBase" />
    /// <seealso cref="IHydroRegion" />
    [Entity]
    public class HydroArea : RegionBase, IHydroRegion
    {
        /// <summary>
        /// Creates a new <see cref="HydroArea"/>.
        /// </summary>
        public HydroArea()
        {
            Initialize();
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="LandBoundary2D"/>.
        /// </summary>
        public virtual IEventedList<LandBoundary2D> LandBoundaries { get; protected set; }

        /// <summary>
        /// Gets or sets the dry points.
        /// </summary>
        public virtual IEventedList<GroupablePointFeature> DryPoints { get; protected set; }

        /// <summary>
        /// Gets or sets the dry areas.
        /// </summary>
        public virtual IEventedList<GroupableFeature2DPolygon> DryAreas { get; protected set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="ThinDam2D"/> objects.
        /// </summary>
        public virtual IEventedList<ThinDam2D> ThinDams { get; protected set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="FixedWeir"/>.
        /// </summary>
        public virtual IEventedList<FixedWeir> FixedWeirs { get; protected set; }

        /// <summary>
        /// Gets or sets the observation points.
        /// </summary>
        public virtual IEventedList<GroupableFeature2DPoint> ObservationPoints { get; protected set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="ObservationCrossSection2D"/> objects.
        /// </summary>
        public virtual IEventedList<ObservationCrossSection2D> ObservationCrossSections { get; protected set; }

        /// <summary>
        /// Gets or sets the dumping locations.
        /// </summary>
        public virtual IEventedList<GroupableFeature2D> DumpingLocations { get; protected set; }

        /// <summary>
        /// Gets or sets the dredging locations.
        /// </summary>
        public virtual IEventedList<GroupableFeature2D> DredgingLocations { get; protected set; }

        /// <summary>
        /// Gets or sets the enclosures.
        /// </summary>
        public virtual IEventedList<GroupableFeature2DPolygon> Enclosures { get; protected set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="BridgePillar"/> objects.
        /// </summary>
        public virtual IEventedList<BridgePillar> BridgePillars { get; protected set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Pump"/>.
        /// </summary>
        public virtual IEventedList<Pump> Pumps { get; protected set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Structure"/> objects.
        /// </summary>
        public virtual IEventedList<Structure> Structures { get; protected set; }

        private void Initialize()
        {
            Name = "Area";

            LandBoundaries = new EventedList<LandBoundary2D>();
            DryPoints = new EventedList<GroupablePointFeature>();
            DryAreas = new EventedList<GroupableFeature2DPolygon>();
            ThinDams = new EventedList<ThinDam2D>();
            FixedWeirs = new EventedList<FixedWeir>();
            ObservationPoints = new EventedList<GroupableFeature2DPoint>();
            ObservationCrossSections = new EventedList<ObservationCrossSection2D>();
            DumpingLocations = new EventedList<GroupableFeature2D>();
            DredgingLocations = new EventedList<GroupableFeature2D>();
            Enclosures = new EventedList<GroupableFeature2DPolygon>();
            BridgePillars = new EventedList<BridgePillar>();

            Pumps = new EventedList<Pump>();
            Structures = new EventedList<Structure>();
        }

        #region IHydroRegion

        public virtual IEnumerable<IHydroObject> AllHydroObjects => Enumerable.Empty<IHydroObject>();

        public override IEnumerable<object> GetDirectChildren()
        {
            yield return Pumps; // Required to open view for the collection of pumps
            foreach (Pump pump in Pumps)
            {
                yield return pump;
            }

            yield return Structures; // Required to open view for the collection of weirs
            foreach (Structure weir in Structures)
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

            foreach (GroupableFeature2DPolygon enclosure in Enclosures)
            {
                yield return enclosure;
            }

            foreach (BridgePillar bridgePillar in BridgePillars)
            {
                yield return bridgePillar;
            }
        }

        public override object Clone()
        {
            var clone = (HydroArea)base.Clone();
            clone.Initialize();

            return clone;
        }

        #endregion
    }
}