using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.Helpers;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDZWDefinitionReader : CrossSectionDefinitionReaderBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CSDZWDefinitionReader));

        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {
            var crossSectionDefinition = new CrossSectionDefinitionZW();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);

            var numLevels = iniSection.ReadProperty<int>(DefinitionPropertySettings.NumLevels.Key);
            var levels = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.Levels.Key);
            var flowWidths = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.FlowWidths.Key);
            var totalWidths = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.TotalWidths.Key);

            if (numLevels == levels.Count
                && numLevels == flowWidths.Count
                && numLevels == totalWidths.Count)
            {
                var table = new FastZWDataTable();
                table.BeginLoadData();
                for (int i = 0; i < numLevels; i++)
                {
                    var storageWidth = totalWidths[i] - flowWidths[i];
                    if (storageWidth < 0.0)
                    {
                        log.WarnFormat("FlowWidth exceeds TotalWidth for cross section definition {0}. The FlowWidth has been set to the TotalWidth.", crossSectionDefinition.Name);
                        storageWidth = 0.0;
                    }
                    table.AddCrossSectionZWRow(levels[i], totalWidths[i], storageWidth);
                }
                table.EndLoadData();
                crossSectionDefinition.ZWDataTable = table;
                CrossSectionHelper.SetDefaultThalweg(crossSectionDefinition);
                crossSectionDefinition.Thalweg = iniSection.ReadProperty<double>(DefinitionPropertySettings.Thalweg.Key);
            }
            else
            {
                var errorMessage = "num levels count property is not equal to number of level, flowWidths or totalWidths"; 
                throw new FileReadingException(errorMessage);
            }
            // summer dike
            var crestLevel = iniSection.ReadProperty<double>(DefinitionPropertySettings.CrestLevee.Key,true);
            var flowArea = iniSection.ReadProperty<double>(DefinitionPropertySettings.FlowAreaLevee.Key, true);
            var totalArea = iniSection.ReadProperty<double>(DefinitionPropertySettings.TotalAreaLevee.Key, true);
            var baseLevel = iniSection.ReadProperty<double>(DefinitionPropertySettings.BaseLevelLevee.Key, true);
            
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