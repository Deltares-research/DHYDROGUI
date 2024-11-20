using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data
{
    [TestFixture]
    public class LateralLocationTypeTest : EnumDescriptionTestFixture<LateralLocationType>
    {
        protected override IDictionary<LateralLocationType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<LateralLocationType, string>
            {
                { LateralLocationType.TwoD, "2d" },
                { LateralLocationType.Unsupported, string.Empty },
                { LateralLocationType.None, string.Empty },
            };
    }
}