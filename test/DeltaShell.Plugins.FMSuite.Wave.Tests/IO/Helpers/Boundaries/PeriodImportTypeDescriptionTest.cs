using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class PeriodImportTypeDescriptionTest : EnumDescriptionTestFixture<PeriodImportType>
    {
        protected override IDictionary<PeriodImportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<PeriodImportType, string>
            {
                {PeriodImportType.Mean, "mean"},
                {PeriodImportType.Peak, "peak"},
            };
    }
}