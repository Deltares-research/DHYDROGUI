using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionSectionType : Unique<long>, INameable, ICloneable
    {
        public virtual string Name { get; set;}

        public virtual object Clone()
        {
            return new CrossSectionSectionType { Name = Name };
        }

        public override string ToString()
        {
            return Name ?? "<CSST>";
        }
    }
}