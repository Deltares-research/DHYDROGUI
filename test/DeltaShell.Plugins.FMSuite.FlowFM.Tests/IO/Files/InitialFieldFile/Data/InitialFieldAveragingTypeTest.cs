using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Data
{
    [TestFixture]
    public class InitialFieldAveragingTypeTest : EnumDescriptionTestFixture<InitialFieldAveragingType>
    {
        protected override IDictionary<InitialFieldAveragingType, string> ExpectedDescriptionForEnumValues => new Dictionary<InitialFieldAveragingType, string>
        {
            { InitialFieldAveragingType.Mean, "mean" },
            { InitialFieldAveragingType.NearestNb, "nearestNb" },
            { InitialFieldAveragingType.Max, "max" },
            { InitialFieldAveragingType.Min, "min" },
            { InitialFieldAveragingType.InverseDistance, "invDist" },
            { InitialFieldAveragingType.MinAbsolute, "minAbs" },
            { InitialFieldAveragingType.Median, "median" }
        };
    }
}