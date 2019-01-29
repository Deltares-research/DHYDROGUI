using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class AdvancedWeirConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new PierWeirFormula()
            };
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var weir = structure as Weir;
            var weirFormula = weir.WeirFormula as PierWeirFormula;

            weir.CrestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = category.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            weirFormula.NumberOfPiers = category.ReadProperty<int>(StructureRegion.NPiers.Key);

            weirFormula.UpstreamFacePos = category.ReadProperty<double>(StructureRegion.PosHeight.Key);
            weirFormula.DesignHeadPos = category.ReadProperty<double>(StructureRegion.PosDesignHead.Key);
            weirFormula.PierContractionPos = category.ReadProperty<double>(StructureRegion.PosPierContractCoef.Key);
            weirFormula.AbutmentContractionPos = category.ReadProperty<double>(StructureRegion.PosAbutContractCoef.Key);

            weirFormula.UpstreamFaceNeg = category.ReadProperty<double>(StructureRegion.NegHeight.Key);
            weirFormula.DesignHeadNeg = category.ReadProperty<double>(StructureRegion.NegDesignHead.Key);
            weirFormula.PierContractionNeg = category.ReadProperty<double>(StructureRegion.NegPierContractCoef.Key);
            weirFormula.AbutmentContractionNeg = category.ReadProperty<double>(StructureRegion.NegAbutContractCoef.Key);
        }
    }
}