using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorStructureUniversalWeir"/> generates the <see cref="DelftIniCategory"/> corresponding with a
    /// <see cref="Weir"/> with a <see cref="FreeFormWeirFormula"/> in the structures.ini file.
    /// </summary>
    public class DefinitionGeneratorStructureUniversalWeir : DefinitionGeneratorTimeSeriesStructure
    {
        public DefinitionGeneratorStructureUniversalWeir(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.UniversalWeir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as FreeFormWeirFormula;
            if (formula == null) return IniCategory;
      
            AddAllowedFlowDir(weir);
            AddLevelsCount(formula);
            AddYValues(formula);
            AddZValues(formula);
            AddCrestLevel(weir);
            AddDischargeCoeff(formula);
            
            return IniCategory;
        }

        private void AddDischargeCoeff(FreeFormWeirFormula formula) =>
            IniCategory.AddProperty(StructureRegion.DischargeCoeff, formula.DischargeCoefficient);

        private void AddCrestLevel(IWeir weir)
        {
            AddProperty(weir.IsUsingTimeSeriesForCrestLevel(),
                        StructureRegion.CrestLevel,
                        weir.CrestLevel);
        }

        private void AddZValues(FreeFormWeirFormula formula) => 
            IniCategory.AddProperty(StructureRegion.ZValues, formula.Z);

        private void AddYValues(FreeFormWeirFormula formula) => 
            IniCategory.AddProperty(StructureRegion.YValues, formula.Y);

        private void AddLevelsCount(FreeFormWeirFormula formula) => 
            IniCategory.AddProperty(StructureRegion.LevelsCount, formula.Y.ToList().Count);

        private void AddAllowedFlowDir(IWeir weir) => 
            IniCategory.AddProperty(StructureRegion.AllowedFlowDir, weir.FlowDirection.ToString().ToLower());
    }
}
