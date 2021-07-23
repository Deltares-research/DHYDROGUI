using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Item for grouping a region and it elected features
    /// </summary>
    public class RegionFeatureSelection
    {
        public RegionFeatureSelection(IHydroRegion region, IEnumerable<IFeature> features)
        {
            Region = region;
            Features = features.ToList();
        }

        /// <summary>
        /// Region of the item
        /// </summary>
        public IHydroRegion Region { get; }

        /// <summary>
        /// Selected features
        /// </summary>
        public IList<IFeature> Features { get; }
    }
}