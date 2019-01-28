using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class AdvancedWeirConverter : IStructureConverter
    {
        public IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory, IBranch branch)
        {
            var weirFormula = new PierWeirFormula();

            var weir = new Weir
            {
                WeirFormula = weirFormula
            };

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, branch, weir);

            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestWidth.Key);
            
            weirFormula.NumberOfPiers = structureBranchCategory.ReadProperty<int>(StructureRegion.NPiers.Key);

            weirFormula.UpstreamFacePos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosHeight.Key);
            weirFormula.DesignHeadPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosDesignHead.Key);
            weirFormula.PierContractionPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosPierContractCoef.Key);
            weirFormula.AbutmentContractionPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosAbutContractCoef.Key);

            weirFormula.UpstreamFaceNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegHeight.Key);
            weirFormula.DesignHeadNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegDesignHead.Key);
            weirFormula.PierContractionNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegPierContractCoef.Key);
            weirFormula.AbutmentContractionNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegAbutContractCoef.Key);
            
            return weir;
        }
    }
}