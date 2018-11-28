using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class GeneralStructureConverter : IStructureConverter
    {
        public IStructure1D ConvertToStructure1D(IDelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weirFormula = new GeneralStructureWeirFormula();

            var weir = new Weir()
            {
                WeirFormula = weirFormula
            };

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);

            weirFormula.WidthLeftSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthLeftW1.Key);
            weirFormula.WidthStructureLeftSide = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthLeftWsdl.Key);
            weirFormula.WidthStructureCentre = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthCenter.Key);
            weirFormula.WidthStructureRightSide = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthRightWsdr.Key);
            weirFormula.WidthRightSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthRightW2.Key);

            weirFormula.BedLevelLeftSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelLeftZb1.Key);
            weirFormula.BedLevelLeftSideStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelLeftZbsl.Key);
            weirFormula.BedLevelStructureCentre = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelCenter.Key);
            weirFormula.BedLevelRightSideStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelRightZbsr.Key);
            weirFormula.BedLevelRightSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelRightZb2.Key);

            weirFormula.GateOpening = structureBranchCategory.ReadProperty<double>(StructureRegion.GateHeight.Key)-weir.CrestLevel;

            weirFormula.PositiveFreeGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosFreeGateFlowCoeff.Key);
            weirFormula.PositiveDrownedGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosDrownGateFlowCoeff.Key);
            weirFormula.PositiveFreeWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosFreeWeirFlowCoeff.Key);
            weirFormula.PositiveDrownedWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosDrownWeirFlowCoeff.Key);
            weirFormula.PositiveContractionCoefficient = structureBranchCategory.ReadProperty<double>(StructureRegion.PosContrCoefFreeGate.Key);

            weirFormula.NegativeFreeGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegFreeGateFlowCoeff.Key);
            weirFormula.NegativeDrownedGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegDrownGateFlowCoeff.Key);
            weirFormula.NegativeFreeWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegFreeWeirFlowCoeff.Key);
            weirFormula.NegativeDrownedWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegDrownWeirFlowCoeff.Key);
            weirFormula.NegativeContractionCoefficient = structureBranchCategory.ReadProperty<double>(StructureRegion.NegContrCoefFreeGate.Key);

            var extraResistance = structureBranchCategory.ReadProperty<double>(StructureRegion.ExtraResistance.Key);
            if (Math.Abs(extraResistance) > 0.0)
            {
                ((GeneralStructureWeirFormula) (weir.WeirFormula)).ExtraResistance = extraResistance;
                weirFormula.UseExtraResistance = true;
            }
            else
            {
                weirFormula.UseExtraResistance = false;
            }
           return weir;
        }   
    }
}