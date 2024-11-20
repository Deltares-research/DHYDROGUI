using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class DefinitionImportTypeDescriptionTest : EnumDescriptionTestFixture<DefinitionImportType>
    {
        protected override IDictionary<DefinitionImportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<DefinitionImportType, string>
            {
                {DefinitionImportType.Coordinates, KnownWaveBoundariesFileConstants.CoordinatesDefinitionType},
                {DefinitionImportType.Oriented, KnownWaveBoundariesFileConstants.OrientationDefinitionType},
                {DefinitionImportType.SpectrumFile, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType}
            };
    }
}