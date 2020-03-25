using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class PeriodTypeDescriptionTest : EnumDescriptionTestFixture<PeriodType>
    {
        protected override IDictionary<PeriodType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<PeriodType, string>
            {
                {PeriodType.Mean, "mean"},
                {PeriodType.Peak, "peak"},
            };
    }
}