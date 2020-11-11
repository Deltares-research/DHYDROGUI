using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureWeir : DefinitionGeneratorStructure
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Weir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as SimpleWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, weir.FlowDirection.ToString().ToLower(), StructureRegion.AllowedFlowDir.Description);

            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            if (weir.CrestWidth > 0)
            {
                IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth,StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            }

            IniCategory.AddProperty(StructureRegion.CorrectionCoeff.Key, formula.CorrectionCoefficient, StructureRegion.CorrectionCoeff.Description, StructureRegion.CorrectionCoeff.Format);
            IniCategory.AddProperty(StructureRegion.UseVelocityHeight.Key, weir.UseVelocityHeight.ToString().ToLower());

            return IniCategory;
        }
    }
}