using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class PeriodTypeTest : EnumValuesTestFixture<PeriodType>
    {
        protected override IDictionary<PeriodType, int> ExpectedValueForEnumValues =>
            new Dictionary<PeriodType, int>
            {
                {PeriodType.Mean, 0},
                {PeriodType.Peak, 1},
            };
    }
}