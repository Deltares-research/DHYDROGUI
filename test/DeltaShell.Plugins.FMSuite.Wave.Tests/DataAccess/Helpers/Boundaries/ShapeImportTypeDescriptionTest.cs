using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class ShapeImportTypeDescriptionTest : EnumDescriptionTestFixture<ShapeImportType>
    {
        protected override IDictionary<ShapeImportType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<ShapeImportType, string>
            {
                {ShapeImportType.Jonswap, KnownWaveBoundariesFileConstants.JonswapShape},
                {ShapeImportType.PiersonMoskowitz, KnownWaveBoundariesFileConstants.PiersonMoskowitzShape},
                {ShapeImportType.Gauss, KnownWaveBoundariesFileConstants.GaussShape}
            };
    }
}