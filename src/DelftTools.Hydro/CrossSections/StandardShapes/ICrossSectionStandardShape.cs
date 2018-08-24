using System;
using System.Collections.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    public interface ICrossSectionStandardShape : IUnique<long>, ICloneable, INameable, ISewerFeature
    {
        CrossSectionStandardShapeType Type { get; }
        IEnumerable<Coordinate> Profile { get; }
        CrossSectionDefinitionZW GetTabulatedDefinition();
    }
}
