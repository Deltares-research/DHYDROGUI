using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureOrifice : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureOrifice(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.Orifice);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as GatedWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);

            IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, (weir.CrestLevel + formula.GateOpening), StructureRegion.GateLowerEdgeLevel.Description, StructureRegion.GateLowerEdgeLevel.Format);
            
            //Jan Noort : welke Coof gebruiken?
            IniCategory.AddProperty(StructureRegion.CorrectionCoeff.Key, double.Parse(StructureRegion.CorrectionCoeff.DefaultValue), StructureRegion.CorrectionCoeff.Description, StructureRegion.CorrectionCoeff.Format);


            return IniCategory;
        }
    }
}