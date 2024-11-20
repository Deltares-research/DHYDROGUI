using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Enums
{
    [TestFixture]
    public class SpatialDefinitionViewTypeTest :
        EnumValuesTestFixture<SpatialDefinitionViewType>
    {
        protected override IDictionary<SpatialDefinitionViewType, int> ExpectedValueForEnumValues =>
            new Dictionary<SpatialDefinitionViewType, int>
            {
                {SpatialDefinitionViewType.Uniform, 1},
                {SpatialDefinitionViewType.SpatiallyVarying, 2}
            };
    }
}