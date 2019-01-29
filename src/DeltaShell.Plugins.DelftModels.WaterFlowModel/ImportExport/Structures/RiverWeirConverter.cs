using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class RiverWeirConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new RiverWeirFormula()
            };
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var weir = structure as Weir;
            var weirFormula = weir.WeirFormula as RiverWeirFormula;

            weir.CrestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = category.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            weirFormula.CorrectionCoefficientPos = category.ReadProperty<double>(StructureRegion.PosCwCoef.Key);
            weirFormula.SubmergeLimitPos = category.ReadProperty<double>(StructureRegion.PosSlimLimit.Key);

            weirFormula.CorrectionCoefficientNeg = category.ReadProperty<double>(StructureRegion.NegCwCoef.Key);
            weirFormula.SubmergeLimitNeg = category.ReadProperty<double>(StructureRegion.NegSlimLimit.Key);

            var posCount = category.ReadProperty<int>(StructureRegion.PosSfCount.Key);
            var argumentsPos = category.ReadPropertiesToListOfType<double>(StructureRegion.PosSf.Key);
            var componentsPos = category.ReadPropertiesToListOfType<double>(StructureRegion.PosRed.Key);

            if (posCount != argumentsPos.Count || posCount != componentsPos.Count)
            {
                throw new Exception(string.Format(
                    "For river weir {0} the reduction table for positive flow direction contains an error", weir.Name));
            }

            weirFormula.SubmergeReductionPos.Clear();

            for (var i = 0; i < posCount; i++)
            {
                weirFormula.SubmergeReductionPos[argumentsPos[i]] = componentsPos[i];
            }

            var negCount = category.ReadProperty<int>(StructureRegion.NegSfCount.Key);
            var argumentsNeg = category.ReadPropertiesToListOfType<double>(StructureRegion.NegSf.Key);
            var componentsNeg = category.ReadPropertiesToListOfType<double>(StructureRegion.NegRed.Key);

            if (negCount != argumentsNeg.Count || negCount != componentsNeg.Count)
            {
                throw new Exception(string.Format("For river weir {0} the reduction table for negative flow direction contains an error", weir.Name));
            }

            weirFormula.SubmergeReductionNeg.Clear();

            for (var i = 0; i < negCount; i++)
            {
                weirFormula.SubmergeReductionNeg[argumentsNeg[i]] = componentsNeg[i];
            }
        }
    }
}