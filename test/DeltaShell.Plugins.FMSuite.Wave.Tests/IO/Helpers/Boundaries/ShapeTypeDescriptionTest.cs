using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture]
    public class ShapeTypeDescriptionTest : EnumDescriptionTestFixture<ShapeType>
    {
        protected override IDictionary<ShapeType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<ShapeType, string>
            {
                {ShapeType.Jonswap, "jonswap"},
                {ShapeType.PiersonMoskowitz, "pierson-moskowitz"},
                {ShapeType.Gauss, "gauss"},
            };
    }
}