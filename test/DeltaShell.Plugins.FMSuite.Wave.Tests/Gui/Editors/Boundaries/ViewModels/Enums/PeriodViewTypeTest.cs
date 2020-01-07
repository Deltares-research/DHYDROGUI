using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.Enums
{
    [TestFixture]
    public class PeriodViewTypeTest :
        EnumValuesTestFixture<PeriodViewType>
    {
        protected override IDictionary<PeriodViewType, int> ExpectedValueForEnumValues =>
            new Dictionary<PeriodViewType, int>
            {
                {PeriodViewType.Peak, 1},
                {PeriodViewType.Mean, 2}
            };
    }
}