using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.BndExtForce;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    public class BndExtForceDataFileTypeTest : EnumDescriptionTestFixture<BndExtForceDataFileType>
    {
        protected override IDictionary<BndExtForceDataFileType, string> ExpectedDescriptionForEnumValues
            => new Dictionary<BndExtForceDataFileType, string>
            {
                { BndExtForceDataFileType.None, "" },
                { BndExtForceDataFileType.BcAscii, "bcascii" },
                { BndExtForceDataFileType.Uniform, "uniform" },
                { BndExtForceDataFileType.UniMagDir, "unimagdir" },
                { BndExtForceDataFileType.ArcInfo, "arcinfo" },
                { BndExtForceDataFileType.SpiderWeb, "spiderweb" },
                { BndExtForceDataFileType.CurviGrid, "curvigrid" },
                { BndExtForceDataFileType.NetCDF, "netcdf" }
            };
    }
}