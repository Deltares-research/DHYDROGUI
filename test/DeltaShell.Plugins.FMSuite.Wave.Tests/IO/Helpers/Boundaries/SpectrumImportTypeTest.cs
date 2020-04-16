using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class SpectrumImportTypeTest : EnumValuesTestFixture<SpectrumImportType>
    {
        protected override IDictionary<SpectrumImportType, int> ExpectedValueForEnumValues =>
            new Dictionary<SpectrumImportType, int>
            {
                {SpectrumImportType.FromFile, 1},
                {SpectrumImportType.Parametrized, 2}
            };
    }
}