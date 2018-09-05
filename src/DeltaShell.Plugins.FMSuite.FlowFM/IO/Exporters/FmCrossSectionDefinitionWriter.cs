using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public static class FmCrossSectionDefinitionWriter
    {
        public static void WriteFile(string filePath, WaterFlowFMModel model)
        {
            FileUtils.DeleteIfExists(filePath);

            var iniCategories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion,
                                      GeneralRegion.CrossSectionDefinitionsMinorVersion,
                                      GeneralRegion.FileTypeName.CrossSectionDefinition)
            };

            iniCategories.AddCategoriesFromNetworkCrossSections(model.Network);

            new IniFileWriter().WriteIniFile(iniCategories, filePath);
        }

        private static void AddCategoriesFromNetworkCrossSections(this ICollection<DelftIniCategory> categories, IHydroNetwork network)
        {
            var processedCsDefinitionNames = new List<string>();
            
            var crossSectionDefinitions = GetNetworkCrossSectionDefinitions(network);
            foreach (var crossSectionDefinition in crossSectionDefinitions)
            {
                if (processedCsDefinitionNames.Contains(crossSectionDefinition.Name)) continue;

                var iniCategory = ConstructIniCategoryFromCrossSectionDefinition(crossSectionDefinition, network);
                if (iniCategory == null) continue;

                categories.Add(iniCategory);
                processedCsDefinitionNames.Add(crossSectionDefinition.Name);
            }
        }

        private static IEnumerable<ICrossSectionDefinition> GetNetworkCrossSectionDefinitions(IHydroNetwork network)
        {
            var crossSectionDefinitions = GetNetworkCrossSections(network).Select(GetCrossSectionDefinition).ToList();
            crossSectionDefinitions.AddRange(network.Pipes.Select(p => p.CrossSectionDefinition));
            return crossSectionDefinitions;
        }

        private static IEnumerable<ICrossSection> GetNetworkCrossSections(IHydroNetwork network)
        {
            var crossSections = network.CrossSections.ToList();
            AddCulvertCrossSectionDefinitions(crossSections, network);
            AddBridgeCrossSectionDefinitions(crossSections, network);

            return crossSections;
        }

        private static ICrossSectionDefinition GetCrossSectionDefinition(ICrossSection crossSection)
        {
            var crossSectionDefinition = crossSection.Definition;
            var definition = crossSectionDefinition.IsProxy
                ? ((CrossSectionDefinitionProxy) crossSectionDefinition).InnerDefinition
                : crossSectionDefinition;
            return definition;
        }

        private static DelftIniCategory ConstructIniCategoryFromCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, IHydroNetwork network)
        {
            var iniCategoryGenerator = DefinitionIniCategoryGeneratorFactory.GetIniCategoryGenerator(crossSectionDefinition);
            if (iniCategoryGenerator == null) return null;

            var iniCategory = iniCategoryGenerator.CreateDefinitionRegion(crossSectionDefinition);
            iniCategory.AddNetworkRelatedProperties(network, crossSectionDefinition);

            return iniCategory;
        }

        private static void AddNetworkRelatedProperties(this DelftIniCategory iniCategory, IHydroNetwork network, ICrossSectionDefinition crossSectionDefinition)
        {
            switch (crossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.GeometryBased:
                case CrossSectionType.YZ:
                    // TODO: add roughness, works differently on an FM model. Look at CrossSectionDefinitionFileWriter for an example.
                    //iniCategory = AddRoughnessDataToFileContent(iniCategory, crossSection, waterFlowModel1D.RoughnessSections, waterFlowModel1D.UseReverseRoughness);
                    break;
                case CrossSectionType.ZW:
                case CrossSectionType.Standard:
                    //add groundlevel
                    iniCategory.AddGroundLayer(crossSectionDefinition.Name, network);
                    break;
            }

            if (network.SharedCrossSectionDefinitions.Contains(crossSectionDefinition))
                iniCategory.AddProperty(DefinitionPropertySettings.IsShared.Key, 1, DefinitionPropertySettings.IsShared.Description);
        }

        private static void AddBridgeCrossSectionDefinitions(List<ICrossSection> crossSections, IHydroNetwork network)
        {
            var bridgeCrossSectionDefinitions = network.Bridges.Where(b => b.CrossSectionDefinition != null).Select(b => b.CrossSectionDefinition);
            crossSections.AddRange(bridgeCrossSectionDefinitions.Select(csd => new CrossSection(csd) {Name = csd.Name}));
        }

        private static void AddCulvertCrossSectionDefinitions(List<ICrossSection> crossSections, IHydroNetwork network)
        {
            var culvertCrossSectionDefinitions = network.Culverts.Select(c => c.CrossSectionDefinition);
            crossSections.AddRange(culvertCrossSectionDefinitions.Select(csd => new CrossSection(csd) {Name = csd.Name}));
        }

        private static void AddGroundLayer(this DelftIniCategory iniCategory, string crossSectionDefinitionName, IHydroNetwork network)
        {
            var groundlayerUsed = 0; // default value
            var groundlayer = 0.0;  // default value
            var structure = network.Structures.FirstOrDefault(s => IsCulvertWithRightName(s, crossSectionDefinitionName) ||
                                                                   IsBridgeWithRightName(s, crossSectionDefinitionName)) as IGroundLayer;
            if (structure != null)
            {
                groundlayerUsed = Convert.ToInt32(structure.GroundLayerEnabled);
                groundlayer = structure.GroundLayerThickness;
            }

            iniCategory.AddProperty(DefinitionPropertySettings.GroundlayerUsed, groundlayerUsed);
            iniCategory.AddProperty(DefinitionPropertySettings.Groundlayer, groundlayer);
        }

        private static bool IsCulvertWithRightName(IStructure1D structure, string crossSectionDefinitionName)
        {
            var culvert = structure as ICulvert;
            return culvert?.CrossSectionDefinition != null
                   && culvert.CrossSectionDefinition.Name == crossSectionDefinitionName;
        }

        private static bool IsBridgeWithRightName(IStructure1D structure, string crossSectionDefinitionName)
        {
            var bridge = structure as IBridge;
            return bridge?.CrossSectionDefinition != null
                   && bridge.CrossSectionDefinition.Name == crossSectionDefinitionName;
        }
    }
}
