using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureUniversalWeir : DefinitionGeneratorStructure
    {
        private const double DEFAULT_FREE_SUBMERGED_FACTOR = 0.667F;

        public DefinitionGeneratorStructureUniversalWeir(int compoundStructureId)
            : base(compoundStructureId)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.UniversalWeir);

            var weir = structure as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as FreeFormWeirFormula;
            if (formula == null) return IniCategory;
      
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, (int) weir.FlowDirection, StructureRegion.AllowedFlowDir.Description);
            IniCategory.AddProperty(StructureRegion.LevelsCount.Key, formula.Y.ToList().Count, StructureRegion.LevelsCount.Description);
            IniCategory.AddProperty(StructureRegion.YValues.Key, formula.Y, StructureRegion.YValues.Description, StructureRegion.YValues.Format);
            IniCategory.AddProperty(StructureRegion.ZValues.Key, formula.Z, StructureRegion.ZValues.Description, StructureRegion.ZValues.Format);
            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.DischargeCoeff.Key, formula.DischargeCoefficient, StructureRegion.DischargeCoeff.Description, StructureRegion.DischargeCoeff.Format);
            IniCategory.AddProperty(StructureRegion.FreeSubmergedFactor.Key, DEFAULT_FREE_SUBMERGED_FACTOR, StructureRegion.FreeSubmergedFactor.Description, StructureRegion.FreeSubmergedFactor.Format);

            return IniCategory;
        }
    }
}