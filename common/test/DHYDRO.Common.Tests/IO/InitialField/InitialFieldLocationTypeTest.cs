using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.InitialField;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldLocationTypeTest : EnumDescriptionTestFixture<InitialFieldLocationType>
    {
        protected override IDictionary<InitialFieldLocationType, string> ExpectedDescriptionForEnumValues => new Dictionary<InitialFieldLocationType, string>
        {
            { InitialFieldLocationType.OneD, "1d" },
            { InitialFieldLocationType.TwoD, "2d" },
            { InitialFieldLocationType.All, "all" }
        };
    }
}