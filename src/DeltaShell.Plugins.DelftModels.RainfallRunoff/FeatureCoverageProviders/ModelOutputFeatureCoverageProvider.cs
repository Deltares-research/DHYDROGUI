using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders
{
    public class ModelOutputFeatureCoverageProvider : IFeatureCoverageProvider
    {
        private const string OutputPrefix = "Output: ";
        private readonly RainfallRunoffModel model;

        public ModelOutputFeatureCoverageProvider(RainfallRunoffModel model)
        {
            this.model = model;
        }

        private IEnumerable<IFeatureCoverage> FeatureCoverages
        {
            get
            {
                return model.OutputDataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output).Where(
                    di => di.Value is IFeatureCoverage).
                    Select(di => di.Value as IFeatureCoverage);
            }
        }

        #region IFeatureCoverageProvider Members

        public IEnumerable<string> FeatureCoverageNames
        {
            get { return FeatureCoverages.Select(fc => OutputPrefix + fc.Name); }
        }

        public IFeatureCoverage GetFeatureCoverageByName(string name)
        {
            return FeatureCoverages.FirstOrDefault(fc => OutputPrefix + fc.Name == name);
        }

        #endregion
    }
}