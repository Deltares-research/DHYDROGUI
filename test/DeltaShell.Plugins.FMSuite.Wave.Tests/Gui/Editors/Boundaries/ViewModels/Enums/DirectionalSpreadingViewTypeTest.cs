using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.Enums
{
    [TestFixture]
    public class DirectionalSpreadingViewTypeTest :
        EnumValuesTestFixture<DirectionalSpreadingViewType>
    {
        protected override IDictionary<DirectionalSpreadingViewType, int> ExpectedValueForEnumValues =>
            new Dictionary<DirectionalSpreadingViewType, int>
            {
                {DirectionalSpreadingViewType.Power, 1},
                {DirectionalSpreadingViewType.Degrees, 2}
            };
    }
}