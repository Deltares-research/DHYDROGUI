using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorStructureWeir"/> generates the <see cref="DelftIniCategory"/> corresponding with a
    /// <see cref="Weir"/> in the structures.ini file.
    /// </summary>
    public sealed class DefinitionGeneratorStructureWeir : DefinitionGeneratorStructure
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Weir);

            if (!(hydroObject is IWeir weir) || !(weir.WeirFormula is SimpleWeirFormula formula))
            {
                return IniCategory;
            }

            AddAllowedFlowDir(weir);
            AddCrestLevel(weir);
            AddCrestWidth(weir);
            AddCorrectionCoeff(formula);
            AddUseVelocityHeight(weir);

            return IniCategory;
        }

        private void AddAllowedFlowDir(IWeir weir) =>
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir, weir.FlowDirection.ToString().ToLower());

        private void AddCrestLevel(IWeir weir)
        {
            if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
            {
                // Note: the generation of tim files is the responsibility of the StructureFile
                //       not the DefinitionGeneratorStructureWeir.
                IniCategory.AddProperty(StructureRegion.CrestLevel, 
                                         StructureTimFileNameGenerator.Generate(weir, weir.CrestLevelTimeSeries));
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.CrestLevel, weir.CrestLevel);
            }
        }

        private void AddCrestWidth(IWeir weir)
        {
            if (weir.CrestWidth > 0)
            {
                IniCategory.AddProperty(StructureRegion.CrestWidth, weir.CrestWidth);
            }
        }

        private void AddCorrectionCoeff(SimpleWeirFormula formula) =>
            IniCategory.AddProperty(StructureRegion.CorrectionCoeff,
                                    formula.CorrectionCoefficient);

        private void AddUseVelocityHeight(IWeir weir) =>
            IniCategory.AddProperty(StructureRegion.UseVelocityHeight, 
                                    weir.UseVelocityHeight.ToString().ToLower());
    }
}