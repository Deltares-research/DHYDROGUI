using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.InitialField;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldDataFileTypeTest : EnumDescriptionTestFixture<InitialFieldDataFileType>
    {
        protected override IDictionary<InitialFieldDataFileType, string> ExpectedDescriptionForEnumValues => new Dictionary<InitialFieldDataFileType, string>
        {
            { InitialFieldDataFileType.ArcInfo, "arcinfo" },
            { InitialFieldDataFileType.GeoTIFF, "GeoTIFF" },
            { InitialFieldDataFileType.Sample, "sample" },
            { InitialFieldDataFileType.OneDField, "1dField" },
            { InitialFieldDataFileType.Polygon, "polygon" },
            { InitialFieldDataFileType.None, "" }
        };
    }
}