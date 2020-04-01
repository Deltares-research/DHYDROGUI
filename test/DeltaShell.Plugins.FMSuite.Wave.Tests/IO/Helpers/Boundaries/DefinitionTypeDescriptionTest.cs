using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class DefinitionTypeDescriptionTest : EnumDescriptionTestFixture<DefinitionType>
    {
        protected override IDictionary<DefinitionType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<DefinitionType, string>
            {
                {DefinitionType.Coordinates, "xy-coordinates"},
                {DefinitionType.Oriented, "orientation"},
            };
    }
}
