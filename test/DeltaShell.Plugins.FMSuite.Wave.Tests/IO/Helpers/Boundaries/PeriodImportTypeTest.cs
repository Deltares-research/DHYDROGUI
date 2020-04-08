using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class PeriodImportTypeTest : EnumValuesTestFixture<PeriodImportType>
    {
        protected override IDictionary<PeriodImportType, int> ExpectedValueForEnumValues =>
            new Dictionary<PeriodImportType, int>
            {
                {PeriodImportType.Mean, 0},
                {PeriodImportType.Peak, 1},
            };
    }
}