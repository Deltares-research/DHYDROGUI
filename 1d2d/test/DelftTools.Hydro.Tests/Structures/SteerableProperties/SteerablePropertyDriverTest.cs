using System.Collections.Generic;
using DelftTools.Hydro.Structures.SteerableProperties;
using DeltaShell.NGHS.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures.SteerableProperties
{
    [TestFixture]
    public class SteerablePropertyDriverTest : EnumValuesTestFixture<SteerablePropertyDriver>
    {
        protected override IDictionary<SteerablePropertyDriver, int> ExpectedValueForEnumValues => new Dictionary<SteerablePropertyDriver, int>()
        {
            { SteerablePropertyDriver.Constant, 0 },
            { SteerablePropertyDriver.TimeSeries, 1 },
        };
    }
}