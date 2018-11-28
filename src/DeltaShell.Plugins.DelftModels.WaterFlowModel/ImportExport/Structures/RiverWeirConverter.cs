using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class RiverWeirConverter : IStructureConverter
    {
        public IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weirFormula = new RiverWeirFormula();
            var weir = new Weir
            {
                WeirFormula = weirFormula
            };

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);
            
            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            weirFormula.CorrectionCoefficientPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosCwCoef.Key);
            weirFormula.SubmergeLimitPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosSlimLimit.Key);

            weirFormula.CorrectionCoefficientNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegCwCoef.Key);
            weirFormula.SubmergeLimitNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegSlimLimit.Key);

            var posCount = structureBranchCategory.ReadProperty<int>(StructureRegion.PosSfCount.Key);
            var argumentsPos = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.PosSf.Key);
            var componentsPos = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.PosRed.Key);

            var check = new int[] { posCount, argumentsPos.Count, componentsPos.Count };

            if (posCount != argumentsPos.Count || posCount != componentsPos.Count)
            {
                throw new Exception(string.Format(
                    "For river weir {0} the reduction table for positive flow direction contains an error", weir.Name));
            }

            weirFormula.SubmergeReductionPos.Clear();

            for (int i = 0; i < posCount; i++)
            {
                weirFormula.SubmergeReductionPos[argumentsPos[i]] = componentsPos[i];
            }
            
            var negCount = structureBranchCategory.ReadProperty<int>(StructureRegion.NegSfCount.Key);
            var argumentsNeg = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.NegSf.Key);
            var componentsNeg = structureBranchCategory.ReadPropertiesToListOfType<double>(StructureRegion.NegRed.Key);

            check = new int[] { negCount, argumentsNeg.Count, componentsNeg.Count };

            if (negCount != argumentsNeg.Count || negCount != componentsNeg.Count)
            {
                throw new Exception(string.Format("For river weir {0} the reduction table for negative flow direction contains an error", weir.Name));
            }

            weirFormula.SubmergeReductionNeg.Clear();

            for (int i = 0; i < negCount; i++)
            {
                weirFormula.SubmergeReductionNeg[argumentsNeg[i]] = componentsNeg[i];
            }
            
            return weir;
        }
    }
}