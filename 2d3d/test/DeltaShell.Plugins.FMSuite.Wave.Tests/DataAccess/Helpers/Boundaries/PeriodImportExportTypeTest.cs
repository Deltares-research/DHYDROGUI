using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class PeriodImportExportTypeTest : EnumValuesTestFixture<PeriodImportExportType>
    {
        protected override IDictionary<PeriodImportExportType, int> ExpectedValueForEnumValues =>
            new Dictionary<PeriodImportExportType, int>
            {
                {PeriodImportExportType.Mean, 0},
                {PeriodImportExportType.Peak, 1}
            };
    }
}