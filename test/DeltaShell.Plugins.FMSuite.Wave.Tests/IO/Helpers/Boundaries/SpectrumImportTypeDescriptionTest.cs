using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class SpectrumImportTypeDescriptionTest : EnumDescriptionTestFixture<SpectrumImportType>
    {
        protected override IDictionary<SpectrumImportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<SpectrumImportType, string>
            {
                {SpectrumImportType.Parametrized, "parametric"},
                {SpectrumImportType.FromFile, "from file"},
            };
    }
}