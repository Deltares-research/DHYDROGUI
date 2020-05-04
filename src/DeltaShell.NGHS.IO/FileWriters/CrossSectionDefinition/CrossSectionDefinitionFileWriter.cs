using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class CrossSectionDefinitionFileWriter
    {
        public static void WriteFile(string targetFile, IHydroNetwork network)
        {
            var crossSectionDefinitions = network.GetNetworkCrossSectionDefinitions();
            var sharedCrossSectionDefinitions = network.SharedCrossSectionDefinitions;

            WriteFile(targetFile, crossSectionDefinitions, sharedCrossSectionDefinitions);
        }

        private static void WriteFile(string targetFile, IEnumerable<ICrossSectionDefinition> crossSectionDefinitions, IEnumerable<ICrossSectionDefinition> sharedCrossSectionDefinitions)
        {
            if (File.Exists(targetFile)) File.Delete(targetFile);

            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion, 
                    GeneralRegion.CrossSectionDefinitionsMinorVersion, 
                    GeneralRegion.FileTypeName.CrossSectionDefinition),
            };

            var processedCsDefinitions = new List<string>();
            foreach (var crossSectionDefinition in crossSectionDefinitions)
            {
                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory.GetDefinitionGeneratorCrossSection(crossSectionDefinition);
                
                if (definitionGeneratorCrossSectionDefinition == null) continue;

                var csDefinitionId = crossSectionDefinition.Name;
                if (processedCsDefinitions.Contains(csDefinitionId)) continue;

                var definitionRegion = definitionGeneratorCrossSectionDefinition.CreateDefinitionRegion(crossSectionDefinition);

                categories.Add(definitionRegion);
                processedCsDefinitions.Add(csDefinitionId);
            }

            foreach (var sharedCrossSectionDefinition in sharedCrossSectionDefinitions)
            {
                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory.GetDefinitionGeneratorCrossSection(sharedCrossSectionDefinition);

                if (definitionGeneratorCrossSectionDefinition == null) continue;

                var csDefinitionId = sharedCrossSectionDefinition.Name;
                if (processedCsDefinitions.Contains(csDefinitionId)) continue;

                var definitionRegion = definitionGeneratorCrossSectionDefinition.CreateDefinitionRegion(sharedCrossSectionDefinition);
                definitionRegion.AddProperty(DefinitionPropertySettings.IsShared.Key,true);
                categories.Add(definitionRegion);
            }

            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        
    }
}