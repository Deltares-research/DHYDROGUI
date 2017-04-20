using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders
{
    public class CompositeFeatureCoverageProvider : IFeatureCoverageProvider
    {
        private readonly IEnumerable<IFeatureCoverageProvider> providers;

        public CompositeFeatureCoverageProvider(IEnumerable<IFeatureCoverageProvider> providers)
        {
            this.providers = providers;
        }

        #region IFeatureCoverageProvider Members

        public IEnumerable<string> FeatureCoverageNames
        {
            get { return providers.SelectMany(pv => pv.FeatureCoverageNames); }
        }

        public IFeatureCoverage GetFeatureCoverageByName(string name)
        {
            return providers.Select(pv => pv.GetFeatureCoverageByName(name)).Where(fc => fc != null).FirstOrDefault();
        }

        #endregion
    }
}