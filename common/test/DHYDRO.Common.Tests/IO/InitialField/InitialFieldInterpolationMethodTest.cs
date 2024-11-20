using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.InitialField;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldInterpolationMethodTest : EnumDescriptionTestFixture<InitialFieldInterpolationMethod>
    {
        protected override IDictionary<InitialFieldInterpolationMethod, string> ExpectedDescriptionForEnumValues => new Dictionary<InitialFieldInterpolationMethod, string>
        {
            { InitialFieldInterpolationMethod.Constant, "constant" },
            { InitialFieldInterpolationMethod.Triangulation, "triangulation" },
            { InitialFieldInterpolationMethod.Averaging, "averaging" },
            { InitialFieldInterpolationMethod.None, "" }
        };
    }
}