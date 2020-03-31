using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public interface IHydroNetwork : INetwork, IHydroRegion
    {
        IEventedList<Route> Routes { get; }

        IEnumerable<IHydroNode> HydroNodes{ get; }
        IEnumerable<IPipe> Pipes { get; }
        IEnumerable<IManhole> Manholes { get; }
        IEnumerable<OutletCompartment> OutletCompartments { get; }
        IEnumerable<Compartment> Compartments { get; }
        IEnumerable<Orifice> Orifices { get; }
        IEnumerable<ISewerConnection> SewerConnections { get; }
        IEnumerable<IChannel> Channels { get; }

        IEnumerable<ICrossSection> CrossSections { get; }

        IEnumerable<IStructure1D> Structures { get; }

        IEnumerable<ICompositeBranchStructure> CompositeBranchStructures { get; }

        IEnumerable<IPump> Pumps { get; }
        IEnumerable<ICulvert> Culverts { get; }
        IEnumerable<IBridge> Bridges { get; }
        IEnumerable<IWeir> Weirs { get; }
        IEnumerable<IGate> Gates { get; }
        IEnumerable<ILateralSource> LateralSources { get; }
        IEnumerable<IRetention> Retentions { get; }
        IEnumerable<IObservationPoint> ObservationPoints { get; }
        IEnumerable<IExtraResistance> ExtraResistances { get; }
        IEnumerable<IGully> Gullies { get; }

        IEventedList<CrossSectionSectionType> CrossSectionSectionTypes { get; }
        IEventedList<ICrossSectionDefinition> SharedCrossSectionDefinitions { get; }

        ICrossSectionDefinition DefaultCrossSectionDefinition { get; set; }
    }
}