using System;
using System.Collections.Generic;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public interface ICrossSectionStandardShape : IUnique<long>, ICloneable
    {
        CrossSectionStandardShapeType Type { get; }
        IEnumerable<Coordinate> Profile { get; }
        CrossSectionDefinitionZW GetTabulatedDefinition();
    }
}