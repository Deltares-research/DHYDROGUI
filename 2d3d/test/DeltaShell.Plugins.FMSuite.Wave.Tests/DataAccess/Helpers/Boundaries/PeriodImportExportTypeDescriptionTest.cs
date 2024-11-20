using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class PeriodImportExportTypeDescriptionTest : EnumDescriptionTestFixture<PeriodImportExportType>
    {
        protected override IDictionary<PeriodImportExportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<PeriodImportExportType, string>
            {
                {PeriodImportExportType.Mean, KnownWaveBoundariesFileConstants.MeanPeriodType},
                {PeriodImportExportType.Peak, KnownWaveBoundariesFileConstants.PeakPeriodType}
            };
    }
}