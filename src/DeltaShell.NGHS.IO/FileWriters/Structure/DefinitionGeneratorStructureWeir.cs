using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorStructureWeir"/> generates the <see cref="IniSection"/> corresponding with a
    /// <see cref="Weir"/> in the structures.ini file.
    /// </summary>
    public sealed class DefinitionGeneratorStructureWeir : DefinitionGeneratorTimeSeriesStructure
    {
        public DefinitionGeneratorStructureWeir(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Weir);

            if (!(hydroObject is IWeir weir) || !(weir.WeirFormula is SimpleWeirFormula formula))
            {
                return IniSection;
            }

            AddAllowedFlowDir(weir);
            AddCrestLevel(weir);
            AddCrestWidth(weir);
            AddCorrectionCoeff(formula);
            AddUseVelocityHeight(weir);

            return IniSection;
        }

        private void AddAllowedFlowDir(IWeir weir) =>
            IniSection.AddPropertyFromConfiguration(StructureRegion.AllowedFlowDir, weir.FlowDirection.ToString().ToLower());

        private void AddCrestLevel(IWeir weir)
        {
            AddProperty(weir.IsUsingTimeSeriesForCrestLevel(),
                        StructureRegion.CrestLevel,
                        weir.CrestLevel);
        }

        private void AddCrestWidth(IWeir weir)
        {
            if (weir.CrestWidth > 0)
            {
                IniSection.AddPropertyFromConfiguration(StructureRegion.CrestWidth, weir.CrestWidth);
            }
        }

        private void AddCorrectionCoeff(SimpleWeirFormula formula) =>
            IniSection.AddPropertyFromConfiguration(StructureRegion.CorrectionCoeff,
                                    formula.CorrectionCoefficient);

        private void AddUseVelocityHeight(IWeir weir) =>
            IniSection.AddPropertyFromConfiguration(StructureRegion.UseVelocityHeight, 
                                    weir.UseVelocityHeight.ToString().ToLower());
    }
}
