using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekWaterFlowModel1DReaderHelper
    {
        /// <summary>
        /// Method to improve performance?
        /// </summary>
        /// <param name="sourceCoverage"></param>
        /// <param name="targetCoverage"></param>
        public static void CopyCoverageValuesAndDefault(INetworkCoverage sourceCoverage, INetworkCoverage targetCoverage)
        {
            //suspend segment generation...
            var segmentGenerationMethod = targetCoverage.SegmentGenerationMethod;
            targetCoverage.SegmentGenerationMethod = SegmentGenerationMethod.None;

            targetCoverage.Clear();
            foreach (var location in sourceCoverage.Locations.Values)
            {
                targetCoverage[location] = sourceCoverage[location];
            }
            //copy the default value as well
            targetCoverage.DefaultValue = sourceCoverage.DefaultValue;

            targetCoverage.SegmentGenerationMethod = segmentGenerationMethod;
        }
    }
}