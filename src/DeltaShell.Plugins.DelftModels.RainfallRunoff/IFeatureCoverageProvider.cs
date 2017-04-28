using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// At some point perhaps move this to framework level?
    /// </summary>
    public interface IFeatureCoverageProvider
    {
        IEnumerable<string> FeatureCoverageNames { get; }
        IFeatureCoverage GetFeatureCoverageByName(string name);
    }
}