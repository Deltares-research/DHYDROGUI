using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class ExtraResistanceConverter : IStructureConverter
    {
        public IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory, IBranch branch)
        {
            var extraResistance = new ExtraResistance();

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, branch, extraResistance);

            var numValues = structureBranchCategory.ReadProperty<int>(StructureRegion.NumValues.Key);
            var argumentsLevels = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.Levels.Key);
            var componentsKsi = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.Ksi.Key);

            if (numValues != argumentsLevels.Count || numValues != componentsKsi.Count)
            {
                throw new Exception(string.Format("For extra resistance {0} the friction table contains an error", extraResistance.Name));
            }

           extraResistance.FrictionTable.Clear();

            for (var i = 0; i < numValues; i++)
            {
                extraResistance.FrictionTable[argumentsLevels[i]] = componentsKsi[i];
            }
            
            return extraResistance;
        }

    }
}
