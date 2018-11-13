using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public static class CrossSectionLocationConverter
    {
        public static IList<ICrossSectionLocation> Convert(IList<DelftIniCategory> categories)
        {
            var selectedCategories = categories.Where(category => category.Name == CrossSectionRegion.IniHeader).ToList();

            return selectedCategories.Select(CovertToCrossSectionLocation).ToList();
        }

        private static ICrossSectionLocation CovertToCrossSectionLocation(IDelftIniCategory category)
        {
            var name = category.ReadProperty<string>(LocationRegion.Id.Key);
            var branchName = category.ReadProperty<string>(LocationRegion.BranchId.Key);
            var chainage = category.ReadProperty<double>(LocationRegion.Chainage.Key);
            var shift = category.ReadProperty<double>(CrossSectionRegion.Shift.Key);
            var definition = category.ReadProperty<string>(CrossSectionRegion.Definition.Key);
            var longName = category.ReadProperty<string>(LocationRegion.Name.Key, true);

            var crossSectionLocation = new CrossSectionLocation(name)
            {
                BranchName = branchName,
                Chainage = chainage,
                Shift = shift,
                Definition = definition,
                LongName = longName
            };

            return crossSectionLocation;
        }

    }
}
