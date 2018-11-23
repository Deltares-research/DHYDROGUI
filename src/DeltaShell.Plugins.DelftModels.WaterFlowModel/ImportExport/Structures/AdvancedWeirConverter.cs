using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    internal class AdvancedWeirConverter
    {
        public static IWeir ConvertToAdvancedWeir(DelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weir = new Weir();
            weir.WeirFormula = new GatedWeirFormula();

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);

            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestWidth.Key);
            weir.FlowDirection = (FlowDirection)structureBranchCategory.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);

            PierWeirFormula reference = ((PierWeirFormula) (weir.WeirFormula));
            reference.NumberOfPiers = structureBranchCategory.ReadProperty<int>(StructureRegion.NPiers.Key);

            reference.UpstreamFacePos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosHeight.Key);
            reference.DesignHeadPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosDesignHead.Key);
            reference.PierContractionPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosPierContractCoef.Key);
            reference.AbutmentContractionPos = structureBranchCategory.ReadProperty<double>(StructureRegion.PosAbutContractCoef.Key);

            reference.UpstreamFaceNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegHeight.Key);
            reference.DesignHeadNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegDesignHead.Key);
            reference.PierContractionNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegPierContractCoef.Key);
            reference.AbutmentContractionNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.NegAbutContractCoef.Key);
            
            return weir;
        }
    }
}