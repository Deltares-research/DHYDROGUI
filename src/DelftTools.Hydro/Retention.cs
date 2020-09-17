using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Implements a retention area.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class Retention : BranchFeatureHydroObject, IRetention, IItemContainer
    {
        public virtual IHydroNetwork HydroNetwork => throw new NotImplementedException();

        [DisplayName("LongName")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            throw new NotImplementedException();
        }
    }
}