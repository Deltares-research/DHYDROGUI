using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public static class CrossSectionDefinitionConverter
    {
        public static IList<ICrossSectionDefinition> Convert(IList<DelftIniCategory> categories)
        {
            var selectedCategories = categories.Where(category => category.Name == DefinitionRegion.Header).ToList();

            return selectedCategories.Select(CovertToCrossSectionDefinition).ToList();
        }

        private static ICrossSectionDefinition CovertToCrossSectionDefinition(IDelftIniCategory category)
        {
            var type = category.ReadProperty<string>(DefinitionRegion.DefinitionType.Key);

            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderCrossSection(type);

            var crossSectionDefinition = definitionReader?.ReadCrossSectionDefinition(category);

            if (category.ReadProperty<int>(DefinitionRegion.IsShared.Key, true) == 1)
            {
                return new CrossSectionDefinitionProxy(crossSectionDefinition);
            }

            return crossSectionDefinition;
        }
    }
}
