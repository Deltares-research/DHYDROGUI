using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureOrifice : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureOrifice(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Orifice);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as GatedWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, (int)weir.FlowDirection, StructureRegion.AllowedFlowDir.Description);
            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            IniCategory.AddProperty(StructureRegion.OpenLevel.Key, (weir.CrestLevel + formula.GateOpening), StructureRegion.OpenLevel.Description, StructureRegion.OpenLevel.Format);
            IniCategory.AddProperty(StructureRegion.ContractionCoeff.Key, formula.ContractionCoefficient, StructureRegion.ContractionCoeff.Description, StructureRegion.ContractionCoeff.Format);
            IniCategory.AddProperty(StructureRegion.LatContrCoeff.Key, formula.LateralContraction, StructureRegion.LatContrCoeff.Description, StructureRegion.LatContrCoeff.Format);

            IniCategory.AddProperty(StructureRegion.UseLimitFlowPos.Key, Convert.ToInt32(formula.UseMaxFlowPos), StructureRegion.UseLimitFlowPos.Description);
            IniCategory.AddProperty(StructureRegion.LimitFlowPos.Key, formula.MaxFlowPos, StructureRegion.LimitFlowPos.Description, StructureRegion.LimitFlowPos.Format);
            IniCategory.AddProperty(StructureRegion.UseLimitFlowNeg.Key, Convert.ToInt32(formula.UseMaxFlowNeg), StructureRegion.UseLimitFlowNeg.Description);
            IniCategory.AddProperty(StructureRegion.LimitFlowNeg.Key, formula.MaxFlowNeg, StructureRegion.LimitFlowNeg.Description, StructureRegion.LimitFlowNeg.Format);

            return IniCategory;
        }
    }
}