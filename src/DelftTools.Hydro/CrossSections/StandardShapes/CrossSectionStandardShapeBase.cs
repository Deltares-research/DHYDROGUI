using System.Collections.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public abstract class CrossSectionStandardShapeBase : Unique<long>, ICrossSectionStandardShape
    {
        public abstract CrossSectionStandardShapeType Type { get; }

        public virtual IEnumerable<Coordinate> Profile
        {
            get
            {
                return GetTabulatedDefinition().Profile;
            }
        }

        public abstract CrossSectionDefinitionZW GetTabulatedDefinition();

        public virtual object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }
    }
}