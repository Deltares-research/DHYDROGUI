using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    public static class SalinityConverter
    {
        public static string Convert(IEnumerable<DelftIniCategory> categories, IList<string> errorMessages)
        {
            var mouthCategory = categories.FirstOrDefault(cat => cat.Name == SalinityRegion.MouthHeader);
            if (mouthCategory == null)
            {
                errorMessages.Add($"Expected a category with name '{SalinityRegion.MouthHeader}' in the file 'Salinity.ini', but it was not present. Nothing was read from this file.");
                return null;
            }

            var estuaryMouthNodeId = mouthCategory.ReadProperty<string>(SalinityRegion.NodeId.Key, true);
            if (estuaryMouthNodeId == null)
            {
                errorMessages.Add($"Expected a property with name '{SalinityRegion.NodeId.Key}' under category {SalinityRegion.MouthHeader} in the file 'Salinity.ini', but it was not present. Reading of this property has been skipped.");
            }

            return estuaryMouthNodeId;
        }
    }
}
