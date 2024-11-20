using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridSideTest : EnumValuesTestFixture<GridSide>
    {
        protected override IDictionary<GridSide, int> ExpectedValueForEnumValues =>
            new Dictionary<GridSide, int>
            {
                {GridSide.West, 1},
                {GridSide.North, 2},
                {GridSide.East, 3},
                {GridSide.South, 4}
            };
    }
}