using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Domain;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Domain
{
    [TestFixture]
    public class WaveDomainDataConverterTest
    {
        private readonly Random rand = new Random();
        private readonly DomainEqualityComparer comparer = new DomainEqualityComparer();

        [Test]
        public void Convert_FilesDoesNotExist_UseGlobalMeteoDataIsTrue()
        {
            // Setup
            var domain = new WaveDomainData("the_domain");
            DelftIniCategory category = CreateCategory(domain);
            category.AddProperty("MeteoFile", "meteo1.file");
            category.AddProperty("MeteoFile", "meteo2.file");

            var logHandler = Substitute.For<ILogHandler>();

            // Call
            IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[]
            {
                category
            }, @"c:\some\dir", logHandler);

            // Assert
            WaveDomainData resultDomain = result.First();
            Assert.That(resultDomain.UseGlobalMeteoData, Is.True);
            logHandler.Received(1).ReportWarning(@"Meteo file c:\some\dir\meteo1.file does not exist for domain 'the_domain'. Defaulted to global settings.");
            logHandler.Received(1).ReportWarning(@"Meteo file c:\some\dir\meteo2.file does not exist for domain 'the_domain'. Defaulted to global settings.");
        }

        [Test]
        public void Convert_UnspecifiedQuantity_UseGlobalMeteoDataIsTrue()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = "xwind.wnd",
                    YComponentFilePath = "ywind.wnd"
                };

                WaveDomainData domain = CreateWaveDomainData(true, true, false, true, meteoData);
                DelftIniCategory category = CreateCategory(domain);

                temp.CreateFile(meteoData.XComponentFileName);
                temp.CreateFile(meteoData.YComponentFileName);

                var logHandler = Substitute.For<ILogHandler>();

                // Call
                IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[]
                {
                    category
                }, temp.Path, logHandler);

                // Assert
                WaveDomainData resultDomain = result.First();
                Assert.That(resultDomain.UseGlobalMeteoData, Is.True);
                logHandler.Received(1).ReportWarning("Meteo data could not be created for domain 'the_domain'. Defaulted to global settings.");
            }
        }

        [Test]
        public void Convert_EqualQuantities_UseGlobalMeteoDataIsTrue()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = "xwind.wnd",
                    YComponentFilePath = "ywind.wnd"
                };

                WaveDomainData domain = CreateWaveDomainData(true, true, false, true, meteoData);
                DelftIniCategory category = CreateCategory(domain);

                temp.CreateFile(meteoData.XComponentFileName, GetExampleContent("x_wind"));
                temp.CreateFile(meteoData.YComponentFileName, GetExampleContent("x_wind"));

                var logHandler = Substitute.For<ILogHandler>();

                // Call
                IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[]
                {
                    category
                }, temp.Path, logHandler);

                // Assert
                WaveDomainData resultDomain = result.First();
                Assert.That(resultDomain.UseGlobalMeteoData, Is.True);
                logHandler.Received(1).ReportWarning("Meteo data could not be created for domain 'the_domain'. Defaulted to global settings.");
            }
        }

        [Test]
        public void Convert_UnsupportedQuantityWndFile1_UseGlobalMeteoDataIsTrue()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = "xwind.wnd",
                    YComponentFilePath = "ywind.wnd"
                };

                WaveDomainData domain = CreateWaveDomainData(true, true, false, true, meteoData);
                DelftIniCategory category = CreateCategory(domain);

                temp.CreateFile(meteoData.XComponentFileName, GetExampleContent("the_wind"));
                temp.CreateFile(meteoData.YComponentFileName, GetExampleContent("y_wind"));

                var logHandler = Substitute.For<ILogHandler>();

                // Call
                IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[]
                {
                    category
                }, temp.Path, logHandler);

                // Assert
                WaveDomainData resultDomain = result.First();
                Assert.That(resultDomain.UseGlobalMeteoData, Is.True);
                logHandler.Received(1).ReportWarning("Meteo data could not be created for domain 'the_domain'. Defaulted to global settings.");
            }
        }

        [Test]
        public void Convert_UnsupportedQuantityWndFile2_UseGlobalMeteoDataIsTrue()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = "xwind.wnd",
                    YComponentFilePath = "ywind.wnd"
                };

                WaveDomainData domain = CreateWaveDomainData(true, true, false, true, meteoData);
                DelftIniCategory category = CreateCategory(domain);

                temp.CreateFile(meteoData.XComponentFileName, GetExampleContent("x_wind"));
                temp.CreateFile(meteoData.YComponentFileName, GetExampleContent("the_wind"));

                var logHandler = Substitute.For<ILogHandler>();

                // Call
                IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[]
                {
                    category
                }, temp.Path, logHandler);

                // Assert
                WaveDomainData resultDomain = result.First();
                Assert.That(resultDomain.UseGlobalMeteoData, Is.True);
                logHandler.Received(1).ReportWarning("Meteo data could not be created for domain 'the_domain'. Defaulted to global settings.");
            }
        }

        [TestCaseSource(nameof(ArgumentNullCases))]
        public void Convert_ArgumentNull_ThrowsArgumentNullException(IEnumerable<DelftIniCategory> domainCategories, ILogHandler logHandler, string expParamName)
        {
            // Call
            void Call() => WaveDomainDataConverter.Convert(domainCategories, "some/path", logHandler).ToList();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        private static IEnumerable<TestCaseData> ArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<ILogHandler>(), "domainCategories");
            yield return new TestCaseData(Enumerable.Empty<DelftIniCategory>(), null, "logHandler");
        }

        [TestCase(null)]
        [TestCase("")]
        public void Convert_MdwDirPathNullOrEmpty_ThrowsArgumentException(string mdwDirPath)
        {
            // Call
            void Call() => WaveDomainDataConverter.Convert(Enumerable.Empty<DelftIniCategory>(), mdwDirPath, Substitute.For<ILogHandler>()).ToList();

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("mdwDirPath"));
        }

        [TestCaseSource(nameof(ValidDomainCases))]
        public void Convert_ReturnsTheCorrectWaveDomainData(bool useDefDir, bool useDefFreq, bool useDefMeteo, bool useDefHydro, WaveMeteoData meteoData)
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                WaveDomainData domain = CreateWaveDomainData(useDefDir, useDefFreq, useDefMeteo, useDefHydro, meteoData);
                DelftIniCategory category = CreateCategory(domain);
                CreateFiles(temp, domain);

                var logHandler = Substitute.For<ILogHandler>();

                // Call
                IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[]
                {
                    category
                }, temp.Path, logHandler);

                // Assert
                WaveDomainData resultDomain = result.First();
                Assert.That(resultDomain, Is.EqualTo(domain).Using(comparer));
                logHandler.DidNotReceiveWithAnyArgs().ReportWarning(Arg.Any<string>());
            }
        }

        [TestCaseSource(nameof(VariousUnsupportedMeteoFileCombinations))]
        public void Convert_MeteoFileCombinationNotSupported_UseGlobalMeteoDataIsTrue(string[] meteoFiles)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var domain = new WaveDomainData("the_domain");
                DelftIniCategory category = CreateCategory(domain);
                foreach (string meteoFile in meteoFiles)
                {
                    category.AddProperty("MeteoFile", meteoFile);
                    temp.CreateFile(meteoFile);
                }

                var logHandler = Substitute.For<ILogHandler>();

                // Call
                IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[]
                {
                    category
                }, temp.Path, logHandler);

                // Assert
                WaveDomainData resultDomain = result.First();
                Assert.That(resultDomain.UseGlobalMeteoData, Is.True);
                logHandler.Received(1).ReportWarning("Meteo data could not be created for domain 'the_domain'. Defaulted to global settings.");
            }
        }

        private static IEnumerable<string[]> VariousUnsupportedMeteoFileCombinations()
        {
            yield return new string[]
            {
                "meteo.spw",
                "meteo.spw"
            };
            yield return new string[]
            {
                "meteo.amu"
            };
            yield return new string[]
            {
                "meteo.amu",
                "meteo.amu"
            };
            yield return new string[]
            {
                "meteo.amv"
            };
            yield return new string[]
            {
                "meteo.amv",
                "meteo.amv"
            };
            yield return new string[]
            {
                "meteo.wnd",
                "meteo.wnd",
                "meteo.wnd"
            };
            yield return new string[]
            {
                "meteo.wnd",
                "meteo.amu",
                "meteo.spw"
            };
            yield return new string[]
            {
                "meteo.wnd",
                "meteo.amu",
                "meteo.amv"
            };
            yield return new string[]
            {
                "meteo.amu",
                "meteo.amu",
                "meteo.amv"
            };
        }

        private static IEnumerable<TestCaseData> ValidDomainCases()
        {
            foreach (WaveMeteoData meteoData in GetWaveMeteoData())
            {
                yield return new TestCaseData(false, false, false, false, meteoData);
                yield return new TestCaseData(false, false, false, true, meteoData);
                yield return new TestCaseData(false, false, true, false, meteoData);
                yield return new TestCaseData(false, false, true, true, meteoData);
                yield return new TestCaseData(false, true, false, false, meteoData);
                yield return new TestCaseData(false, true, false, true, meteoData);
                yield return new TestCaseData(false, true, true, false, meteoData);
                yield return new TestCaseData(false, true, true, true, meteoData);
                yield return new TestCaseData(true, false, false, false, meteoData);
                yield return new TestCaseData(true, false, false, true, meteoData);
                yield return new TestCaseData(true, false, true, false, meteoData);
                yield return new TestCaseData(true, false, true, true, meteoData);
                yield return new TestCaseData(true, true, false, false, meteoData);
                yield return new TestCaseData(true, true, false, true, meteoData);
                yield return new TestCaseData(true, true, true, false, meteoData);
                yield return new TestCaseData(true, true, true, true, meteoData);
            }
        }

        private static IEnumerable<WaveMeteoData> GetWaveMeteoData()
        {
            yield return new WaveMeteoData
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                HasSpiderWeb = true,
                SpiderWebFilePath = "file.spw"
            };
            yield return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXY,
                HasSpiderWeb = true,
                SpiderWebFilePath = "file.spw",
                XYVectorFilePath = "xy_file.wnd"
            };
            yield return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXY,
                HasSpiderWeb = false,
                XYVectorFilePath = "xy_file.wnd"
            };

            yield return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                HasSpiderWeb = true,
                SpiderWebFilePath = "file.spw",
                XComponentFilePath = "x_file.wnd",
                YComponentFilePath = "y_file.wnd",
            };
            yield return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                HasSpiderWeb = false,
                XComponentFilePath = "x_file.wnd",
                YComponentFilePath = "y_file.wnd",
            };
            yield return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                HasSpiderWeb = true,
                SpiderWebFilePath = "file.spw",
                XComponentFilePath = "x_file.amu",
                YComponentFilePath = "y_file.amv",
            };
            yield return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                HasSpiderWeb = false,
                XComponentFilePath = "x_file.amu",
                YComponentFilePath = "y_file.amv",
            };
        }

        private WaveDomainData CreateWaveDomainData(bool useDefDir, bool useDefFreq, bool useDefMeteo, bool useDefHydro, WaveMeteoData meteoData)
        {
            var spectralDomainData = Create.For<SpectralDomainData>();
            var hydroFromFlowData = Create.For<HydroFromFlowSettings>();

            spectralDomainData.UseDefaultDirectionalSpace = useDefDir;
            spectralDomainData.UseDefaultFrequencySpace = useDefFreq;
            hydroFromFlowData.UseDefaultHydroFromFlowSettings = useDefHydro;

            var domain = new WaveDomainData("the_domain")
            {
                BedLevelGridFileName = "bed_level_grid_file",
                Output = rand.NextBoolean(),
                NestedInDomain = -1,
                SpectralDomainData = spectralDomainData,
                MeteoData = meteoData,
                HydroFromFlowData = hydroFromFlowData,
                UseGlobalMeteoData = useDefMeteo
            };

            return domain;
        }

        private static DelftIniCategory CreateCategory(IWaveDomainData domain)
        {
            var domainCategory = new DelftIniCategory(KnownWaveCategories.DomainCategory);
            domainCategory.AddProperty("Grid", domain.GridFileName);
            domainCategory.AddProperty("BedLevelGrid", domain.BedLevelGridFileName);
            domainCategory.AddProperty("BedLevel", domain.BedLevelFileName);

            if (!domain.SpectralDomainData.UseDefaultDirectionalSpace)
            {
                domainCategory.AddProperty("DirSpace", domain.SpectralDomainData.DirectionalSpaceType.GetDescription().ToLower());
                domainCategory.AddProperty("NDir", domain.SpectralDomainData.NDir);
                domainCategory.AddProperty("StartDir", domain.SpectralDomainData.StartDir);
                domainCategory.AddProperty("EndDir", domain.SpectralDomainData.EndDir);
            }

            if (!domain.SpectralDomainData.UseDefaultFrequencySpace)
            {
                domainCategory.AddProperty("NFreq", domain.SpectralDomainData.NFreq);
                domainCategory.AddProperty("FreqMin", domain.SpectralDomainData.FreqMin);
                domainCategory.AddProperty("FreqMax", domain.SpectralDomainData.FreqMax);
            }

            if (!domain.UseGlobalMeteoData)
            {
                WaveMeteoData meteoData = domain.MeteoData;
                switch (meteoData.FileType)
                {
                    case WindDefinitionType.WindXY:
                        domainCategory.AddProperty(KnownWaveProperties.MeteoFile, meteoData.XYVectorFileName);
                        break;
                    case WindDefinitionType.WindXWindY:
                        domainCategory.AddProperty(KnownWaveProperties.MeteoFile, meteoData.XComponentFileName);
                        domainCategory.AddProperty(KnownWaveProperties.MeteoFile, meteoData.YComponentFileName);
                        break;
                }

                if (meteoData.FileType == WindDefinitionType.SpiderWebGrid || meteoData.HasSpiderWeb)
                {
                    domainCategory.AddProperty(KnownWaveProperties.MeteoFile, meteoData.SpiderWebFileName);
                }
            }

            if (!domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings)
            {
                domainCategory.AddProperty("FlowBedLevel", (int) domain.HydroFromFlowData.BedLevelUsage);
                domainCategory.AddProperty("FlowWaterLevel", (int) domain.HydroFromFlowData.WaterLevelUsage);
                domainCategory.AddProperty("FlowVelocity", (int) domain.HydroFromFlowData.VelocityUsage);
                domainCategory.AddProperty("FlowVelocityType", domain.HydroFromFlowData.VelocityUsageType.GetDescription());
                domainCategory.AddProperty("FlowWind", (int) domain.HydroFromFlowData.WindUsage);
            }

            return domainCategory;
        }

        private static void CreateFiles(TemporaryDirectory temp, WaveDomainData domain)
        {
            if (domain.UseGlobalMeteoData)
            {
                return;
            }

            temp.CreateFile(domain.MeteoData.SpiderWebFileName);
            temp.CreateFile(domain.MeteoData.XYVectorFileName);
            temp.CreateFile(domain.MeteoData.XComponentFileName, GetExampleContent("x_wind"));
            temp.CreateFile(domain.MeteoData.YComponentFileName, GetExampleContent("y_wind"));
        }

        private static string GetExampleContent(string quantity)
        {
            return "FileVersion      = 1.03\n" +
                   "filetype = meteo_on_equidistant_grid\n" +
                   "NODATA_value = -999\n" +
                   "n_cols = 2\n" +
                   "n_rows = 2\n" +
                   "grid_unit = m\n" +
                   "x_llcorner = 1.00E+03\n" +
                   "y_llcorner = 1.00E+03\n" +
                   "dx = 2.44E+04\n" +
                   "dy = 3.00E+04\n" +
                   "n_quantity = 1\n" +
                   $"quantity1 = {quantity}\n" +
                   "unit1 = m s - 1\n";
        }

        private sealed class DomainEqualityComparer : IEqualityComparer<WaveDomainData>
        {
            public bool Equals(WaveDomainData x, WaveDomainData y)
            {
                if (x.Name != y.Name
                    || x.BedLevelFileName != y.BedLevelFileName
                    || x.BedLevelGridFileName != y.BedLevelGridFileName
                    || x.GridFileName != y.GridFileName
                    || x.NestedInDomain != y.NestedInDomain)
                {
                    return false;
                }

                if (!Equals(x.SpectralDomainData, y.SpectralDomainData))
                {
                    return false;
                }

                if (x.UseGlobalMeteoData != y.UseGlobalMeteoData)
                {
                    return false;
                }

                if (!x.UseGlobalMeteoData && !Equals(x.MeteoData, y.MeteoData))
                {
                    return false;
                }

                if (!Equals(x.HydroFromFlowData, y.HydroFromFlowData))
                {
                    return false;
                }

                return true;
            }

            public int GetHashCode(WaveDomainData node) => throw new NotImplementedException();

            private static bool Equals(SpectralDomainData x, SpectralDomainData y)
            {
                if (x.UseDefaultFrequencySpace != y.UseDefaultFrequencySpace)
                {
                    return false;
                }

                if (!x.UseDefaultFrequencySpace)
                {
                    if (x.NFreq != y.NFreq ||
                        !Equals(x.FreqMin, y.FreqMin) ||
                        !Equals(x.FreqMax, y.FreqMax))
                    {
                        return false;
                    }
                }

                if (!x.UseDefaultDirectionalSpace)
                {
                    if (x.DirectionalSpaceType != y.DirectionalSpaceType ||
                        x.NDir != y.NDir ||
                        !Equals(x.StartDir, y.StartDir) ||
                        !Equals(x.EndDir, y.EndDir))
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool Equals(WaveMeteoData x, WaveMeteoData y)
            {
                if (x.FileType != y.FileType || x.HasSpiderWeb != y.HasSpiderWeb)
                {
                    return false;
                }

                if (x.HasSpiderWeb && x.SpiderWebFileName != y.SpiderWebFileName)
                {
                    return false;
                }

                if (x.FileType == WindDefinitionType.WindXWindY && (x.XComponentFileName != y.XComponentFileName ||
                                                                    x.YComponentFileName != y.YComponentFileName))
                {
                    return false;
                }

                if (x.FileType == WindDefinitionType.WindXY && x.XYVectorFileName != y.XYVectorFileName)
                {
                    return false;
                }

                return true;
            }

            private static bool Equals(HydroFromFlowSettings x, HydroFromFlowSettings y)
            {
                if (x.UseDefaultHydroFromFlowSettings != y.UseDefaultHydroFromFlowSettings)
                {
                    return false;
                }

                if (x.UseDefaultHydroFromFlowSettings)
                {
                    return true;
                }

                if (x.BedLevelUsage != y.BedLevelUsage ||
                    x.WaterLevelUsage != y.WaterLevelUsage ||
                    x.VelocityUsage != y.VelocityUsage ||
                    x.VelocityUsageType != y.VelocityUsageType ||
                    x.WindUsage != y.WindUsage)
                {
                    return false;
                }

                return true;
            }

            private static bool Equals(double x, double y) => Math.Abs(x - y) < 1E-7;
        }
    }
}