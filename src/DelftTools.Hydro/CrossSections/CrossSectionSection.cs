using System;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.CrossSections
{
    [Serializable]
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionSection : Unique<long>
    {
        public virtual double MinY { get; set; }

        public virtual double MaxY { get; set; }

        [Aggregation]
        public virtual CrossSectionSectionType SectionType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} - {2}", SectionType, MinY, MaxY);
        }
    }
}