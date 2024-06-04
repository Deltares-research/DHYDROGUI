using System.Collections.Generic;
using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.TestUtils;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
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