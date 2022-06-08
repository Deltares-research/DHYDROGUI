using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureOrifice : DefinitionGeneratorStructure
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Orifice);

            if (!(hydroObject is IOrifice orifice) ||
                !(orifice.WeirFormula is GatedWeirFormula formula))
            {
                return IniCategory;
            }

            AddAllowedFlowDir(orifice);
            AddCrestLevel(orifice);
            AddCrestWidth(orifice);
            AddGateLowerEdgeLevel(orifice, formula);
            AddCorrectionCoeff(formula);
            AddUseVelocityHeight(orifice);
            AddLimitFlowPos(orifice, formula);
            AddLimitFlowNeg(orifice, formula);

            return IniCategory;
        }

        private void AddAllowedFlowDir(IOrifice orifice) => 
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir.Key, 
                                    orifice.FlowDirection.ToString().ToLower(), 
                                    StructureRegion.AllowedFlowDir.Description);

        private void AddCrestLevel(IOrifice orifice)
        {
            if (orifice.CanBeTimedependent && orifice.UseCrestLevelTimeSeries)
            {
                // Note: the generation of tim files is the responsibility of the StructureFile
                //       not the DefinitionGeneratorStructureWeir.
                IniCategory.AddProperty(StructureRegion.CrestLevel.Key,
                                        StructureTimFileNameGenerator.Generate(orifice, orifice.CrestLevelTimeSeries), 
                                        StructureRegion.CrestLevel.Description);
            }
            else
            { 
                IniCategory.AddProperty(StructureRegion.CrestLevel.Key, 
                                        orifice.CrestLevel, 
                                        StructureRegion.CrestLevel.Description, 
                                        StructureRegion.CrestLevel.Format);
            }
        }

        private void AddCrestWidth(IOrifice orifice) => 
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, 
                                    orifice.CrestWidth, 
                                    StructureRegion.CrestWidth.Description, 
                                    StructureRegion.CrestWidth.Format);

        private void AddGateLowerEdgeLevel(IOrifice orifice, GatedWeirFormula formula)
        {
            if (formula.CanBeTimedependent && formula.UseLowerEdgeLevelTimeSeries)
            {
                IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key,
                                        StructureTimFileNameGenerator.Generate(orifice, formula.LowerEdgeLevelTimeSeries), 
                                        StructureRegion.GateLowerEdgeLevel.Description);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key,
                                        (orifice.CrestLevel + formula.GateOpening),
                                        StructureRegion.GateLowerEdgeLevel.Description,
                                        StructureRegion.GateLowerEdgeLevel.Format);
            }
        }

        private void AddCorrectionCoeff(GatedWeirFormula formula) => 
            IniCategory.AddProperty(StructureRegion.CorrectionCoeff.Key, 
                                    formula.ContractionCoefficient * formula.LateralContraction, 
                                    StructureRegion.CorrectionCoeff.Description, 
                                    StructureRegion.CorrectionCoeff.Format);

        private void AddUseVelocityHeight(IOrifice orifice) => 
            IniCategory.AddProperty(StructureRegion.UseVelocityHeight.Key, 
                                    orifice.UseVelocityHeight.ToString().ToLower());

        private void AddLimitFlowPos(IOrifice orifice, GatedWeirFormula formula)
        {
            if (!formula.UseMaxFlowPos || !orifice.AllowPositiveFlow) return;

            IniCategory.AddProperty(StructureRegion.UseLimitFlowPos.Key, formula.UseMaxFlowPos, StructureRegion.UseLimitFlowPos.Description);
            IniCategory.AddProperty(StructureRegion.LimitFlowPos.Key, formula.MaxFlowPos, StructureRegion.LimitFlowPos.Description, StructureRegion.LimitFlowPos.Format);
        }

        private void AddLimitFlowNeg(IOrifice orifice, GatedWeirFormula formula)
        {
            if (!formula.UseMaxFlowNeg || !orifice.AllowNegativeFlow) return;

            IniCategory.AddProperty(StructureRegion.UseLimitFlowNeg.Key, formula.UseMaxFlowNeg, StructureRegion.UseLimitFlowNeg.Description);
            IniCategory.AddProperty(StructureRegion.LimitFlowNeg.Key, formula.MaxFlowNeg, StructureRegion.LimitFlowNeg.Description, StructureRegion.LimitFlowNeg.Format);
        }
    }
}