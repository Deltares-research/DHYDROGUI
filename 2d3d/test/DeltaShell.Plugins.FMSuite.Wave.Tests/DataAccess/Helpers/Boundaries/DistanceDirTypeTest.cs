using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class DistanceDirTypeTest : EnumValuesTestFixture<DistanceDirType>
    {
        protected override IDictionary<DistanceDirType, int> ExpectedValueForEnumValues =>
            new Dictionary<DistanceDirType, int>
            {
                {DistanceDirType.CounterClockwise, 0},
                {DistanceDirType.Clockwise, 1}
            };
    }
}