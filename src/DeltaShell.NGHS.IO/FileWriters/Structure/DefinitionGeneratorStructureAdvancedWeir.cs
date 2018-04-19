using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureAdvancedWeir : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureAdvancedWeir(KeyValuePair<int, string> compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IStructure structure)
        {
            AddCommonRegionElements(structure, StructureRegion.StructureTypeName.AdvancedWeir);

            var weir = structure as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as PierWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            IniCategory.AddProperty(StructureRegion.NPiers.Key, formula.NumberOfPiers, StructureRegion.NPiers.Description);
            IniCategory.AddProperty(StructureRegion.PosHeight.Key, formula.UpstreamFacePos, StructureRegion.PosHeight.Description, StructureRegion.PosHeight.Format);
            IniCategory.AddProperty(StructureRegion.PosDesignHead.Key, formula.DesignHeadPos, StructureRegion.PosDesignHead.Description, StructureRegion.PosDesignHead.Format);
            IniCategory.AddProperty(StructureRegion.PosPierContractCoef.Key, formula.PierContractionPos, StructureRegion.PosPierContractCoef.Description, StructureRegion.PosPierContractCoef.Format);
            IniCategory.AddProperty(StructureRegion.PosAbutContractCoef.Key, formula.AbutmentContractionPos, StructureRegion.PosAbutContractCoef.Description, StructureRegion.PosAbutContractCoef.Format);

            IniCategory.AddProperty(StructureRegion.NegHeight.Key, formula.UpstreamFaceNeg, StructureRegion.NegHeight.Description, StructureRegion.NegHeight.Format);
            IniCategory.AddProperty(StructureRegion.NegDesignHead.Key, formula.DesignHeadNeg, StructureRegion.NegDesignHead.Description, StructureRegion.NegDesignHead.Format);
            IniCategory.AddProperty(StructureRegion.NegPierContractCoef.Key, formula.PierContractionNeg, StructureRegion.NegPierContractCoef.Description, StructureRegion.NegPierContractCoef.Format);
            IniCategory.AddProperty(StructureRegion.NegAbutContractCoef.Key, formula.AbutmentContractionNeg, StructureRegion.NegAbutContractCoef.Description, StructureRegion.NegAbutContractCoef.Format);

            return IniCategory;
        }
    }
}