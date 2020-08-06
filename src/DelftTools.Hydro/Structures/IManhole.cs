using System;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    [Obsolete("D3DFMIQ-2083 Remove obsolete 1D functionality")]
    public interface IManhole : INodeFeature, IStructure1D {}
}