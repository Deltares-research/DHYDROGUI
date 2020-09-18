using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Entity]
    public abstract class BranchFeatureHydroObject : BranchFeature, IHydroObject
    {
        public virtual IHydroRegion Region => (IHydroRegion) Network;
    }
}