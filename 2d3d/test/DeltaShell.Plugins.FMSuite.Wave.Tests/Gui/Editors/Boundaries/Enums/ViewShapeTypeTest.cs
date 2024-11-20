using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Enums
{
    [TestFixture]
    public class ViewShapeTypeTest : EnumValuesTestFixture<ViewShapeType>
    {
        protected override IDictionary<ViewShapeType, int> ExpectedValueForEnumValues { get; } =
            new Dictionary<ViewShapeType, int>
            {
                {ViewShapeType.Gauss, 1},
                {ViewShapeType.Jonswap, 2},
                {ViewShapeType.PiersonMoskowitz, 3}
            };
    }
}