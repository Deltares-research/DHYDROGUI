using System;
using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public interface IHydroNetwork : INetwork, IHydroRegion
    {
        IEventedList<Route> Routes { get; }

        IEnumerable<IHydroNode> HydroNodes { get; }
        IEnumerable<IPipe> Pipes { get; }
        IEnumerable<IChannel> Channels { get; }
        IEnumerable<IStructure1D> Structures { get; }
        IEnumerable<ICompositeBranchStructure> CompositeBranchStructures { get; }
        IEnumerable<IPump> Pumps { get; }
        IEnumerable<IWeir> Weirs { get; }
        IEnumerable<IGate> Gates { get; }
        IEnumerable<ILateralSource> LateralSources { get; }
        IEnumerable<IRetention> Retentions { get; }
        IEnumerable<IObservationPoint> ObservationPoints { get; }
        IEnumerable<IExtraResistance> ExtraResistances { get; }

        IEnumerable<IManhole> Manholes { get; }
        IEnumerable<IGully> Gullies { get; }
        INode GetNodeByName(string nodeName);
    }
}