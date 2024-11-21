using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceLocationTypeTest : EnumDescriptionTestFixture<BndExtForceLocationType>
    {
        protected override IDictionary<BndExtForceLocationType, string> ExpectedDescriptionForEnumValues
            => new Dictionary<BndExtForceLocationType, string>
            {
                { BndExtForceLocationType.OneD, "1d" },
                { BndExtForceLocationType.TwoD, "2d" },
                { BndExtForceLocationType.All, "all" }
            };
    }
}