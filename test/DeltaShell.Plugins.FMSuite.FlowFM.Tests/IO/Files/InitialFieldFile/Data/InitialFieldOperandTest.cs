using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Data
{
    [TestFixture]
    public class InitialFieldOperandTest : EnumDescriptionTestFixture<InitialFieldOperand>
    {
        protected override IDictionary<InitialFieldOperand, string> ExpectedDescriptionForEnumValues => new Dictionary<InitialFieldOperand, string>
        {
            { InitialFieldOperand.Override, "O" },
            { InitialFieldOperand.Append, "A" },
            { InitialFieldOperand.Add, "+" },
            { InitialFieldOperand.Multiply, "*" },
            { InitialFieldOperand.Maximum, "X" },
            { InitialFieldOperand.Minimum, "N" }
        };
    }
}