using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Enums
{
    // TODO (MWT): Add tests for verifying descriptions
    // TODO (MWT): Move descriptions to Resources
    [TestFixture]
    public class ForcingViewTypeTest :
        EnumValuesTestFixture<ForcingViewType>
    {
        protected override IDictionary<ForcingViewType, int> ExpectedValueForEnumValues =>
            new Dictionary<ForcingViewType, int>
            {
                {ForcingViewType.Constant, 1},
                {ForcingViewType.TimeSeries, 2},
                {ForcingViewType.FileBased, 3},
            };
    }
}