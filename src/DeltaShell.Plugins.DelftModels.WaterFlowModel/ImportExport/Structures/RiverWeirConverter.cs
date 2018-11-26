using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    internal class RiverWeirConverter
    {
        public static IWeir ConvertToRiverWeir(DelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weir = new Weir();
            weir.WeirFormula = new RiverWeirFormula();

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);

            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            ((RiverWeirFormula)(weir.WeirFormula)).CorrectionCoefficientPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosCwCoef.Key);
            ((RiverWeirFormula)(weir.WeirFormula)).SubmergeLimitPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosSlimLimit.Key);

            ((RiverWeirFormula)(weir.WeirFormula)).CorrectionCoefficientNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegCwCoef.Key);
            ((RiverWeirFormula)(weir.WeirFormula)).SubmergeLimitNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegSlimLimit.Key);

            var posCount = structureBranchCategory.ReadProperty<int>(StructureRegion.PosSfCount.Key);
            var argumentsPos = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.PosSf.Key);
            var componentsPos = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.PosRed.Key);

            var check = new int[] { posCount, argumentsPos.Count, componentsPos.Count };

            for (int i = 0; i < 2; i++)
            {
                if (check[i] != check[i + 1]) throw new Exception(string.Format("For river weir {0} the reduction table for positive flow direction contains an error", weir.Name));
            }
            
            for (int i = 0; i < posCount; i++)
            {
                ((RiverWeirFormula)(weir.WeirFormula)).SubmergeReductionPos[argumentsPos[i]] = componentsPos[i];
            }
            
            var negCount = structureBranchCategory.ReadProperty<int>(StructureRegion.NegSfCount.Key);
            var argumentsNeg = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.NegSf.Key);
            var componentsNeg = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.NegRed.Key);

            check = new int[] { negCount, argumentsNeg.Count, componentsNeg.Count };

            for (int i = 0; i < 2; i++)
            {
                if (check[i] != check[i + 1]) throw new Exception(string.Format("For river weir {0} the reduction table for negative flow direction contains an error", weir.Name));
            }
            
            for (int i = 0; i < posCount; i++)
            {
                ((RiverWeirFormula)(weir.WeirFormula)).SubmergeReductionNeg[argumentsNeg[i]] = componentsNeg[i];
            }
            
            return weir;
        }
    }
}