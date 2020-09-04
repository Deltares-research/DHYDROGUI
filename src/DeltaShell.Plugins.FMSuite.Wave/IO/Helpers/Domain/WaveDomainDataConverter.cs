using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Domain
{
    /// <summary>
    /// Converter for converting a collection of boundary <see cref="DelftIniCategory"/>
    /// to a collection of <see cref="WaveDomainData"/>.
    /// </summary>
    public static class WaveDomainDataConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="domainCategories"/> to
        /// their respective <see cref="WaveDomainData"/>.
        /// </summary>
        /// <param name="domainCategories">The domain categories.</param>
        /// <returns>
        /// The converted collection of <see cref="WaveDomainData"/>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="domainCategories"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<WaveDomainData> Convert(IEnumerable<DelftIniCategory> domainCategories)
        {
            Ensure.NotNull(domainCategories, nameof(domainCategories));

            foreach (DelftIniCategory domainCategory in domainCategories)
            {
                string gridFileName = domainCategory.GetPropertyValue("Grid", "");
                string domainName = Path.GetFileNameWithoutExtension(gridFileName);

                var domain = new WaveDomainData(domainName)
                {
                    GridFileName = gridFileName,
                    BedLevelGridFileName = domainCategory.GetPropertyValue("BedLevelGrid", ""),
                    BedLevelFileName = domainCategory.GetPropertyValue("BedLevel", ""),
                    NestedInDomain = int.Parse(domainCategory.GetPropertyValue("NestedInDomain", "-1"), NumberStyles.Any, CultureInfo.InvariantCulture)
                };

                CreateDirectionalSpaceData(domainCategory, domain);
                CreateFrequencySpaceData(domainCategory, domain);
                CreateHydroFromFlowSettingsData(domainCategory, domain);

                yield return domain;
            }
        }

        private static void CreateHydroFromFlowSettingsData(DelftIniCategory domainCategory, WaveDomainData domain)
        {
            string bedLevelUsage = domainCategory.GetPropertyValue("FlowBedLevel");
            if (bedLevelUsage != null)
            {
                domain.HydroFromFlowData.BedLevelUsage = (UsageFromFlowType) int.Parse(bedLevelUsage, NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.HydroFromFlowData.WaterLevelUsage = (UsageFromFlowType) int.Parse(domainCategory.GetPropertyValue("FlowWaterLevel", "0"),
                                                                                         NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.HydroFromFlowData.VelocityUsage = (UsageFromFlowType) int.Parse(domainCategory.GetPropertyValue("FlowVelocity", "0"),
                                                                                       NumberStyles.Any,
                                                                                       CultureInfo.InvariantCulture);
                string velocityType = domainCategory.GetPropertyValue("FlowVelocityType", "not-specified");
                switch (velocityType)
                {
                    case "wave-dependent":
                        domain.HydroFromFlowData.VelocityUsageType = VelocityComputationType.WaveDependent;
                        break;
                    case "surface-layer":
                        domain.HydroFromFlowData.VelocityUsageType = VelocityComputationType.SurfaceLayer;
                        break;
                    default:
                        domain.HydroFromFlowData.VelocityUsageType = VelocityComputationType.DepthAveraged;
                        break;
                }

                domain.HydroFromFlowData.WindUsage = (UsageFromFlowType) int.Parse(domainCategory.GetPropertyValue("FlowWind", "0"),
                                                                                   NumberStyles.Any, CultureInfo.InvariantCulture);

                domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings = false;
            }
            else
            {
                domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings = true;
            }
        }

        private static void CreateFrequencySpaceData(DelftIniCategory domainCategory, WaveDomainData domain)
        {
            string nFreq = domainCategory.GetPropertyValue("NFreq");
            if (nFreq != null)
            {
                domain.SpectralDomainData.NFreq = (int) double.Parse(nFreq, NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.SpectralDomainData.FreqMin = double.Parse(domainCategory.GetPropertyValue("FreqMin", "0.0"),
                                                                 NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.SpectralDomainData.FreqMax = double.Parse(domainCategory.GetPropertyValue("FreqMax", "0.0"),
                                                                 NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.SpectralDomainData.UseDefaultFrequencySpace = false;
            }
            else
            {
                domain.SpectralDomainData.UseDefaultFrequencySpace = true;
            }
        }

        private static void CreateDirectionalSpaceData(DelftIniCategory domainCategory, WaveDomainData domain)
        {
            string spaceType = domainCategory.GetPropertyValue("DirSpace");
            if (spaceType != null)
            {
                domain.SpectralDomainData.DirectionalSpaceType = spaceType == "circle"
                                                                     ? WaveDirectionalSpaceType.Circle
                                                                     : WaveDirectionalSpaceType.Sector;
                domain.SpectralDomainData.NDir = int.Parse(domainCategory.GetPropertyValue("NDir", "0"),
                                                           NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.SpectralDomainData.StartDir = double.Parse(domainCategory.GetPropertyValue("StartDir", "0.0"),
                                                                  NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.SpectralDomainData.EndDir = double.Parse(domainCategory.GetPropertyValue("EndDir", "0.0"),
                                                                NumberStyles.Any, CultureInfo.InvariantCulture);
                domain.SpectralDomainData.UseDefaultDirectionalSpace = false;
            }
            else
            {
                domain.SpectralDomainData.UseDefaultDirectionalSpace = true;
            }
        }
    }
}