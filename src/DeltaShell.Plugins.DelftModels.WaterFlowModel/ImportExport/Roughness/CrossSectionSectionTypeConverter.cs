using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public static class CrossSectionSectionTypeConverter
    {
        public static CrossSectionSectionType Convert(IList<DelftIniCategory> categories, IList<string> errorMessages)
        {
            foreach (var roughnessCategory in categories.Where(category => category.Name == RoughnessDataRegion.ContentIniHeader))
            {
                try
                {
                    return ConvertToCrossSectionSectionType(roughnessCategory);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            return null;
        }

        private static CrossSectionSectionType ConvertToCrossSectionSectionType(DelftIniCategory roughnessCategory)
        {
            var idProperty = roughnessCategory.ReadProperty<string>(RoughnessDataRegion.SectionId.Key);

            return new CrossSectionSectionType
            {
                Name = idProperty
            };
        }
    }
}
