using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    public static class SalinityConverter
    {
        public static string Convert(IEnumerable<DelftIniCategory> categories)
        {
            var mouthCategory = categories.FirstOrDefault(cat => cat.Name == "Mouth");
            var estuaryMouthNodeId = mouthCategory.ReadProperty<string>("nodeId");

            return estuaryMouthNodeId;
        }
    }
}
