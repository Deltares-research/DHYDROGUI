using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.TimeFrame
{
    [TestFixture]
    public class HydrodynamicsInputDataTypeDescriptionTest : EnumDescriptionTestFixture<HydrodynamicsInputDataType>
    {
        protected override IDictionary<HydrodynamicsInputDataType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<HydrodynamicsInputDataType, string>
            {
                {HydrodynamicsInputDataType.Constant, "Constant"},
                {HydrodynamicsInputDataType.TimeVarying, "Per Timepoint"},
            };
    }
}