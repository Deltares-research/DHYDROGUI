using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public static class CrossSectionDefinitionConverter
    {
        public static IEnumerable<ICrossSectionDefinition> Convert(IList<DelftIniCategory> categories, List<string> errorMessages)
        {
            var crossSectionDefinitions = new List<ICrossSectionDefinition>();

            var selectedCategories = categories.Where(category => category.Name == DefinitionRegion.Header).ToList();

            selectedCategories.ForEach(category =>
            {
                try
                {
                    var generatedCrossSectionDefinition = ConvertToCrossSectionDefinition(category);
                    ValidateGeneratedCrossSectionDefinition(generatedCrossSectionDefinition, crossSectionDefinitions);
                    crossSectionDefinitions.Add(generatedCrossSectionDefinition);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            });

            return crossSectionDefinitions;
        }

        private static void ValidateGeneratedCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, IList<ICrossSectionDefinition> crossSectionDefinitions)
        {
            if (crossSectionDefinition.IsDuplicateIn(crossSectionDefinitions))
                throw new Exception($"Cross section definition with id {crossSectionDefinition.Name} already exists, there cannot be any duplicate cross section definition ids");
        }

        private static ICrossSectionDefinition ConvertToCrossSectionDefinition(IDelftIniCategory category)
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

        private static bool IsDuplicateIn(this ICrossSectionDefinition crossSectionDefinition, IList<ICrossSectionDefinition> crossSectionDefinitions)
        {
            return crossSectionDefinitions.Contains(crossSectionDefinition) || crossSectionDefinitions.Any(n => n.Name == crossSectionDefinition.Name);
        }
    }
}
