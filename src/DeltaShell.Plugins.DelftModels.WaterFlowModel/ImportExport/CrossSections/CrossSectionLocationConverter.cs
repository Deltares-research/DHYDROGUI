using System;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public static class CrossSectionLocationConverter
    {
        public static IEnumerable<ICrossSectionLocation> Convert(IList<DelftIniCategory> categories, List<string> errorMessages)
        {
            var crossSectionLocations = new List<ICrossSectionLocation>();

            var selectedCategories = categories.Where(category => category.Name == CrossSectionRegion.IniHeader).ToList();

            selectedCategories.ForEach(category =>
            {
                try
                {
                    var generatedCrossSectionLocation = ConvertToCrossSectionLocation(category);
                    ValidateGeneratedCrossSectionLocation(generatedCrossSectionLocation, crossSectionLocations);
                    crossSectionLocations.Add(generatedCrossSectionLocation);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            });
            return crossSectionLocations;
        }

        private static ICrossSectionLocation ConvertToCrossSectionLocation(IDelftIniCategory category)
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

        private static void ValidateGeneratedCrossSectionLocation(ICrossSectionLocation crossSectionLocation, IList<ICrossSectionLocation> crossSectionLocations)
        {
            if (crossSectionLocation.IsDuplicateIn(crossSectionLocations))
                throw new Exception($"Cross section location with id {crossSectionLocation.Name} already exists, there cannot be any duplicate cross section location ids");
        }

        private static bool IsDuplicateIn(this ICrossSectionLocation crossSectionLocation, IList<ICrossSectionLocation> crossSectionLocations)
        {
            return crossSectionLocations.Contains(crossSectionLocation) || crossSectionLocations.Any(n => n.Name == crossSectionLocation.Name);
        }

    }
}
