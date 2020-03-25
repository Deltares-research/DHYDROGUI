using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class SpreadingTypeDescriptionTest : EnumDescriptionTestFixture<SpreadingType>
    {
        protected override IDictionary<SpreadingType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<SpreadingType, string>
            {
                {SpreadingType.Degrees, "degrees"},
                {SpreadingType.Power, "power"},
            };
    }
}