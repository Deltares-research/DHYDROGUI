using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IPipe : IBranch, IHydroNetworkFeature
    {
        ICrossSectionDefinition CrossSectionDefinition { get; set; }

        IEnumerable<IStructure1D> Structures { get; set; }

        IEnumerable<IPump> Pumps { get; }

        IEnumerable<IGully> Gullies { get; }

        double StartZ { get; set; }

        double EndZ { get; set; }
    }
}