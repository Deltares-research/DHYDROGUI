using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.Delftnetworks.WaterFlownetwork.ImportExport.CrossSections
{
    public static class CrossSectionDefinitionFileReader
    {
        public static IList<ICrossSectionDefinition> Read(string path, IHydroNetwork network)
        {
            if (!File.Exists(path))
                throw new FileReadingException($"Could not read file {path} properly, it doesn't exist.");

            var categories = DelftIniFileParser.ReadFile(path);

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories).ToList();

            if (crossSectionDefinitions == null || crossSectionDefinitions.Count == 0)
                throw new FileReadingException("Could not read cross section locations.");

            if (!crossSectionDefinitions.Select(csd => csd.Name).HasUniqueValues())
                throw new FileReadingException("There are duplicate cross section IDs in the definition file, must be unique!");

            SetRoughnessOnCrossSectionDefinitions(categories, crossSectionDefinitions, network);

            return crossSectionDefinitions;
        }

        private static void SetRoughnessOnCrossSectionDefinitions(IList<DelftIniCategory> categories, List<ICrossSectionDefinition> definitions, IHydroNetwork network)
        {
            var selectedCategories = categories.Where(category => category.Name == DefinitionRegion.Header);

            definitions.ForEach(d =>
            {
                var category = selectedCategories.FirstOrDefault(c => d.Name == c.ReadProperty<string>(DefinitionRegion.Id.Key));
                SetRoughnessOnCrossSectionDefinition(d, category, network);
            });
        }

        private static void SetRoughnessOnCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, IDelftIniCategory category, IHydroNetwork network)
        {
            if (category == null || crossSectionDefinition == null) return;

            switch (crossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.YZ:
                case CrossSectionType.GeometryBased:
                    SetFrictionOnYZOrGeometryBasedCrossSectionDefinition(category, crossSectionDefinition, network);
                    break;
                case CrossSectionType.ZW:
                    SetFrictionOnZWCrossSectionDefinition(category, crossSectionDefinition, network);
                    break;
                case CrossSectionType.Standard:
                    SetFrictionOnStandardCrossSectionDefinition(category, crossSectionDefinition, network);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void SetFrictionOnStandardCrossSectionDefinition(IDelftIniCategory category,
            ICrossSectionDefinition crossSectionDefinition, IHydroNetwork network)
        {
            if (category == null || crossSectionDefinition == null || network == null) return;

            var roughnessNames = category.ReadPropertiesToListOfType<string>(DefinitionRegion.RoughnessNames.Key, true);
            if (roughnessNames == null) return;

            if (roughnessNames.Count != 1)
                throw new FileReadingException("There can only be one roughness defined on a cross section definition.");

            var sectionTypeName = roughnessNames.FirstOrDefault();
            if (sectionTypeName == null)
                throw new FileReadingException("There was no roughness defined on the cross section definition");

            var crossSectionSectionType = GetCrossSectionSectionType(sectionTypeName, network);

            var maxflowWidth = category.ReadPropertiesToListOfType<double>(DefinitionRegion.FlowWidths.Key).Max();

            crossSectionDefinition.AddSection(crossSectionSectionType, maxflowWidth);

            AddCrossSectionSectionType(crossSectionSectionType, network);
        }

        private static void SetFrictionOnZWCrossSectionDefinition(IDelftIniCategory category, ICrossSectionDefinition crossSectionDefinition,
            IHydroNetwork network)
        {
            if (category == null || crossSectionDefinition == null || network == null) return;

            var mainCrossSectionSectionType =
                GetCrossSectionSectionType(CrossSectionDefinition.MainSectionName, network);
            var floodPlain1CrossSectionSectionType =
                GetCrossSectionSectionType(CrossSectionDefinitionZW.Floodplain1SectionTypeName, network);
            var floodPlain2CrossSectionSectionType =
                GetCrossSectionSectionType(CrossSectionDefinitionZW.Floodplain2SectionTypeName, network);

            var mainSectionWidth = category.ReadProperty<double>(DefinitionRegion.Main.Key);
            var floodPlain1Width = category.ReadProperty<double>(DefinitionRegion.FloodPlain1.Key, true);
            var flowWidths = category.ReadPropertiesToListOfType<double>(DefinitionRegion.FlowWidths.Key);

            var floodPlain2Width = flowWidths.Max() - mainSectionWidth - floodPlain1Width;

            crossSectionDefinition.Sections.Clear();

            crossSectionDefinition.AddSection(mainCrossSectionSectionType, mainSectionWidth);
            crossSectionDefinition.AddSection(floodPlain1CrossSectionSectionType, floodPlain1Width);
            crossSectionDefinition.AddSection(floodPlain2CrossSectionSectionType, floodPlain2Width);

            AddCrossSectionSectionType(mainCrossSectionSectionType, network);
            AddCrossSectionSectionType(floodPlain1CrossSectionSectionType, network);
            AddCrossSectionSectionType(floodPlain2CrossSectionSectionType, network);
        }

        private static void SetFrictionOnYZOrGeometryBasedCrossSectionDefinition(IDelftIniCategory category,
            ICrossSectionDefinition crossSectionDefinition, IHydroNetwork network)
        {
            if (category == null || crossSectionDefinition == null || network == null) return;

            var roughnessNames =
                category.ReadPropertiesToListOfType<string>(DefinitionRegion.RoughnessNames.Key, separator: ';');
            if (roughnessNames.Count < 0)
                throw new FileReadingException("reading error");

            var roughnessPositions =
                category.ReadPropertiesToListOfType<double>(DefinitionRegion.RoughnessPositions.Key);
            if (roughnessPositions.Count < 0)
                throw new FileReadingException("reading error");

            if (roughnessPositions.Count != roughnessNames.Count + 1)
                throw new FileReadingException("reading error");

            crossSectionDefinition.Sections.Clear();

            for (int i = 0; i < roughnessNames.Count; i++)
            {
                var networkSectionType = GetCrossSectionSectionType(roughnessNames[i], network);

                crossSectionDefinition.Sections.Add(new CrossSectionSection
                {
                    SectionType = networkSectionType,
                    MinY = roughnessPositions[i],
                    MaxY = roughnessPositions[i + 1]
                });

                AddCrossSectionSectionType(networkSectionType, network);
            }
        }

        private static void AddCrossSectionSectionType(CrossSectionSectionType crossSectionSectionType, IHydroNetwork network)
        {
            if (!network.CrossSectionSectionTypes.Contains(crossSectionSectionType))
            {
                network.CrossSectionSectionTypes.Add(crossSectionSectionType);
            }
        }

        private static CrossSectionSectionType GetCrossSectionSectionType(string sectionTypeName, IHydroNetwork network)
        {
            var crossSectionSectionType = network.CrossSectionSectionTypes.FirstOrDefault(cst => cst.Name == sectionTypeName);

            if (crossSectionSectionType == null)
                crossSectionSectionType = new CrossSectionSectionType { Name = sectionTypeName };

            return crossSectionSectionType;
        }
    }
}
