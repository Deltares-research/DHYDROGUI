using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Domain
{
    /// <summary>
    /// Converter for converting a collection of boundary <see cref="DelftIniCategory"/>
    /// to a collection of <see cref="WaveDomainData"/>.
    /// </summary>
    public static class WaveDomainDataConverter
    {
        private const string xWindQuantity = "x_wind";
        private const string yWindQuantity = "y_wind";

        private static readonly MeteoFileReader meteoFileReader = new MeteoFileReader();

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
                SetMeteoData(domainCategory, domain, mdwDirPath, logHandler);
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

        private static void SetMeteoData(DelftIniCategory domainCategory, WaveDomainData domain, string mdwDirPath, ILogHandler logHandler)
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

            WaveMeteoData meteoData = CreateWaveMeteoData(spwFiles, wndFiles, amuFiles, amvFiles);

            if (meteoData == null)
            {
                logHandler.ReportWarning($"Meteo data could not be created for domain '{domain.Name}'. Defaulted to global settings.");
                return;
            }

            if (spwFiles.Length == 1)
            {
                AddSpiderwebData(meteoData, spwFiles[0]);
            }

            domain.UseGlobalMeteoData = false;
            domain.MeteoData = meteoData;
        }

        private static bool TryCreateSpiderWebMeteoData(IList<string> spwFiles,
                                                        IList<string> wndFiles,
                                                        IList<string> amuFiles,
                                                        IList<string> amvFiles,
                                                        out WaveMeteoData meteoData)
        {
            meteoData = null;

            if (spwFiles.Count == 1 &&
                wndFiles.Count == 0 &&
                amuFiles.Count == 0 &&
                amvFiles.Count == 0)
            {
                meteoData = new WaveMeteoData {FileType = WindDefinitionType.SpiderWebGrid};
                return true;
            }

            return false;
        }

        private static bool TryCreateWindXYMeteoData(IList<string> wndFiles,
                                                     IList<string> amuFiles,
                                                     IList<string> amvFiles,
                                                     out WaveMeteoData meteoData)
        {
            meteoData = null;

            if (wndFiles.Count == 1 && amuFiles.Count == 0 && amvFiles.Count == 0)
            {
                meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXY,
                    XYVectorFilePath = wndFiles[0],
                };
                return true;
            }

            return false;
        }

        private static bool TryCreateWindXWindYMeteoData(IList<string> wndFiles,
                                                         IList<string> amuFiles,
                                                         IList<string> amvFiles,
                                                         out WaveMeteoData meteoData)
        {
            meteoData = null;
            string xWind = null;
            string yWind = null;

            if (wndFiles.Count == 2 && amuFiles.Count == 0 && amvFiles.Count == 0)
            {
                string wndFile1 = wndFiles[0];
                string wndFile2 = wndFiles[1];

                string quantity1 = GetQuantity(wndFile1);
                string quantity2 = GetQuantity(wndFile2);

                if (quantity1 == quantity2 ||
                    !IsSupported(quantity1) ||
                    !IsSupported(quantity2))
                {
                    return false;
                }

                xWind = quantity1 == xWindQuantity ? wndFile1 : wndFile2;
                yWind = quantity1 == yWindQuantity ? wndFile1 : wndFile2;
            }

            else if (wndFiles.Count == 0 && amuFiles.Count == 1 && amvFiles.Count == 1)
            {
                xWind = amuFiles[0];
                yWind = amvFiles[0];
            }

            if (xWind == null || yWind == null)
            {
                return false;
            }

            meteoData = new WaveMeteoData()
            {
                FileType = WindDefinitionType.WindXWindY,
                XComponentFilePath = xWind,
                YComponentFilePath = yWind,
            };

            return true;
        }

        private static WaveMeteoData CreateWaveMeteoData(IList<string> spwFiles,
                                                         IList<string> wndFiles,
                                                         IList<string> amuFiles,
                                                         IList<string> amvFiles)
        {
            if (TryCreateSpiderWebMeteoData(spwFiles, wndFiles, amuFiles, amvFiles, out WaveMeteoData spiderWebData))
            {
                return spiderWebData;
            }

            if (TryCreateWindXYMeteoData(wndFiles, amuFiles, amvFiles, out WaveMeteoData windXYData))
            {
                return windXYData;
            }

            if (TryCreateWindXWindYMeteoData(wndFiles, amuFiles, amvFiles, out WaveMeteoData windXWindYData))
            {
                return windXWindYData;
            }

            return null;
        }

        private static void AddSpiderwebData(WaveMeteoData meteoData, string spwFilePath)
        {
            meteoData.HasSpiderWeb = true;
            meteoData.SpiderWebFilePath = spwFilePath;
        }

        private static bool IsSupported(string quantity) => quantity == xWindQuantity || quantity == yWindQuantity;

        private static string GetQuantity(string file)
        {
            return meteoFileReader.Read(file)
                                  .FirstOrDefault(mp => mp.Property == KnownWaveProperties.MeteoQuantityField)?
                                  .Value;
        }
    }
}