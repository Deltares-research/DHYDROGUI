using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField.Data
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