using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class SpreadingImportTypeDescriptionTest : EnumDescriptionTestFixture<SpreadingImportType>
    {
        protected override IDictionary<SpreadingImportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<SpreadingImportType, string>
            {
                {SpreadingImportType.Degrees, KnownWaveBoundariesFileConstants.DegreesDefinedSpreading},
                {SpreadingImportType.Power, KnownWaveBoundariesFileConstants.PowerDefinedSpreading}
            };
    }
}