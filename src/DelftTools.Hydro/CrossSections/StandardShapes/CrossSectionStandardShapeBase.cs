using System;
using System.Collections.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    [Entity(FireOnCollectionChange = false)]
    public abstract class CrossSectionStandardShapeBase : Unique<long>, ICrossSectionStandardShape
    {
        public abstract CrossSectionStandardShapeType Type { get; }

        public virtual IEnumerable<Coordinate> Profile => GetTabulatedDefinition().Profile;

        public abstract CrossSectionDefinitionZW GetTabulatedDefinition();

        public virtual object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }
    }
}