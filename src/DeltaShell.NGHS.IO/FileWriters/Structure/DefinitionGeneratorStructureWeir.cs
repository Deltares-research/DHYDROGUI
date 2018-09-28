using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureWeir : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureWeir(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Weir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as SimpleWeirFormula;
            if (formula == null) return IniCategory;
            
            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            IniCategory.AddProperty(StructureRegion.DischargeCoeff.Key, formula.DischargeCoefficient, StructureRegion.DischargeCoeff.Description, StructureRegion.DischargeCoeff.Format);
            IniCategory.AddProperty(StructureRegion.LatDisCoeff.Key, formula.LateralContraction, StructureRegion.LatDisCoeff.Description, StructureRegion.LatDisCoeff.Format);
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, (int)weir.FlowDirection, StructureRegion.AllowedFlowDir.Description);

            return IniCategory;
        }
    }
}