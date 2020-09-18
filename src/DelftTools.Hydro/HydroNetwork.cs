using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Hydrographic network (channels, pipes)
    /// </summary>
    [DisplayName("Hydro Network")]
    [Entity]
    public class HydroNetwork : Network, IHydroNetwork
    {
        public virtual IEventedList<IRegion> SubRegions { get; set; }

        public virtual IEnumerable<IRegion> AllRegions => HydroRegion.GetAllRegions(this);

        [Aggregation]
        public virtual IRegion Parent { get; set; }

        public virtual IEnumerable<IHydroObject> AllHydroObjects { get; }

        public new virtual bool EditWasCancelled { get; }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            yield break;
        }
    }
}