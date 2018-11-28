using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class OrificeConverter : IStructureConverter
    {
        public IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory,
            IList<IChannel> channelsList)
        {
            var weirFormula = new GatedWeirFormula();

            var weir = new Weir
            {
                WeirFormula = weirFormula
            };

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);
            
            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestWidth.Key);
            weir.FlowDirection =
                (FlowDirection) structureBranchCategory.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);

            weirFormula.ContractionCoefficient =
                structureBranchCategory.ReadProperty<double>(StructureRegion.ContractionCoeff.Key);
            weirFormula.LateralContraction =
                structureBranchCategory.ReadProperty<double>(StructureRegion.LatContrCoeff.Key);
            weirFormula.GateOpening = structureBranchCategory.ReadProperty<double>(StructureRegion.OpenLevel.Key) -
                                      weir.CrestLevel;
            weirFormula.UseMaxFlowPos =
                Convert.ToBoolean(structureBranchCategory.ReadProperty<int>(StructureRegion.UseLimitFlowPos.Key));
            weirFormula.UseMaxFlowNeg =
                Convert.ToBoolean(structureBranchCategory.ReadProperty<int>(StructureRegion.UseLimitFlowNeg.Key));
            weirFormula.MaxFlowPos = structureBranchCategory.ReadProperty<double>(StructureRegion.LimitFlowPos.Key);
            weirFormula.MaxFlowNeg = structureBranchCategory.ReadProperty<double>(StructureRegion.LimitFlowNeg.Key);

            return weir;
        }
    }
}