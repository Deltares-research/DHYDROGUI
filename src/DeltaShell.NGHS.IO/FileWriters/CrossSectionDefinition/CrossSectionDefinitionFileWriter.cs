using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class CrossSectionDefinitionFileWriter
    {
        public static void WriteFile(string targetFile, IHydroNetwork network, IEnumerable<RoughnessSection> roughnessSections)
        {
            var crossSections = GetNetworkCrossSectionDefinitions(network);
            var sharedCrossSectionDefinitions = network.SharedCrossSectionDefinitions;

            WriteFile(targetFile, crossSections, sharedCrossSectionDefinitions, roughnessSections.ToList(), network.Structures);
        }

        protected static void WriteFile(string targetFile, IEnumerable<ICrossSectionDefinition> crossSectionDefinitions, IEnumerable<ICrossSectionDefinition> sharedCrossSectionDefinitions, IList<RoughnessSection> roughnessSections, IEnumerable<IStructure> structures)
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

                switch (crossSectionDefinition.CrossSectionType)
                {
                    case CrossSectionType.GeometryBased:
                    case CrossSectionType.YZ:
                        //add roughness
                        definitionRegion = AddRoughnessDataToFileContent(definitionRegion, crossSectionDefinition, roughnessSections);
                        break;
                    case CrossSectionType.ZW:
                    case CrossSectionType.Standard:
                        //add groundlevel
                        var groundLayer = structures.FirstOrDefault(s => IsCulvertWithRightName(s, crossSectionDefinition.Name) ||
                                                                         IsBridgeWithRightName(s, crossSectionDefinition.Name)) as IGroundLayer;
                        definitionRegion = AddGroundLayer(definitionRegion, groundLayer);
                        break;
                }
                if (sharedCrossSectionDefinitions.Contains(crossSectionDefinition)) definitionRegion.AddProperty(DefinitionPropertySettings.IsShared.Key, 1, DefinitionPropertySettings.IsShared.Description);
                categories.Add(definitionRegion);
                processedCsDefinitions.Add(csDefinitionId);
            }
            
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        private static DelftIniCategory AddRoughnessDataToFileContent(DelftIniCategory iniCategory, ICrossSectionDefinition crossSectionDefinition, IList<RoughnessSection> roughnessSections)
        {
            var sectionSections = crossSectionDefinition.Sections as IList<CrossSectionSection>;
            if (sectionSections.Count == 0)
            {
                IList<double> y = crossSectionDefinition.Profile.Select(yz => yz.X).ToArray();
                
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

            //iniCategory.AddProperty(DefinitionPropertySettings.SectionCount, sectionCount);
            //iniCategory.AddProperty(DefinitionPropertySettings.RoughnessNames, String.Join(";", frictionNames));
            //iniCategory.AddProperty(DefinitionPropertySettings.RoughnessPositions, roughnessPositions);
            return iniCategory;
        }

        private static bool IsBridgeWithRightName(IStructure structure, string crossSectionDefinitionName)
        {
            var bridge = structure as IBridge;
            return bridge?.CrossSectionDefinition != null 
                   && bridge.CrossSectionDefinition.Name == crossSectionDefinitionName;
        }

        private static bool IsCulvertWithRightName(IStructure structure, string crossSectionDefinitionName)
        {
            var culvert = structure as ICulvert;
            return culvert?.CrossSectionDefinition != null
                   && culvert.CrossSectionDefinition.Name == crossSectionDefinitionName;
        }

        private static DelftIniCategory AddGroundLayer(DelftIniCategory iniCategory, IGroundLayer groundLayer)
        {
            int groundlayerUsed = 0; // default value
            double groundlayer = 0.0;  // default value
            if (groundLayer != null)
            {
                groundlayerUsed = Convert.ToInt32(groundLayer.GroundLayerEnabled);
                groundlayer = groundLayer.GroundLayerThickness;
            }
            
            //iniCategory.AddProperty(DefinitionPropertySettings.GroundlayerUsed, groundlayerUsed);
            //iniCategory.AddProperty(DefinitionPropertySettings.Groundlayer, groundlayer);
            return iniCategory;
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

        private static IEnumerable<ICrossSectionDefinition> GetNetworkCrossSectionDefinitions(IHydroNetwork network)
        {
            var crossSections = GetNetworkCrossSections(network);
            var crossSectionDefinitions = crossSections.Select(GetCrossSectionDefinition).ToList();
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
                ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition
                : crossSectionDefinition;
            return definition;
        }

        private static void AddBridgeCrossSectionDefinitions(List<ICrossSection> crossSections, IHydroNetwork network)
        {
            var bridgeCrossSectionDefinitions = network.Bridges.Where(b => b.CrossSectionDefinition != null).Select(b => b.CrossSectionDefinition);
            crossSections.AddRange(bridgeCrossSectionDefinitions.Select(csd => new CrossSection(csd) { Name = csd.Name }));
        }

        private static void AddCulvertCrossSectionDefinitions(List<ICrossSection> crossSections, IHydroNetwork network)
        {
            var culvertCrossSectionDefinitions = network.Culverts.Select(c => c.CrossSectionDefinition);
            crossSections.AddRange(culvertCrossSectionDefinitions.Select(csd => new CrossSection(csd) { Name = csd.Name }));
        }
    }
}