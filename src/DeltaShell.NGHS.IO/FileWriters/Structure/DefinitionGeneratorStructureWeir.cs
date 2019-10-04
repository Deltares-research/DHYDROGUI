using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureWeir : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureWeir(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Weir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as SimpleWeirFormula;
            if (formula == null) return IniCategory;
            
            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            //Jan Noort : weir.DischargeCoof gebruiken?
            IniCategory.AddProperty(StructureRegion.CorrectionCoeff.Key, double.Parse(StructureRegion.CorrectionCoeff.DefaultValue), StructureRegion.CorrectionCoeff.Description, StructureRegion.CorrectionCoeff.Format);
            
            return IniCategory;
        }
    }
}