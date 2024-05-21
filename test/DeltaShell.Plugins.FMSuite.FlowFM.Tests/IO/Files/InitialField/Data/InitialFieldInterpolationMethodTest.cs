using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField.Data
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