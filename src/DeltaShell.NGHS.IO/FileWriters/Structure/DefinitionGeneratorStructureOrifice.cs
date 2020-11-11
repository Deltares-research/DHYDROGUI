using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
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

            var orifice = hydroObject as IOrifice;
            if (orifice == null) return IniCategory;

            var formula = orifice.WeirFormula as GatedWeirFormula;
            if (formula == null) return IniCategory;
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, orifice.FlowDirection.ToString().ToLower(), StructureRegion.AllowedFlowDir.Description);

            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, orifice.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, orifice.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);

            IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, (orifice.CrestLevel + formula.GateOpening), StructureRegion.GateLowerEdgeLevel.Description, StructureRegion.GateLowerEdgeLevel.Format);
            
            IniCategory.AddProperty(StructureRegion.CorrectionCoeff.Key, formula.ContractionCoefficient*formula.LateralContraction, StructureRegion.CorrectionCoeff.Description, StructureRegion.CorrectionCoeff.Format);
            IniCategory.AddProperty(StructureRegion.UseVelocityHeight.Key, orifice.UseVelocityHeight.ToString().ToLower());

            if (formula.UseMaxFlowPos && orifice.AllowPositiveFlow)
            {
                IniCategory.AddProperty(StructureRegion.UseLimitFlowPos.Key, formula.UseMaxFlowPos, StructureRegion.UseLimitFlowPos.Description);
                IniCategory.AddProperty(StructureRegion.LimitFlowPos.Key, formula.MaxFlowPos, StructureRegion.LimitFlowPos.Description);
            }

            if (formula.UseMaxFlowNeg && orifice.AllowNegativeFlow)
            {
                IniCategory.AddProperty(StructureRegion.UseLimitFlowNeg.Key, formula.UseMaxFlowNeg, StructureRegion.UseLimitFlowNeg.Description);
                IniCategory.AddProperty(StructureRegion.LimitFlowNeg.Key, formula.MaxFlowNeg, StructureRegion.LimitFlowNeg.Description);
            }

            return IniCategory;
        }
    }
}