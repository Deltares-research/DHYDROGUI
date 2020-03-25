using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class ShapeTypeTest : EnumValuesTestFixture<ShapeType>
    {
        protected override IDictionary<ShapeType, int> ExpectedValueForEnumValues =>
            new Dictionary<ShapeType, int>
            {
                {ShapeType.Gauss, 0},
                {ShapeType.Jonswap, 1},
                {ShapeType.PiersonMoskowitz, 2},
            };
    }
}