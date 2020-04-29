using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.SpectralData
{
    [TestFixture]
    public class WaveDirectionalSpreadingTypeTest : EnumValuesTestFixture<WaveDirectionalSpreadingType>
    {
        protected override IDictionary<WaveDirectionalSpreadingType, int> ExpectedValueForEnumValues =>
            new Dictionary<WaveDirectionalSpreadingType, int>
            {
                { WaveDirectionalSpreadingType.Power,   0 },
                { WaveDirectionalSpreadingType.Degrees, 1 },
            };
    }
}