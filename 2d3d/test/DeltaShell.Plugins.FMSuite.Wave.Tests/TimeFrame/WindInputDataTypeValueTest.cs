using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.TimeFrame
{
    [TestFixture]
    public class WindInputDataTypeValueTest : EnumValuesTestFixture<WindInputDataType>
    {
        protected override IDictionary<WindInputDataType, int> ExpectedValueForEnumValues => new Dictionary<WindInputDataType, int>
        {
            {WindInputDataType.Constant, 1},
            {WindInputDataType.TimeVarying, 2},
            {WindInputDataType.FileBased, 3},
        };
    }
}