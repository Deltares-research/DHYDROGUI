using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Domain;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Domain
{
    [TestFixture]
    public class WaveDomainDataConverterTest
    {
        private readonly Random rand = new Random();
        private readonly DomainEqualityComparer comparer = new DomainEqualityComparer();

        [TestCaseSource(nameof(ArgumentNullCases))]
        public void Convert_ArgumentNull_ThrowsArgumentNullException(IEnumerable<DelftIniCategory> domainCategories, ILogHandler logHandler, string expParamName)
        {
            // Call
            void Call() => WaveDomainDataConverter.Convert(domainCategories, "some/path", logHandler).ToList();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        private IEnumerable<TestCaseData> ArgumentNullCases()
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

        [TestCase(false, false, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(true, true, true)]
        public void Convert_(bool useDefDir, bool useDefFreq, bool useDefHydro)
        {
            WaveDomainData domain = CreateWaveDomainData(useDefDir, useDefFreq, useDefHydro);
            DelftIniCategory category = CreateCategory(domain);
            const string mdwDirPath = "c:/some/dir";
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            IEnumerable<WaveDomainData> result = WaveDomainDataConverter.Convert(new[] {category}, mdwDirPath, logHandler);

            // Call
            WaveDomainData resultDomain = result.First();
            Assert.That(resultDomain, Is.EqualTo(domain).Using(comparer));
        }

        private WaveDomainData CreateWaveDomainData(bool useDefFreq, bool useDefDir, bool useDefHydro)
        {
            var domain = new WaveDomainData("the_domain")
            {
                BedLevelGridFileName = "bed_level_grid_file",
                Output = rand.NextBoolean(),
                NestedInDomain = -1,
                SpectralDomainData = Create.For<SpectralDomainData>(),
                HydroFromFlowData = Create.For<HydroFromFlowSettings>()
            };

            domain.SpectralDomainData.UseDefaultDirectionalSpace = useDefDir;
            domain.SpectralDomainData.UseDefaultFrequencySpace = useDefFreq;
            domain.HydroFromFlowData.UseDefaultHydroFromFlowSettings = useDefHydro;

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

        private class DomainEqualityComparer : IEqualityComparer<WaveDomainData>
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

                if (!Equals(x.HydroFromFlowData, y.HydroFromFlowData))
                {
                    return false;
                }

                return true;
            }

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

            public int GetHashCode(WaveDomainData node) => throw new NotImplementedException();

            private static bool Equals(double x, double y) => Math.Abs(x - y) < 1E-7;
        }
    }
}