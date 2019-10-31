using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.SpectralData
{
    [TestFixture]
    public class WavePeriodTypeTest : EnumValuesTestFixture<WavePeriodType>
    {
        protected override IDictionary<WavePeriodType, int> ExpectedValueForEnumValues =>
            new Dictionary<WavePeriodType, int>
            {
                { WavePeriodType.Peak, 0 },
                { WavePeriodType.Mean, 1 },
            };
    }
}