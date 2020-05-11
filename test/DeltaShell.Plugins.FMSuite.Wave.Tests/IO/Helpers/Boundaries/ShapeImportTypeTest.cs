using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class ShapeImportTypeTest : EnumValuesTestFixture<ShapeImportType>
    {
        protected override IDictionary<ShapeImportType, int> ExpectedValueForEnumValues =>
            new Dictionary<ShapeImportType, int>
            {
                {ShapeImportType.Gauss, 0},
                {ShapeImportType.Jonswap, 1},
                {ShapeImportType.PiersonMoskowitz, 2}
            };
    }
}