using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureWeir2D : DefinitionGeneratorStructure2D
    {
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Weir);

            var weir = (IWeir) hydroObject;
            AddCrestLevelProperty(weir);
            AddCrestWidthProperty(weir);
            AddLateralContractionCoefficientProperty(weir);

            return IniCategory;
        }

        private void AddCrestLevelProperty(IWeir weir)
        {
            if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
            {
                var timeSeriesFileName = $"{weir.Name}_crest_level.tim";
                IniCategory.AddProperty(StructureRegion.CrestLevel.Key, timeSeriesFileName, StructureRegion.CrestLevel.Description);
            }
            else
            {
                IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, "F");
            }
        }

        private void AddCrestWidthProperty(IWeir weir)
        {
            if (weir.CrestWidth > 0)
            {
                IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, "F");
            }
        }

        private void AddLateralContractionCoefficientProperty(IWeir weir)
        {
            var weirFormula = (SimpleWeirFormula) weir.WeirFormula;
            IniCategory.AddProperty(StructureRegion.LatContrCoeff.Key, weirFormula.LateralContraction, StructureRegion.LatContrCoeff.Description, "F");
        }
    }
}
