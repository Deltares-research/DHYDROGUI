using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data
{
    [TestFixture]
    public class LateralForcingTypeTest : EnumDescriptionTestFixture<LateralForcingType>
    {
        protected override IDictionary<LateralForcingType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<LateralForcingType, string>
            {
                { LateralForcingType.Discharge, "discharge" },
                { LateralForcingType.Unsupported, string.Empty },
                { LateralForcingType.None, string.Empty },
            };
    }
}