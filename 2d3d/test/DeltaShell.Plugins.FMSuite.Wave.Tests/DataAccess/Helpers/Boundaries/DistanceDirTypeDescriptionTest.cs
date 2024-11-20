using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class DistanceDirTypeDescriptionTest : EnumDescriptionTestFixture<DistanceDirType>
    {
        protected override IDictionary<DistanceDirType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<DistanceDirType, string>
            {
                {DistanceDirType.CounterClockwise, KnownWaveBoundariesFileConstants.CounterClockwiseDistanceDirType},
                {DistanceDirType.Clockwise, KnownWaveBoundariesFileConstants.ClockwiseDistanceDirType}
            };
    }
}