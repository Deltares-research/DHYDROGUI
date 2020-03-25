using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class SpectrumTypeDescriptionTest : EnumDescriptionTestFixture<SpectrumType>
    {
        protected override IDictionary<SpectrumType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<SpectrumType, string>
            {
                {SpectrumType.Parametrized, "parametric"},
                {SpectrumType.FromFile, "from file"},
            };
    }
}