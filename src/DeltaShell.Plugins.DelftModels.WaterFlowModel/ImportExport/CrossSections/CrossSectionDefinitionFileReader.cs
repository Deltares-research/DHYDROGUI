using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public class CrossSectionDefinitionFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public CrossSectionDefinitionFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public IList<ICrossSectionDefinition> Read(string filePath, IHydroNetwork network)
        {
            var errorMessages = new List<string>();

            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories, errorMessages).ToList();

            try
            {
                SetRoughnessOnCrossSectionDefinitions(categories, crossSectionDefinitions, network);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            CreateErrorReport("cross section definitions", filePath, errorMessages);

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

            var sectionTypeName = roughnessNames.FirstOrDefault();

            if ((roughnessNames.Count == 1 && roughnessNames.First() == String.Empty) || sectionTypeName == null)
            {
                throw new FileReadingException(
                    "There was no roughness defined in the cross section definition file.");
            }

            else if (roughnessNames.Count != 1)
            {
                throw new FileReadingException(
                    "There can only be one roughness defined on a standard cross section definition.");
            }

            var crossSectionSectionType = GetCrossSectionSectionType(sectionTypeName, network);

            var sectionWidth = GetSectionWidthForStandardShape(category);

            crossSectionDefinition.AddSection(crossSectionSectionType, sectionWidth);

            AddCrossSectionSectionType(crossSectionSectionType, network);
        }

        private static double GetSectionWidthForStandardShape(IDelftIniCategory category)
        {
            var highestFlowWidth = category.ReadPropertiesToListOfType<double>(DefinitionRegion.FlowWidths.Key, true)?.Max() ?? 0.0d;
            var rectangleWidth = category.ReadProperty<double>(DefinitionRegion.RectangleWidth.Key, true);
            var maxflowWidth = category.ReadProperty<double>(DefinitionRegion.MaximumFlowWidth.Key, true);

            var widths = new List<double>() { highestFlowWidth, rectangleWidth, maxflowWidth };
            return widths.Max();
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

            if (roughnessNames.Count <= 0 || (roughnessNames.Count == 1 && roughnessNames.First() == String.Empty))
                throw new FileReadingException("There were no roughness names defined in the cross section definition file.");

                var roughnessPositions =
                    category.ReadPropertiesToListOfType<double>(DefinitionRegion.RoughnessPositions.Key);

            if (roughnessPositions.Count != roughnessNames.Count + 1)
                throw new FileReadingException("Incorrect number of roughness positions in cross section definition file: should be one more than the number of roughness sections.");

            crossSectionDefinition.Sections.Clear();

            for (var i = 0; i < roughnessNames.Count; i++)
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

        private static IList<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, List<string> errorMessages)
        {
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            return categories;
        }

        private void CreateErrorReport(string objectName, string filePath, List<string> errorMessages)
        {
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the {objectName} from file '{filePath}', the following errors occured", errorMessages);
        }
    }
}
