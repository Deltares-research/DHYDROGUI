using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureOrifice : DefinitionGeneratorStructure
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Orifice);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as GatedWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);

            IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, (weir.CrestLevel + formula.GateOpening), StructureRegion.GateLowerEdgeLevel.Description, StructureRegion.GateLowerEdgeLevel.Format);
            
            IniCategory.AddProperty(StructureRegion.CorrectionCoeff.Key, formula.ContractionCoefficient*formula.LateralContraction, StructureRegion.CorrectionCoeff.Description, StructureRegion.CorrectionCoeff.Format);
            IniCategory.AddProperty(StructureRegion.UseVelocityHeight.Key, weir.UseVelocityHeight.ToString().ToLower());


            return IniCategory;
        }
    }
}