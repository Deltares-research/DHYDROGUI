using System.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Enums
{
    [TestFixture]
    public class ForcingViewTypeTest :
        EnumValuesTestFixture<ForcingViewType>
    {
        protected override IDictionary<ForcingViewType, int> ExpectedValueForEnumValues =>
            new Dictionary<ForcingViewType, int>
            {
                {ForcingViewType.Constant, 1},
                {ForcingViewType.TimeSeries, 2},
                {ForcingViewType.FileBased, 3}
            };

        [TestCase(ForcingViewType.Constant, "Parametrized (Constant)")]
        [TestCase(ForcingViewType.TimeSeries, "Parametrized (Time Series)")]
        [TestCase(ForcingViewType.FileBased, "Spectrum based (From file)")]
        public void ForcingViewType_GetDescription_ReturnsCorrectDescription(ForcingViewType forcingViewType, string expectedDescription)
        {
            Assert.That(forcingViewType.GetDescription(), Is.EqualTo(expectedDescription));
        }
    }
}