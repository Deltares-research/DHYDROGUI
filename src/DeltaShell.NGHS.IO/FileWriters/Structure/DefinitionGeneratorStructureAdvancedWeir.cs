using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureAdvancedWeir : DefinitionGeneratorStructure
    {
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.AdvancedWeir);

            var weir = hydroObject as Weir;
            if (weir == null) return IniSection;

            var formula = weir.WeirFormula as PierWeirFormula;
            if (formula == null) return IniSection;

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CrestLevel.Key, weir.CrestLevel, StructureRegion.CrestLevel.Description, StructureRegion.CrestLevel.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CrestWidth.Key, weir.CrestWidth, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            IniSection.AddProperty(StructureRegion.NPiers.Key, formula.NumberOfPiers, StructureRegion.NPiers.Description);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosHeight.Key, formula.UpstreamFacePos, StructureRegion.PosHeight.Description, StructureRegion.PosHeight.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosDesignHead.Key, formula.DesignHeadPos, StructureRegion.PosDesignHead.Description, StructureRegion.PosDesignHead.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosPierContractCoef.Key, formula.PierContractionPos, StructureRegion.PosPierContractCoef.Description, StructureRegion.PosPierContractCoef.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosAbutContractCoef.Key, formula.AbutmentContractionPos, StructureRegion.PosAbutContractCoef.Description, StructureRegion.PosAbutContractCoef.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegHeight.Key, formula.UpstreamFaceNeg, StructureRegion.NegHeight.Description, StructureRegion.NegHeight.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegDesignHead.Key, formula.DesignHeadNeg, StructureRegion.NegDesignHead.Description, StructureRegion.NegDesignHead.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegPierContractCoef.Key, formula.PierContractionNeg, StructureRegion.NegPierContractCoef.Description, StructureRegion.NegPierContractCoef.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegAbutContractCoef.Key, formula.AbutmentContractionNeg, StructureRegion.NegAbutContractCoef.Description, StructureRegion.NegAbutContractCoef.Format);

            return IniSection;
        }
    }
}