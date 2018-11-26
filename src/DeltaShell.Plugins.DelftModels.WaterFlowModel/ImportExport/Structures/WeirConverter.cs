using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class WeirConverter : StructureConverter
    {
        public override IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory,
            IList<IChannel> channelsList)
        {
            var weir = new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);

            weir.CrestLevel = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = structureBranchCategory.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            ((SimpleWeirFormula) (weir.WeirFormula)).DischargeCoefficient =
                structureBranchCategory.ReadProperty<double>(StructureRegion.DischargeCoeff.Key);
            ((SimpleWeirFormula) (weir.WeirFormula)).LateralContraction =
                structureBranchCategory.ReadProperty<double>(StructureRegion.LatDisCoeff.Key);

            weir.FlowDirection =
                (FlowDirection) structureBranchCategory.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
            return weir;
        }

    }
}
