using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureOrifice : DefinitionGeneratorTimeSeriesStructure
    {
        public DefinitionGeneratorStructureOrifice(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Orifice);

            if (!(hydroObject is IOrifice orifice) ||
                !(orifice.WeirFormula is GatedWeirFormula formula))
            {
                return IniSection;
            }

            AddAllowedFlowDir(orifice);
            AddCrestLevel(orifice);
            AddCrestWidth(orifice);
            AddGateLowerEdgeLevel(formula);
            AddCorrectionCoeff(formula);
            AddUseVelocityHeight(orifice);
            AddLimitFlowPos(orifice, formula);
            AddLimitFlowNeg(orifice, formula);

            return IniSection;
        }

        private void AddAllowedFlowDir(IOrifice orifice) => 
            IniSection.AddPropertyWithOptionalComment(StructureRegion.AllowedFlowDir.Key, 
                                    orifice.FlowDirection.ToString().ToLower(), 
                                    StructureRegion.AllowedFlowDir.Description);

        private void AddCrestLevel(IOrifice orifice)
        {
            AddProperty(orifice.IsUsingTimeSeriesForCrestLevel(),
                        StructureRegion.CrestLevel.Key,
                        orifice.CrestLevel,
                        StructureRegion.CrestLevel.Description,
                        StructureRegion.CrestLevel.Format);
        }

        private void AddCrestWidth(IOrifice orifice) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CrestWidth.Key, 
                                    orifice.CrestWidth, 
                                    StructureRegion.CrestWidth.Description, 
                                    StructureRegion.CrestWidth.Format);

        private void AddGateLowerEdgeLevel(GatedWeirFormula formula) =>
            AddProperty(formula.IsUsingTimeSeriesForLowerEdgeLevel(),
                        StructureRegion.GateLowerEdgeLevel.Key,
                        formula.LowerEdgeLevel,
                        StructureRegion.GateLowerEdgeLevel.Description,
                        StructureRegion.GateLowerEdgeLevel.Format);

        private void AddCorrectionCoeff(GatedWeirFormula formula) => 
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CorrectionCoeff.Key, 
                                    formula.ContractionCoefficient * formula.LateralContraction, 
                                    StructureRegion.CorrectionCoeff.Description, 
                                    StructureRegion.CorrectionCoeff.Format);

        private void AddUseVelocityHeight(IOrifice orifice) => 
            IniSection.AddPropertyWithOptionalComment(StructureRegion.UseVelocityHeight.Key, 
                                    orifice.UseVelocityHeight.ToString().ToLower());

        private void AddLimitFlowPos(IOrifice orifice, GatedWeirFormula formula)
        {
            if (!formula.UseMaxFlowPos || !orifice.AllowPositiveFlow) return;

            IniSection.AddProperty(StructureRegion.UseLimitFlowPos.Key, formula.UseMaxFlowPos, StructureRegion.UseLimitFlowPos.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.LimitFlowPos.Key, formula.MaxFlowPos, StructureRegion.LimitFlowPos.Description, StructureRegion.LimitFlowPos.Format);
        }

        private void AddLimitFlowNeg(IOrifice orifice, GatedWeirFormula formula)
        {
            if (!formula.UseMaxFlowNeg || !orifice.AllowNegativeFlow) return;

            IniSection.AddProperty(StructureRegion.UseLimitFlowNeg.Key, formula.UseMaxFlowNeg, StructureRegion.UseLimitFlowNeg.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.LimitFlowNeg.Key, formula.MaxFlowNeg, StructureRegion.LimitFlowNeg.Description, StructureRegion.LimitFlowNeg.Format);
        }
    }
}
