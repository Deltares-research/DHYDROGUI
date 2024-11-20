using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// Responsible for updating a catchment(feature)coverage whenever the model/network changes. 
    /// Eg, when a relevant catchment is added to the network, add it to the coverage. Can be more 
    /// complex for specific coverages only for unpaved for example.
    /// </summary>
    public interface ICatchmentCoverageMaintainer
    {
        /// <summary>
        /// Allows maintainer to initialize and optionally do some event subscription
        /// </summary>
        /// <param name="featureCoverage"></param>
        void Initialize(IFeatureCoverage featureCoverage);
    }
}