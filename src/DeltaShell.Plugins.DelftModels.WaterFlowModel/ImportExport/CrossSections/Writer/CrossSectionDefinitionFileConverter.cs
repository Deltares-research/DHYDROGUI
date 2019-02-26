using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer
{
    public class CrossSectionDefinitionFileConverter : ICrossSectionDefinitionFileConverter
    {
        /// <summary>
        /// Converts a <see cref="WaterFlowModel1D"/> to a <see cref="DelftIniCategory"/>
        /// </summary>
        /// <param name="waterFlowModel1D"></param>
        /// <returns>a <see cref="DelftIniCategory"/>></returns>
        public IEnumerable<DelftIniCategory> Convert(WaterFlowModel1D waterFlowModel1D)
        {
            var categories = CreateCrossSectionDefinitionCategory();
            var crossSections = AddCrossSections(waterFlowModel1D);
            var sharedCrossSections = waterFlowModel1D.Network.SharedCrossSectionDefinitions;

            var processedCsDefinitions = new List<string>();
            foreach (var crossSection in crossSections)
            {
                var definition = GetDefinition(crossSection);
                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory.GetDefinitionGeneratorCrossSection(definition, crossSection.CrossSectionType);

                if (definitionGeneratorCrossSectionDefinition == null) continue;

                string csDefinitionId = definition.Name;
                if (processedCsDefinitions.Contains(csDefinitionId)) continue;

                var definitionRegion = definitionGeneratorCrossSectionDefinition.CreateDefinitionRegion(definition);
                definitionRegion = DetermineCrossSectionType(waterFlowModel1D, crossSection, definitionRegion);

                CheckIfSharedCrossSectionsContainsDefinition(sharedCrossSections, definition, definitionRegion);

                categories.Add(definitionRegion);
                processedCsDefinitions.Add(csDefinitionId);
            }

            return categories;
        }

        private static void CheckIfSharedCrossSectionsContainsDefinition(IEventedList<ICrossSectionDefinition> sharedCrossSections, ICrossSectionDefinition definition,
            DelftIniCategory definitionRegion)
        {
            if (sharedCrossSections.Count > 0)
            {
                definitionRegion.AddProperty(DefinitionRegion.IsShared.Key,
                    1, DefinitionRegion.IsShared.Description);
            }
        }

        private static DelftIniCategory DetermineCrossSectionType(WaterFlowModel1D waterFlowModel1D, ICrossSection crossSection,
            DelftIniCategory definitionRegion)
        {
            switch (crossSection.CrossSectionType)
            {
                case CrossSectionType.GeometryBased:
                case CrossSectionType.YZ:
                    definitionRegion = RoughnessDataProcessor.AddRoughnessDataToFileContent(definitionRegion, crossSection,
                        waterFlowModel1D.RoughnessSections, waterFlowModel1D.UseReverseRoughness);
                    break;
                case CrossSectionType.ZW:
                case CrossSectionType.Standard:
                    definitionRegion = AddGroundLayer(definitionRegion, crossSection.Definition.Name, waterFlowModel1D.Network);
                    break;
            }

            return definitionRegion;
        }

        private static DelftIniCategory AddGroundLayer(DelftIniCategory iniCategory, string crossSectionDefinitionName, IHydroNetwork network)
        {
            var groundLayerUsed = 0; // default value
            var groundLayer = 0.0;  // default value
            var structure = network.Structures.FirstOrDefault(s => (s is ICulvert && ((ICulvert)s).CrossSectionDefinition.Name == crossSectionDefinitionName) ||
                                                                   (s is IBridge && ((IBridge)s).CrossSectionDefinition != null && ((IBridge)s).CrossSectionDefinition.Name == crossSectionDefinitionName)) as IGroundLayer;
            if (structure != null)
            {
                groundLayerUsed = System.Convert.ToInt32(structure.GroundLayerEnabled);
                groundLayer = structure.GroundLayerThickness;
            }

            iniCategory.AddProperty(DefinitionRegion.GroundlayerUsed.Key, groundLayerUsed, DefinitionRegion.GroundlayerUsed.Description);
            iniCategory.AddProperty(DefinitionRegion.Groundlayer.Key, groundLayer, DefinitionRegion.Groundlayer.Description, DefinitionRegion.Groundlayer.Format);
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

        private static List<ICrossSection> AddCrossSections(WaterFlowModel1D waterFlowModel1D)
        {
            var crossSections = waterFlowModel1D.Network.CrossSections.ToList();
            crossSections.AddRange(waterFlowModel1D.Network.Culverts
                .Select(c => c.CrossSectionDefinition)
                .Select(crossSectionDefinition =>
                    new CrossSection(crossSectionDefinition) {Name = crossSectionDefinition.Name}));

            crossSections.AddRange(waterFlowModel1D.Network.Bridges
                .Where(b => b.CrossSectionDefinition != null)
                .Select(b => b.CrossSectionDefinition)
                .Select(crossSectionDefinition =>
                    new CrossSection(crossSectionDefinition) {Name = crossSectionDefinition.Name}));
            return crossSections;
        }
    }

    public interface ICrossSectionDefinitionFileConverter
    {
        IEnumerable<DelftIniCategory> Convert(WaterFlowModel1D model);
    }
}