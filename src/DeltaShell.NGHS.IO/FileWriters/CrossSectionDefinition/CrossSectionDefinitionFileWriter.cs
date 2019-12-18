using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class CrossSectionDefinitionFileWriter
    {
        public static bool ContainsAnyCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.GetNetworkCrossSectionDefinitions().Any();
        }
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
            
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }
        
        private static IEnumerable<ICrossSectionDefinition> GetNetworkCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.CrossSections.Select(GetCrossSectionDefinition)
                .Concat(network.BridgeCrossSectionDefinitions())
                .Concat(network.CulvertCrossSectionDefinitions())
                .Concat(network.PipeCrossSectionDefinitions());
        }
        
        private static ICrossSectionDefinition GetCrossSectionDefinition(ICrossSection crossSection)
        {
            var crossSectionDefinition = crossSection.Definition;
            var definition = crossSectionDefinition.IsProxy
                ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition
                : crossSectionDefinition;
            return definition;
        }

        private static IEnumerable<ICrossSectionDefinition> BridgeCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.Bridges.Where(b => b.CrossSectionDefinition != null).Select(b => b.CrossSectionDefinition);
        }
        
        private static IEnumerable<ICrossSectionDefinition> CulvertCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.Culverts.Where(c => c.CrossSectionDefinition != null).Select(c => c.CrossSectionDefinition);
        }

        
        private static IEnumerable<ICrossSectionDefinition> PipeCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.Pipes.Where(p => p.CrossSectionDefinition != null).Select(p => p.CrossSectionDefinition.IsProxy ? ((CrossSectionDefinitionProxy)p.CrossSectionDefinition).InnerDefinition:p.CrossSectionDefinition);
        }
    }
}