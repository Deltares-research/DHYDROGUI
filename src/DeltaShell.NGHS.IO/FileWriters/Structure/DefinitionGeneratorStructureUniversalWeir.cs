using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureUniversalWeir : DefinitionGeneratorStructure
    {
        
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.UniversalWeir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as FreeFormWeirFormula;
            if (formula == null) return IniCategory;
      
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, weir.FlowDirection.ToString().ToLower(), StructureRegion.AllowedFlowDir.Description);
            IniCategory.AddProperty(StructureRegion.LevelsCount.Key, formula.Y.ToList().Count, StructureRegion.LevelsCount.Description);
            IniCategory.AddProperty(StructureRegion.YValues.Key, formula.Y, StructureRegion.YValues.Description, StructureRegion.YValues.Format);
            IniCategory.AddProperty(StructureRegion.ZValues.Key, formula.Z, StructureRegion.ZValues.Description, StructureRegion.ZValues.Format);
            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.DischargeCoeff.Key, formula.DischargeCoefficient, StructureRegion.DischargeCoeff.Description, StructureRegion.DischargeCoeff.Format);
            
            return IniCategory;
        }
    }
}