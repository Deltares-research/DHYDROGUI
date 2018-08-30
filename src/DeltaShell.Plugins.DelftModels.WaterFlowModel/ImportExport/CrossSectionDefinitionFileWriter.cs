using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class CrossSectionDefinitionFileWriter
    {
        public static void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
            if (File.Exists(targetFile)) File.Delete(targetFile);

            var categories = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion, 
                                      GeneralRegion.CrossSectionDefinitionsMinorVersion, 
                                      GeneralRegion.FileTypeName.CrossSectionDefinition),
            };

            var crossSections = waterFlowModel1D.Network.CrossSections.ToList();
            crossSections.AddRange(waterFlowModel1D.Network.Culverts.Select(c => c.CrossSectionDefinition).Select(crossSectionDefinition => new CrossSection(crossSectionDefinition) { Name = crossSectionDefinition.Name }));
            crossSections.AddRange(waterFlowModel1D.Network.Bridges.Where(b => b.CrossSectionDefinition != null).Select(b => b.CrossSectionDefinition).Select(crossSectionDefinition => new CrossSection(crossSectionDefinition) { Name = crossSectionDefinition.Name }));
            var sharedCrossSections = waterFlowModel1D.Network.SharedCrossSectionDefinitions;
            var processedCsDefinitions = new List<string>();
            foreach (var crossSection in crossSections)
            {
                var definition = crossSection.Definition.IsProxy
                    ? ((CrossSectionDefinitionProxy)crossSection.Definition).InnerDefinition
                    : crossSection.Definition;

                var definitionGeneratorCrossSectionDefinition = DefinitionGeneratorFactory.GetDefinitionGeneratorCrossSection(definition, crossSection.CrossSectionType);
                
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
                        definitionRegion = AddGroundLayer(definitionRegion, crossSection.Definition.Name, waterFlowModel1D.Network);
                        break;
                }
                if (sharedCrossSections.Contains(definition)) definitionRegion.AddProperty(DefinitionPropertySettings.IsShared.Key, 1, DefinitionPropertySettings.IsShared.Description);
                categories.Add(definitionRegion);
                processedCsDefinitions.Add(csDefinitionId);
            }
            
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        private static DelftIniCategory AddGroundLayer(DelftIniCategory iniCategory, string crossSectionDefinitionName, IHydroNetwork network)
        {
            int groundlayerUsed = 0; // default value
            double groundlayer = 0.0;  // default value
            var structure = network.Structures.FirstOrDefault(s =>  (s is ICulvert && ((ICulvert)s).CrossSectionDefinition.Name == crossSectionDefinitionName) ||
                                                                    (s is IBridge && ((IBridge)s).CrossSectionDefinition !=null && ((IBridge)s).CrossSectionDefinition.Name == crossSectionDefinitionName)) as IGroundLayer;
            if (structure != null)
            {
                groundlayerUsed = Convert.ToInt32(structure.GroundLayerEnabled);
                groundlayer = structure.GroundLayerThickness;
            }
            
            iniCategory.AddProperty(DefinitionPropertySettings.GroundlayerUsed.Key, groundlayerUsed, DefinitionPropertySettings.GroundlayerUsed.Description);
            iniCategory.AddProperty(DefinitionPropertySettings.Groundlayer.Key, groundlayer, DefinitionPropertySettings.Groundlayer.Description, DefinitionPropertySettings.Groundlayer.Format);
            return iniCategory;
        }

        private static DelftIniCategory AddRoughnessDataToFileContent(DelftIniCategory iniCategory, ICrossSection crossSection, IList<RoughnessSection> roughnessSections, bool useReverseRoughness)
        {
            var sectionSections = crossSection.Definition.Sections as IList<CrossSectionSection>;
            if (sectionSections.Count == 0)
            {
                IList<double> y = crossSection.Definition.Profile.Select(yz => yz.X).ToArray();
                
                IList<CrossSectionSection> crossSectionSections = new List<CrossSectionSection>
                                                                      {
                                                                          new CrossSectionSection
                                                                              {
                                                                                  MinY = y[0],
                                                                                  MaxY = y[y.Count - 1],
                                                                                  // always use "main"?; first is temporary fix
                                                                                  SectionType = roughnessSections[0].CrossSectionSectionType
                                                                              }
                                                                      };
                sectionSections = crossSectionSections;

            }
            var sectionCount = sectionSections.Count.ToString();
            
            var roughnessPositions = sectionSections.Select(s => s.MinY).Union(sectionSections.Select(s => s.MaxY));
            var frictionNames = new List<string>();

            foreach (
                var roughnessSection in
                    sectionSections.Select(
                        section => GetRoughnessSection(roughnessSections, section)))
            {
                frictionNames.Add(roughnessSection.Name);
            }

            iniCategory.AddProperty(DefinitionPropertySettings.SectionCount.Key, sectionCount, DefinitionPropertySettings.SectionCount.Description);
            iniCategory.AddProperty(DefinitionPropertySettings.RoughnessNames.Key, string.Join(";", frictionNames), DefinitionPropertySettings.RoughnessNames.Description);
            iniCategory.AddProperty(DefinitionPropertySettings.RoughnessPositions.Key, roughnessPositions, DefinitionPropertySettings.RoughnessPositions.Description, DefinitionPropertySettings.RoughnessPositions.Format);
            return iniCategory;
        }

        private static double GetNegativeFrictionValue(IList<RoughnessSection> roughnessSections, RoughnessSection roughnessSection, ICrossSection crossSection)
        {
            var reverseRoughnessSection = roughnessSections.GetApplicableReverseRoughnessSection(roughnessSection);
            return reverseRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(crossSection.ToNetworkLocation());
        }

        private static RoughnessSection GetRoughnessSection(IList<RoughnessSection> roughnessSections, CrossSectionSection crossSectionSection)
        {
            var roughnessSection = roughnessSections.FirstOrDefault(rs => rs.Name == crossSectionSection.SectionType.Name);
            if (roughnessSection == null)
            {
                throw new InvalidOperationException("No roughnessSection found with name " + crossSectionSection.SectionType.Name);
            }
            return roughnessSection;
        }

        
    }
}
