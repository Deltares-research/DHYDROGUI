using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class SpectrumTypeTest : EnumValuesTestFixture<SpectrumType>
    {
        protected override IDictionary<SpectrumType, int> ExpectedValueForEnumValues =>
            new Dictionary<SpectrumType, int>
            {
                {SpectrumType.FromFile, 0},
                {SpectrumType.Parametrized, 1},
            };
    }
}