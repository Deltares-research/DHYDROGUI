using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureRiverWeir : DefinitionGeneratorStructure
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.RiverWeir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniSection;

            var formula = weir.WeirFormula as RiverWeirFormula;
            if (formula == null) return IniSection;

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosCwCoef.Key, formula.CorrectionCoefficientPos, StructureRegion.PosCwCoef.Description, StructureRegion.PosCwCoef.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosSlimLimit.Key, formula.SubmergeLimitPos, StructureRegion.PosSlimLimit.Description, StructureRegion.PosSlimLimit.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegCwCoef.Key, formula.CorrectionCoefficientNeg, StructureRegion.NegCwCoef.Description, StructureRegion.NegCwCoef.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegSlimLimit.Key, formula.SubmergeLimitNeg, StructureRegion.NegSlimLimit.Description, StructureRegion.NegSlimLimit.Format);

            if (formula.SubmergeReductionPos != null)
            {
                var arguments = formula.SubmergeReductionPos.Arguments;
                var components = formula.SubmergeReductionPos.Components;

                if (arguments != null && components != null && arguments.Count > 0 && components.Count > 0)
                {
                    var posSf = arguments[0].Values.Cast<double>().ToList();
                    var posRed = components[0].Values.Cast<double>();
                    IniSection.AddProperty(StructureRegion.PosSfCount.Key, posSf.Count, StructureRegion.PosSfCount.Description);
                    IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.PosSf.Key, posSf, StructureRegion.PosSf.Description, StructureRegion.PosSf.Format);
                    IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.PosRed.Key, posRed, StructureRegion.PosRed.Description, StructureRegion.PosRed.Format);
                }
                else
                {
                    IniSection.AddProperty(StructureRegion.PosSfCount.Key, 0, StructureRegion.PosSfCount.Description);
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
                    
                    IniSection.AddProperty(StructureRegion.NegSfCount.Key, negSf.Count, StructureRegion.NegSfCount.Description);
                    IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.NegSf.Key, negSf, StructureRegion.NegSf.Description, StructureRegion.NegSf.Format);
                    IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.NegRed.Key, negRed, StructureRegion.NegRed.Description, StructureRegion.NegRed.Format);
                }
                else
                {
                    IniSection.AddProperty(StructureRegion.NegSfCount.Key, 0, StructureRegion.NegSfCount.Description);
                }
            }

            return IniSection;
        }

    }
}