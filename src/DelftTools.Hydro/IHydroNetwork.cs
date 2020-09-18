using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public interface IHydroNetwork : INetwork, IHydroRegion
    {
    }
}