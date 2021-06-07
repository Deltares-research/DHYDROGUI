using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders
{
    public class CompositeFeatureCoverageProvider : IFeatureCoverageProvider
    {
        private readonly IEnumerable<IFeatureCoverageProvider> providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeFeatureCoverageProvider"/> class.
        /// </summary>
        /// <param name="providers"> The feature coverage providers. </param>
        /// <param name="model"> The model of which the feature coverages are provided. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="providers"/> or <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public CompositeFeatureCoverageProvider(IEnumerable<IFeatureCoverageProvider> providers, IRainfallRunoffModel model)
        {
            Ensure.NotNull(providers, nameof(providers));
            Ensure.NotNull(model, nameof(model));
            
            this.providers = providers;
            Model = model;
        }

        /// <summary>
        /// The model of which the feature coverages are provided.
        /// </summary>
        public IRainfallRunoffModel Model { get; }

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