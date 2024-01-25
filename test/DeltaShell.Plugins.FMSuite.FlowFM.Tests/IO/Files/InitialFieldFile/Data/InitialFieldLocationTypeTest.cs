using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Data
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