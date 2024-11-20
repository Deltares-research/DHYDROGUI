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

        /// <summary>
        /// Is the default channel section (does not have roughness defined on lanes)
        /// </summary>
        public bool IsDefaultChannelsSection { get; set; }

        /// <summary>
        /// The Width is equal to the total width of the cross section that is covered by this
        /// cross section section. See the drawing above for an explanation.
        /// </summary>

        public override string ToString()
        {
            return string.Format("{0}: {1} - {2}", SectionType, MinY, MaxY);
        }
    }
}