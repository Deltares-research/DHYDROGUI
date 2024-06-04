using System.Collections.Generic;
using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.TestUtils;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
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