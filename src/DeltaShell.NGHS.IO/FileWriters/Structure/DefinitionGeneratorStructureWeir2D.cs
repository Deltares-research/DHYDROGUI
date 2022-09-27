using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureWeir2D : DefinitionGeneratorTimeSeriesStructure2D
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Weir);

            var weir = (IWeir) hydroObject;
            AddCrestLevelProperty(weir);
            AddCrestWidthProperty(weir);
            AddCorrectionCoefficientProperty(weir);
            AddUseVelocityHeightProperty(weir);

            return IniCategory;
        }

        private void AddCrestLevelProperty(IWeir weir)
        {
            AddProperty(weir.IsUsingTimeSeriesForCrestLevel(),
                        StructureRegion.CrestLevel.Key,
                        weir.CrestLevel,
                        StructureRegion.CrestLevel.Description,
                        StructureRegion.CrestLevel.Format,
                        weir,
                        weir.CrestLevelTimeSeries);
        }

        private void AddCrestWidthProperty(IWeir weir)
        {
            if (weir.CrestWidth > 0)
            {
                IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            }
        }

        private void AddCorrectionCoefficientProperty(IWeir weir)
        {
            var weirFormula = (SimpleWeirFormula) weir.WeirFormula;
            IniCategory.AddProperty(StructureRegion.CorrectionCoeff.Key, weirFormula.CorrectionCoefficient, StructureRegion.CorrectionCoeff.Description, StructureRegion.CorrectionCoeff.Format);
        }

        private void AddUseVelocityHeightProperty(IWeir weir)
        {
            IniCategory.AddProperty(StructureRegion.UseVelocityHeight.Key, weir.UseVelocityHeight.ToString().ToLower());
        }
    }
}
