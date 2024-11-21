using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceInterpolationMethodTest : EnumDescriptionTestFixture<BndExtForceInterpolationMethod>
    {
        protected override IDictionary<BndExtForceInterpolationMethod, string> ExpectedDescriptionForEnumValues
            => new Dictionary<BndExtForceInterpolationMethod, string>
            {
                { BndExtForceInterpolationMethod.None, "" },
                { BndExtForceInterpolationMethod.LinearSpaceTime, "linearspacetime" },
                { BndExtForceInterpolationMethod.Constant, "constant" },
                { BndExtForceInterpolationMethod.Triangulation, "triangulation" },
                { BndExtForceInterpolationMethod.Averaging, "averaging" }
            };
    }
}