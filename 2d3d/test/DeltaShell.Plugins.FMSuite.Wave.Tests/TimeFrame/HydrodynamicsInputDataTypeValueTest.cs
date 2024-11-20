using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.TimeFrame
{
    [TestFixture]
    public class HydrodynamicsInputDataTypeValueTest : EnumValuesTestFixture<HydrodynamicsInputDataType>
    {
        protected override IDictionary<HydrodynamicsInputDataType, int> ExpectedValueForEnumValues =>
            new Dictionary<HydrodynamicsInputDataType, int>
            {
                {HydrodynamicsInputDataType.Constant, 1},
                {HydrodynamicsInputDataType.TimeVarying, 2},
            };
    }
}