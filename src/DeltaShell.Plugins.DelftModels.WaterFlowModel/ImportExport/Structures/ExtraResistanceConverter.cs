using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public static class ExtraResistanceConverter
    {
        public static IExtraResistance ConvertToExtraResistance(DelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var extraResistance = new ExtraResistance();

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, extraResistance);

            var numValues = structureBranchCategory.ReadProperty<int>(StructureRegion.NumValues.Key);
            var argumentsLevels = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.Levels.Key);
            var componentsKsi = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.Ksi.Key);

            var check = new int[] { numValues, argumentsLevels.Count, componentsKsi.Count };

            for (int i = 0; i < 2; i++)
            {
                if (check[i] != check[i + 1]) throw new Exception(string.Format("For extra resistance {0} the friction table contains an error", extraResistance.Name));
            }

            for (int i = 0; i < numValues; i++)
            {
                extraResistance.FrictionTable[argumentsLevels[i]] = componentsKsi[i];
            }
            
            return extraResistance;
        }

    }
}
