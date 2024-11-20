using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    public class FileBasedFeatureCoverage : FeatureCoverage
    {
        public FileBasedFeatureCoverage() {}

        public FileBasedFeatureCoverage(string name) : base(name) {}
    }
}