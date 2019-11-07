using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDZWDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {
            var crossSectionDefinition = new CrossSectionDefinitionZW();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            var numLevels = category.ReadProperty<int>(DefinitionPropertySettings.NumLevels.Key);
            var levels = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.Levels.Key);
            var flowWidths = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.FlowWidths.Key);
            var totalWidths = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.TotalWidths.Key);

            if (numLevels == levels.Count
                && numLevels == flowWidths.Count
                && numLevels == totalWidths.Count)
            {
                var table = new FastZWDataTable();
                table.BeginLoadData();
                for (int i = 0; i < numLevels; i++)
                {
                    var storageWidth = totalWidths[i] - flowWidths[i];
                    table.AddCrossSectionZWRow(levels[i], totalWidths[i], storageWidth);
                }
                table.EndLoadData();
                crossSectionDefinition.ZWDataTable = table;   
            }
            else
            {
                var errorMessage = "num levels count property is not equal to number of level, flowWidths or totalWidths"; 
                throw new FileReadingException(errorMessage);
            }
            // summer dike
            var crestLevel = category.ReadProperty<double>(DefinitionPropertySettings.CrestLevee.Key);
            var flowArea = category.ReadProperty<double>(DefinitionPropertySettings.FlowAreaLevee.Key);
            var totalArea = category.ReadProperty<double>(DefinitionPropertySettings.TotalAreaLevee.Key);
            var baseLevel = category.ReadProperty<double>(DefinitionPropertySettings.BaseLevelLevee.Key);
            
            if (Math.Abs(flowArea) > double.Epsilon && Math.Abs(totalArea) > double.Epsilon)//(flowArea and totalArea are larger than 0, so you can do something with this
            {
                crossSectionDefinition.SummerDike = new SummerDike()
                {
                    Active = true,
                    CrestLevel = crestLevel,
                    FloodPlainLevel = baseLevel,
                    FloodSurface = flowArea,
                    TotalSurface = totalArea
                };
            }
            
            return crossSectionDefinition;
        }
    }
}