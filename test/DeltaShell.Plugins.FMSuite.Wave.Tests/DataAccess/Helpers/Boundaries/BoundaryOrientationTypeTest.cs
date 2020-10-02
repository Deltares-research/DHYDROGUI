using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class BoundaryOrientationTypeTest : EnumValuesTestFixture<BoundaryOrientationType>
    {
        protected override IDictionary<BoundaryOrientationType, int> ExpectedValueForEnumValues =>
            new Dictionary<BoundaryOrientationType, int>()
            {
                {BoundaryOrientationType.East, 0},
                {BoundaryOrientationType.NorthEast, 1},
                {BoundaryOrientationType.North, 2},
                {BoundaryOrientationType.NorthWest, 3},
                {BoundaryOrientationType.West, 4},
                {BoundaryOrientationType.SouthWest, 5},
                {BoundaryOrientationType.South, 6},
                {BoundaryOrientationType.SouthEast, 7}
            };
    }
}