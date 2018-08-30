using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public static class FmCrossSectionDefinitionWriter
    {
        public static void WriteFile(string filePath, IHydroNetwork network)
        {
            FileUtils.DeleteIfExists(filePath);

            //var categories = new List<DelftIniCategory>()
            //{
            //    GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion,
            //                          GeneralRegion.CrossSectionDefinitionsMinorVersion,
            //                          GeneralRegion.FileTypeName.CrossSectionDefinition),
            //};

            /*var crossSections = network.CrossSections;
            var sharedCrossSections = network.SharedCrossSectionDefinitions;
            var processedCsDefinitions = new List<string>();
            foreach (var crossSection in crossSections)
            {
                var crossSectionDefinition = crossSection.Definition;
                var definition = crossSectionDefinition.IsProxy
                    ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition
                    : crossSectionDefinition;

                var iniCategoryGenerator = CrossSectionDefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(crossSectionDefinition);

                if (definitionGeneratorCrossSectionDefinition == null) continue;

                string csDefinitionId = definition.Name;
                if (processedCsDefinitions.Contains(csDefinitionId)) continue;

                var definitionRegion = definitionGeneratorCrossSectionDefinition.CreateDefinitionRegion(definition);

                switch (crossSection.CrossSectionType)
                {
                    case CrossSectionType.GeometryBased:
                    case CrossSectionType.YZ:
                        //add roughness
                        definitionRegion = AddRoughnessDataToFileContent(definitionRegion, crossSection, waterFlowModel1D.RoughnessSections, waterFlowModel1D.UseReverseRoughness);
                        break;
                    case CrossSectionType.ZW:
                    case CrossSectionType.Standard:
                        //add groundlevel
                        definitionRegion = AddGroundLayer(definitionRegion, crossSectionDefinition.Name, waterFlowModel1D.Network);
                        break;
                }
                if (sharedCrossSections.Contains(definition)) definitionRegion.AddProperty(DefinitionPropertySettings.IsShared.Key, 1, DefinitionPropertySettings.IsShared.Description);
                categories.Add(definitionRegion);
                processedCsDefinitions.Add(csDefinitionId);
            }

            new IniFileWriter().WriteIniFile(categories, targetFile);*/

        }
    }
}
