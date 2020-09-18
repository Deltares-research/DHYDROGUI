using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    public interface IDrainageBasin : IHydroRegion {}

    /// <summary>
    /// Drainage basin is defined as a set of catchments (sub-basins) covering some drainage area, including a set of
    /// related hydgraphic features such as waste-water treatment plants.
    /// </summary>
    [Entity]
    [DisplayName("Drainage Basin")]
    public class DrainageBasin : RegionBase, IDrainageBasin
    {
        public virtual IEventedList<Catchment> Catchments { get; set; }

        public virtual IEnumerable<IHydroObject> AllHydroObjects => throw new NotImplementedException();

        public virtual IEnumerable<Catchment> AllCatchments { get; }

        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual HydroLink AddNewLink(IHydroObject source, IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveLink(IHydroObject source, IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanLinkTo(IHydroObject source, IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> GetDirectChildren() => throw new NotImplementedException();
    }
}