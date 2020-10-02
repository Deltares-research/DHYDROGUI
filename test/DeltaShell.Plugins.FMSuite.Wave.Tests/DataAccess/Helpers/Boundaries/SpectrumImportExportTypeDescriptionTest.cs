using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class SpectrumImportExportTypeDescriptionTest : EnumDescriptionTestFixture<SpectrumImportExportType>
    {
        protected override IDictionary<SpectrumImportExportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<SpectrumImportExportType, string>
            {
                {SpectrumImportExportType.Parametrized, KnownWaveBoundariesFileConstants.ParametrizedSpectrumType},
                {SpectrumImportExportType.FromFile, KnownWaveBoundariesFileConstants.FromFileSpectrumType}
            };
    }
}