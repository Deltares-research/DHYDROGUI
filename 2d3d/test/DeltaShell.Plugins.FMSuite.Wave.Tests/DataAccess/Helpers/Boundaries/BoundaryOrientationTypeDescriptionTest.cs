using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class BoundaryOrientationTypeDescriptionTest : EnumDescriptionTestFixture<BoundaryOrientationType>
    {
        protected override IDictionary<BoundaryOrientationType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<BoundaryOrientationType, string>()
            {
                {BoundaryOrientationType.East, KnownWaveBoundariesFileConstants.EastBoundaryOrientationType},
                {BoundaryOrientationType.NorthEast, KnownWaveBoundariesFileConstants.NorthEastBoundaryOrientationType},
                {BoundaryOrientationType.North, KnownWaveBoundariesFileConstants.NorthBoundaryOrientationType},
                {BoundaryOrientationType.NorthWest, KnownWaveBoundariesFileConstants.NorthWestBoundaryOrientationType},
                {BoundaryOrientationType.West, KnownWaveBoundariesFileConstants.WestBoundaryOrientationType},
                {BoundaryOrientationType.SouthWest, KnownWaveBoundariesFileConstants.SouthWestBoundaryOrientationType},
                {BoundaryOrientationType.South, KnownWaveBoundariesFileConstants.SouthBoundaryOrientationType},
                {BoundaryOrientationType.SouthEast, KnownWaveBoundariesFileConstants.SouthEastBoundaryOrientationType}
            };
    }
}