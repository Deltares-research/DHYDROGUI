using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions
{
    [TestFixture]
    public class BoundaryConditionPeriodTypeTest : EnumValuesTestFixture<BoundaryConditionPeriodType>
    {
        protected override IDictionary<BoundaryConditionPeriodType, int> ExpectedValueForEnumValues =>
            new Dictionary<BoundaryConditionPeriodType, int>
            {
                {BoundaryConditionPeriodType.Peak, 1},
                {BoundaryConditionPeriodType.Mean, 2}
            };
    }
}