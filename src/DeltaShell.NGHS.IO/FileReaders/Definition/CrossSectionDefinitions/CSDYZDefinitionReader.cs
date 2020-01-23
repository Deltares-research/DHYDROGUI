using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDYZDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {
            var crossSectionDefinition = new CrossSectionDefinitionYZ();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            var yList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.YCoors.Key);
            var zList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.ZCoors.Key);

            var yzCount = category.ReadProperty<int>(DefinitionPropertySettings.YZCount.Key);

            if (yzCount == yList.Count && yList.Count != zList.Count)
            {
                var errorMessage = "yz count property is not equal to number of yvalues or zvalues or delta z storage";
                throw new FileReadingException(errorMessage);
            }

            var table = new FastYZDataTable();
            table.BeginLoadData();
            for (int i = 0; i < yList.Count; i++)
            {
                table.AddCrossSectionYZRow(yList[i], zList[i]);
            }
            table.EndLoadData();
            crossSectionDefinition.YZDataTable = table;
            return crossSectionDefinition;
        }
    }
}