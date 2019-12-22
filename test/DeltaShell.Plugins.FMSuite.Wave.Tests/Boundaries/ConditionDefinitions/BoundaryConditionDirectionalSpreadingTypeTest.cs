using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions
{
    [TestFixture]
    public class BoundaryConditionDirectionalSpreadingTypeTest : 
        EnumValuesTestFixture<BoundaryConditionDirectionalSpreadingType>
    {
        protected override IDictionary<BoundaryConditionDirectionalSpreadingType, int> ExpectedValueForEnumValues =>
            new Dictionary<BoundaryConditionDirectionalSpreadingType, int>
            {
                {BoundaryConditionDirectionalSpreadingType.Power, 1},
                {BoundaryConditionDirectionalSpreadingType.Degrees, 2}
            };
        
    }
}