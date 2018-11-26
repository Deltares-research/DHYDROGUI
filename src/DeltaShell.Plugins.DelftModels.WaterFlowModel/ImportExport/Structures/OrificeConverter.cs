using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class OrificeConverter : StructureConverter
    {
        public override IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weir = new Weir();
            weir.WeirFormula = new GatedWeirFormula();

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);

            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestWidth.Key);
            weir.FlowDirection = (FlowDirection)structureBranchCategory.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);

            ((GatedWeirFormula)(weir.WeirFormula)).ContractionCoefficient = structureBranchCategory.ReadProperty<double>(StructureRegion.ContractionCoeff.Key);
            ((GatedWeirFormula)(weir.WeirFormula)).LateralContraction = structureBranchCategory.ReadProperty<double>(StructureRegion.LatContrCoeff.Key);
            ((GatedWeirFormula)(weir.WeirFormula)).GateOpening = structureBranchCategory.ReadProperty<double>(StructureRegion.OpenLevel.Key) - weir.CrestLevel;
            ((GatedWeirFormula)(weir.WeirFormula)).UseMaxFlowPos = Convert.ToBoolean(structureBranchCategory.ReadProperty<int>(StructureRegion.UseLimitFlowPos.Key));
            ((GatedWeirFormula)(weir.WeirFormula)).UseMaxFlowNeg = Convert.ToBoolean(structureBranchCategory.ReadProperty<int>(StructureRegion.UseLimitFlowNeg.Key));
            ((GatedWeirFormula)(weir.WeirFormula)).MaxFlowPos = structureBranchCategory.ReadProperty<double>(StructureRegion.LimitFlowPos.Key);
            ((GatedWeirFormula)(weir.WeirFormula)).MaxFlowNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.LimitFlowNeg.Key);
            return weir;
        }   
    }
}