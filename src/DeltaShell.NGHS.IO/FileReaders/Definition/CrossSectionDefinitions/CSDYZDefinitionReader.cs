using System.Data;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDYZDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {
            var crossSectionDefinition = new CrossSectionDefinitionYZ();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);

            var yList = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.YCoors.Key);
            var zList = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.ZCoors.Key);

            var yzCount = iniSection.ReadProperty<int>(DefinitionPropertySettings.YZCount.Key);

            if (yzCount == yList.Count && yList.Count != zList.Count)
            {
                var errorMessage = "yz count property is not equal to number of yvalues or zvalues or delta z storage";
                throw new FileReadingException(errorMessage);
            }

            var table = new FastYZDataTable();
            table.BeginLoadData();
            
            for (int i = 0; i < yList.Count; i++)
            {
                if (i > 0 && yList[i] < yList[i - 1])
                {
                    var errorMessage = "y-values are decreasing for " + crossSectionDefinition.Name;
                    throw new FileReadingException(errorMessage);
                }
                try
                {
                    table.AddCrossSectionYZRow(yList[i], zList[i]);
                }
                catch (ConstraintException e)
                {
                    throw new FileReadingException($"Can not set YZ-table of cross section definition {crossSectionDefinition.Name}", e);
                }
               
            }
            table.EndLoadData();
            crossSectionDefinition.YZDataTable = table;
            return crossSectionDefinition;
        }
    }
}