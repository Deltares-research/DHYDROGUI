using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using System;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class RiverWeirConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new RiverWeirFormula()
            };
        }

        protected override void SetStructureProperties()
        {
            var weir = Structure as Weir;
            var weirFormula = weir.WeirFormula as RiverWeirFormula;

            weir.CrestLevel = Category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = Category.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            weirFormula.CorrectionCoefficientPos = Category.ReadProperty<double>(StructureRegion.PosCwCoef.Key);
            weirFormula.SubmergeLimitPos = Category.ReadProperty<double>(StructureRegion.PosSlimLimit.Key);

            weirFormula.CorrectionCoefficientNeg = Category.ReadProperty<double>(StructureRegion.NegCwCoef.Key);
            weirFormula.SubmergeLimitNeg = Category.ReadProperty<double>(StructureRegion.NegSlimLimit.Key);

            var posCount = Category.ReadProperty<int>(StructureRegion.PosSfCount.Key);
            var argumentsPos = Category.ReadPropertiesToListOfType<double>(StructureRegion.PosSf.Key);
            var componentsPos = Category.ReadPropertiesToListOfType<double>(StructureRegion.PosRed.Key);

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

            var negCount = Category.ReadProperty<int>(StructureRegion.NegSfCount.Key);
            var argumentsNeg = Category.ReadPropertiesToListOfType<double>(StructureRegion.NegSf.Key);
            var componentsNeg = Category.ReadPropertiesToListOfType<double>(StructureRegion.NegRed.Key);

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