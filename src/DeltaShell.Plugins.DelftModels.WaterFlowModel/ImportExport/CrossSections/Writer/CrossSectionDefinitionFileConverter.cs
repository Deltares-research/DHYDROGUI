using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer
{
    public class CrossSectionDefinitionFileConverter
    {
        /// <summary>
        /// Converts a <see cref="WaterFlowModel1D"/> to a <see cref="DelftIniCategory"/>
        /// </summary>
        /// <param name="waterFlowModel1D">A model that contains cross sections and roughness sections.</param>
        /// <returns>A collection of <see cref="DelftIniCategory"/> objects representing cross section related data.</returns>
        public virtual IEnumerable<DelftIniCategory> Convert(WaterFlowModel1D waterFlowModel1D)
        {
            var categories = CreateCrossSectionDefinitionCategory();
            var network = waterFlowModel1D.Network;
            var crossSections = GatherCrossSectionFromNetwork(network);
            var sharedCrossSections = network.SharedCrossSectionDefinitions;

            var processedCsDefinitions = new List<string>();
            foreach (var crossSection in crossSections)
            {
                var definition = GetDefinition(crossSection);
                var definitionGeneratorCrossSectionDefinition =
                    DefinitionGeneratorFactory.GetDefinitionGeneratorCrossSection(definition,
                        crossSection.CrossSectionType);

                var csDefinitionId = definition.Name;
                if (processedCsDefinitions.Contains(csDefinitionId)) continue;

                var definitionRegion = definitionGeneratorCrossSectionDefinition.CreateDefinitionRegion(definition);
                definitionRegion = DetermineCrossSectionType(waterFlowModel1D, crossSection, definitionRegion);

                if (sharedCrossSections.Any(cs => cs.Name == definition.Name))
                {
                    definitionRegion.AddProperty(DefinitionRegion.IsShared.Key,
                        1, DefinitionRegion.IsShared.Description);
                }

                categories.Add(definitionRegion);
                processedCsDefinitions.Add(csDefinitionId);
            }

            return categories;
        }

        private static DelftIniCategory DetermineCrossSectionType(WaterFlowModel1D waterFlowModel1D,
            ICrossSection crossSection,
            DelftIniCategory definitionRegion)
        {
            switch (crossSection.CrossSectionType)
            {
                case CrossSectionType.GeometryBased:
                case CrossSectionType.YZ:
                    definitionRegion = RoughnessDataProcessor.AddRoughnessDataToFileContent(definitionRegion,
                        crossSection,
                        waterFlowModel1D.RoughnessSections, waterFlowModel1D.UseReverseRoughness);
                    break;
                case CrossSectionType.ZW:
                case CrossSectionType.Standard:
                    definitionRegion = AddGroundLayer(definitionRegion, crossSection.Definition.Name,
                        waterFlowModel1D.Network);
                    break;
            }

            return definitionRegion;
        }

        private static DelftIniCategory AddGroundLayer(DelftIniCategory iniCategory, string crossSectionDefinitionName,
            IHydroNetwork network)
        {
            var groundLayerUsed = 0; // default value
            var groundLayer = 0.0; // default value
            var structure = network.Structures.FirstOrDefault(s =>
                (s is ICulvert && ((ICulvert) s).CrossSectionDefinition.Name == crossSectionDefinitionName) ||
                (s is IBridge && ((IBridge) s).CrossSectionDefinition != null &&
                 ((IBridge) s).CrossSectionDefinition.Name == crossSectionDefinitionName)) as IGroundLayer;
            if (structure != null)
            {
                groundLayerUsed = System.Convert.ToInt32(structure.GroundLayerEnabled);
                groundLayer = structure.GroundLayerThickness;
            }

            iniCategory.AddProperty(DefinitionRegion.GroundlayerUsed.Key, groundLayerUsed,
                DefinitionRegion.GroundlayerUsed.Description);
            iniCategory.AddProperty(DefinitionRegion.Groundlayer.Key, groundLayer,
                DefinitionRegion.Groundlayer.Description, DefinitionRegion.Groundlayer.Format);
            return iniCategory;
        }

        private static ICrossSectionDefinition GetDefinition(ICrossSection crossSection)
        {
            ICrossSectionDefinition definition;
            if (crossSection.Definition.IsProxy)
            {
                var innerDefinition = (CrossSectionDefinitionProxy) crossSection.Definition;
                definition = innerDefinition.InnerDefinition;
            }
            else
            {
                definition = crossSection.Definition;
            }

            return definition;
        }

        private static List<DelftIniCategory> CreateCrossSectionDefinitionCategory()
        {
            var categories = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion,
                    GeneralRegion.CrossSectionDefinitionsMinorVersion,
                    GeneralRegion.FileTypeName.CrossSectionDefinition),
            };
            return categories;
        }

        private static IEnumerable<ICrossSection> GatherCrossSectionFromNetwork(IHydroNetwork network)
        {
            var crossSections = network.CrossSections.ToList();
            crossSections.AddRange(network.Culverts
                .Select(c => c.CrossSectionDefinition)
                .Select(crossSectionDefinition =>
                    new CrossSection(crossSectionDefinition) {Name = crossSectionDefinition.Name}));

            crossSections.AddRange(network.Bridges
                .Where(b => b.CrossSectionDefinition != null)
                .Select(b => b.CrossSectionDefinition)
                .Select(crossSectionDefinition =>
                    new CrossSection(crossSectionDefinition) {Name = crossSectionDefinition.Name}));
            return crossSections;
        }
    }
}