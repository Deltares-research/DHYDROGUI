using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorStructureUniversalWeir"/> generates the <see cref="IniSection"/> corresponding with a
    /// <see cref="Weir"/> with a <see cref="FreeFormWeirFormula"/> in the structures.ini file.
    /// </summary>
    public class DefinitionGeneratorStructureUniversalWeir : DefinitionGeneratorTimeSeriesStructure
    {
        public DefinitionGeneratorStructureUniversalWeir(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.UniversalWeir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniSection;

            var formula = weir.WeirFormula as FreeFormWeirFormula;
            if (formula == null) return IniSection;
      
            AddAllowedFlowDir(weir);
            AddLevelsCount(formula);
            AddYValues(formula);
            AddZValues(formula);
            AddCrestLevel(weir);
            AddDischargeCoeff(formula);
            
            return IniSection;
        }

        private void AddDischargeCoeff(FreeFormWeirFormula formula) =>
            IniSection.AddPropertyFromConfiguration(StructureRegion.DischargeCoeff, formula.DischargeCoefficient);

        private void AddCrestLevel(IWeir weir)
        {
            AddProperty(weir.IsUsingTimeSeriesForCrestLevel(),
                        StructureRegion.CrestLevel,
                        weir.CrestLevel);
        }

        private void AddZValues(FreeFormWeirFormula formula) => 
            IniSection.AddPropertyFromConfigurationWithMultipleValues(StructureRegion.ZValues, formula.Z);

        private void AddYValues(FreeFormWeirFormula formula) => 
            IniSection.AddPropertyFromConfigurationWithMultipleValues(StructureRegion.YValues, formula.Y);

        private void AddLevelsCount(FreeFormWeirFormula formula) => 
            IniSection.AddPropertyFromConfiguration(StructureRegion.LevelsCount, formula.Y.ToList().Count);

        private void AddAllowedFlowDir(IWeir weir) => 
            IniSection.AddPropertyFromConfiguration(StructureRegion.AllowedFlowDir, weir.FlowDirection.ToString().ToLower());
    }
}
