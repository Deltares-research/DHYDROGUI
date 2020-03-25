using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class SpreadingTypeTest : EnumValuesTestFixture<SpreadingType>
    {
        protected override IDictionary<SpreadingType, int> ExpectedValueForEnumValues =>
            new Dictionary<SpreadingType, int>
            {
                {SpreadingType.Degrees, 0},
                {SpreadingType.Power, 1},
            };
    }
}