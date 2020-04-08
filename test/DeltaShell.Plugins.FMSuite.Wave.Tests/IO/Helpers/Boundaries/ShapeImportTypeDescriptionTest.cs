using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class ShapeImportTypeDescriptionTest : EnumDescriptionTestFixture<ShapeImportType>
    {
        protected override IDictionary<ShapeImportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<ShapeImportType, string>
            {
                {ShapeImportType.Jonswap, "jonswap"},
                {ShapeImportType.PiersonMoskowitz, "pierson-moskowitz"},
                {ShapeImportType.Gauss, "gauss"},
            };
    }
}