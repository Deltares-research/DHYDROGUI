using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceOperandTest : EnumDescriptionTestFixture<BndExtForceOperand>
    {
        protected override IDictionary<BndExtForceOperand, string> ExpectedDescriptionForEnumValues
            => new Dictionary<BndExtForceOperand, string>
            {
                { BndExtForceOperand.None, "" },
                { BndExtForceOperand.Overwrite, "O" },
                { BndExtForceOperand.Append, "A" },
                { BndExtForceOperand.Add, "+" },
                { BndExtForceOperand.Multiply, "*" },
                { BndExtForceOperand.Maximum, "X" },
                { BndExtForceOperand.Minimum, "N" }
            };
    }
}