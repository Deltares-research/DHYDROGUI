using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class SpectrumImportExportTypeTest : EnumValuesTestFixture<SpectrumImportExportType>
    {
        protected override IDictionary<SpectrumImportExportType, int> ExpectedValueForEnumValues =>
            new Dictionary<SpectrumImportExportType, int>
            {
                {SpectrumImportExportType.FromFile, 0},
                {SpectrumImportExportType.Parametrized, 1}
            };
    }
}