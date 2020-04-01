using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    public class DefinitionTypeTest : EnumValuesTestFixture<DefinitionType>
    {
        protected override IDictionary<DefinitionType, int> ExpectedValueForEnumValues =>
            new Dictionary<DefinitionType, int>
            {
                {DefinitionType.Coordinates, 0},
                {DefinitionType.Oriented, 1},
            };
    }
}
