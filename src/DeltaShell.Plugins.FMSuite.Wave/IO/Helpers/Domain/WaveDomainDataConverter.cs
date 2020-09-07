using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

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
        /// <param name="mdwDirPath">The path of the directory of the mdw file..</param>
        /// <param name="logHandler">The log handler.</param>
        /// <returns>
        /// The converted collection of <see cref="WaveDomainData"/>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="domainCategories"/> or <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="mdwDirPath"/> is <c>null</c> or empty.
        /// </exception>
        public static IEnumerable<WaveDomainData> Convert(IEnumerable<DelftIniCategory> domainCategories, string mdwDirPath, ILogHandler logHandler)
        {
            Ensure.NotNull(domainCategories, nameof(domainCategories));
            Ensure.NotNullOrEmpty(mdwDirPath, nameof(mdwDirPath));
            Ensure.NotNull(logHandler, nameof(logHandler));

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
                CreateMeteoData(domainCategory, domain, mdwDirPath, logHandler);
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

        private static void CreateMeteoData(DelftIniCategory domainCategory, WaveDomainData domain, string mdwDirPath, ILogHandler logHandler)
        {
            domain.UseGlobalMeteoData = true;

            List<string> meteoFiles = domainCategory.GetPropertyValues(KnownWaveProperties.MeteoFile).Select(f => Path.Combine(mdwDirPath, f)).ToList();

            if (!meteoFiles.Any())
            {
                return;
            }

            List<string> nonExistingFiles = meteoFiles.Where(f => !File.Exists(f)).ToList();
            if (nonExistingFiles.Any())
            {
                foreach (string file in nonExistingFiles)
                {
                    logHandler.ReportWarning($"Meteo file {file} does not exist for domain '{domain.Name}'. Defaulted to global settings.");
                }

                return;
            }

            ILookup<string, string> lookup = meteoFiles.ToLookup(Path.GetExtension);
            string[] spwFiles = lookup[".spw"].ToArray();
            string[] wndFiles = lookup[".wnd"].ToArray();
            string[] amuFiles = lookup[".amu"].ToArray();
            string[] amvFiles = lookup[".amv"].ToArray();

            var mapping = new Dictionary<(int, int, int, int), Func<WaveMeteoData>>
            {
                {(0, 1, 0, 0), () => WaveMeteoDataFactory.CreateVectorMeteoData(wndFiles[0])},
                {(0, 2, 0, 0), () => WaveMeteoDataFactory.CreateWndXYComponentMeteoData(wndFiles[0], wndFiles[1])},
                {(0, 0, 1, 1), () => WaveMeteoDataFactory.CreateXYComponentMeteoData(amuFiles[0], amvFiles[0])},
                {(1, 0, 0, 0), () => WaveMeteoDataFactory.CreateSpiderWebMeteoData(spwFiles[0])},
                {(1, 1, 0, 0), () => WaveMeteoDataFactory.CreateVectorMeteoData(wndFiles[0], spwFiles[0])},
                {(1, 2, 0, 0), () => WaveMeteoDataFactory.CreateWndXYComponentMeteoData(wndFiles[0], wndFiles[1], spwFiles[0])},
                {(1, 0, 1, 1), () => WaveMeteoDataFactory.CreateXYComponentMeteoData(amuFiles[0], amvFiles[0], spwFiles[0])},
            };

            if (mapping.TryGetValue((spwFiles.Length,
                                     wndFiles.Length,
                                     amuFiles.Length,
                                     amvFiles.Length),
                                    out Func<WaveMeteoData> createMeteoData))
            {
                WaveMeteoData meteoData = createMeteoData.Invoke();
                if (meteoData == null)
                {
                    logHandler.ReportWarning($"Meteo data could not be created for domain '{domain.Name}'. Defaulted to global settings.");
                    return;
                }

                domain.UseGlobalMeteoData = false;
                domain.MeteoData = meteoData;
            }
            else
            {
                logHandler.ReportWarning($"Meteo file combination not supported for domain '{domain.Name}'. Defaulted to global settings.");
            }
        }
    }
}