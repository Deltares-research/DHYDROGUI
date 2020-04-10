using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class PeriodImportTypeTest : EnumValuesTestFixture<PeriodImportExportType>
    {
        protected override IDictionary<PeriodImportExportType, int> ExpectedValueForEnumValues =>
            new Dictionary<PeriodImportExportType, int>
            {
                {PeriodImportExportType.Mean, 0},
                {PeriodImportExportType.Peak, 1},
            };
    }
}