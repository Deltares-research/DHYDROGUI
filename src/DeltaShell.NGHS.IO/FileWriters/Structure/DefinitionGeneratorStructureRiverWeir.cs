using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureRiverWeir : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureRiverWeir(int compoundStructureId)
            : base(compoundStructureId)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.RiverWeir);

            var weir = structure as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as RiverWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);

            IniCategory.AddProperty(StructureRegion.PosCwCoef.Key, formula.CorrectionCoefficientPos, StructureRegion.PosCwCoef.Description, StructureRegion.PosCwCoef.Format);
            IniCategory.AddProperty(StructureRegion.PosSlimLimit.Key, formula.SubmergeLimitPos, StructureRegion.PosSlimLimit.Description, StructureRegion.PosSlimLimit.Format);

            IniCategory.AddProperty(StructureRegion.NegCwCoef.Key, formula.CorrectionCoefficientNeg, StructureRegion.NegCwCoef.Description, StructureRegion.NegCwCoef.Format);
            IniCategory.AddProperty(StructureRegion.NegSlimLimit.Key, formula.SubmergeLimitNeg, StructureRegion.NegSlimLimit.Description, StructureRegion.NegSlimLimit.Format);

            if (formula.SubmergeReductionPos != null)
            {
                var arguments = formula.SubmergeReductionPos.Arguments;
                var components = formula.SubmergeReductionPos.Components;

                if (arguments != null && components != null && arguments.Count > 0 && components.Count > 0)
                {
                    var posSf = arguments[0].Values.Cast<double>().ToList();
                    var posRed = components[0].Values.Cast<double>();
                    IniCategory.AddProperty(StructureRegion.PosSfCount.Key, posSf.Count, StructureRegion.PosSfCount.Description);
                    IniCategory.AddProperty(StructureRegion.PosSf.Key, posSf, StructureRegion.PosSf.Description, StructureRegion.PosSf.Format);
                    IniCategory.AddProperty(StructureRegion.PosRed.Key, posRed, StructureRegion.PosRed.Description, StructureRegion.PosRed.Format);
                }
                else
                {
                    IniCategory.AddProperty(StructureRegion.PosSfCount.Key, 0, StructureRegion.PosSfCount.Description);
                }
            }

            if (formula.SubmergeReductionNeg != null)
            {
                var arguments = formula.SubmergeReductionNeg.Arguments;
                var components = formula.SubmergeReductionNeg.Components;

                if (arguments != null && components != null && arguments.Count > 0 && components.Count > 0)
                {
                    var negSf = arguments[0].Values.Cast<double>().ToList();
                    var negRed = components[0].Values.Cast<double>();
                    
                    IniCategory.AddProperty(StructureRegion.NegSfCount.Key, negSf.Count, StructureRegion.NegSfCount.Description);
                    IniCategory.AddProperty(StructureRegion.NegSf.Key, negSf, StructureRegion.NegSf.Description, StructureRegion.NegSf.Format);
                    IniCategory.AddProperty(StructureRegion.NegRed.Key, negRed, StructureRegion.NegRed.Description, StructureRegion.NegRed.Format);
                }
                else
                {
                    IniCategory.AddProperty(StructureRegion.NegSfCount.Key, 0, StructureRegion.NegSfCount.Description);
                }
            }

            return IniCategory;
        }

    }
}