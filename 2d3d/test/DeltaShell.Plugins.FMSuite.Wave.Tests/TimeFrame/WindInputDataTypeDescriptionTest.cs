using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.TimeFrame
{
    [TestFixture]
    public class WindInputDataTypeDescriptionTest : EnumDescriptionTestFixture<WindInputDataType>
    {
        protected override IDictionary<WindInputDataType, string> ExpectedDescriptionForEnumValues => new Dictionary<WindInputDataType, string>
        {
            {WindInputDataType.Constant, "Constant"},
            {WindInputDataType.TimeVarying, "Per Timepoint"},
            {WindInputDataType.FileBased, "From File"},
        };
    }
}