using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class SpreadingImportTypeTest : EnumValuesTestFixture<SpreadingImportType>
    {
        protected override IDictionary<SpreadingImportType, int> ExpectedValueForEnumValues =>
            new Dictionary<SpreadingImportType, int>
            {
                {SpreadingImportType.Degrees, 0},
                {SpreadingImportType.Power, 1}
            };
    }
}