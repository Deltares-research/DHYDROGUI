using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class DefinitionImportTypeDescriptionTest : EnumDescriptionTestFixture<DefinitionImportType>
    {
        protected override IDictionary<DefinitionImportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<DefinitionImportType, string>
            {
                {DefinitionImportType.Coordinates, KnownWaveBoundariesFileConstants.CoordinatesDefinitionType},
                {DefinitionImportType.Oriented, KnownWaveBoundariesFileConstants.OrientationDefinitionType},
                {DefinitionImportType.SpectrumFile, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType},
            };
    }
}
