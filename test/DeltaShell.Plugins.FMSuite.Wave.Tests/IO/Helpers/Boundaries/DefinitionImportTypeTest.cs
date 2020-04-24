using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    public class DefinitionImportTypeTest : EnumValuesTestFixture<DefinitionImportType>
    {
        protected override IDictionary<DefinitionImportType, int> ExpectedValueForEnumValues =>
            new Dictionary<DefinitionImportType, int>
            {
                {DefinitionImportType.Coordinates, 0},
                {DefinitionImportType.Oriented, 1},
                {DefinitionImportType.SpectrumFile, 2}
            };
    }
}
