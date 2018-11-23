using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    internal class GeneralStructureConverter
    {
        public static IWeir ConvertToGeneralStructure(DelftIniCategory structureBranchCategory, IList<IChannel> channelsList)
        {
            var weir = new Weir();
            weir.WeirFormula = new GeneralStructureWeirFormula();

            // Essential Properties (an error will be generated if these fail)
            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, channelsList, weir);
            
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).WidthLeftSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthLeftW1.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).WidthStructureLeftSide = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthLeftWsdl.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).WidthStructureCentre = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthCenter.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).WidthStructureRightSide = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthRightWsdr.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).WidthRightSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.WidthRightW2.Key);

            ((GeneralStructureWeirFormula)(weir.WeirFormula)).BedLevelLeftSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelLeftZb1.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).BedLevelLeftSideStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelLeftZbsl.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).BedLevelStructureCentre = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelCenter.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).BedLevelRightSideStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelRightZbsr.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).BedLevelRightSideOfStructure = structureBranchCategory.ReadProperty<double>(StructureRegion.LevelRightZb2.Key);

            ((GeneralStructureWeirFormula)(weir.WeirFormula)).GateOpening = structureBranchCategory.ReadProperty<double>(StructureRegion.GateHeight.Key)-weir.CrestLevel;

            ((GeneralStructureWeirFormula)(weir.WeirFormula)).PositiveFreeGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosFreeGateFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).PositiveDrownedGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosDrownGateFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).PositiveFreeWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosFreeWeirFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).PositiveDrownedWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.PosDrownWeirFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).PositiveContractionCoefficient = structureBranchCategory.ReadProperty<double>(StructureRegion.PosContrCoefFreeGate.Key);

            ((GeneralStructureWeirFormula)(weir.WeirFormula)).NegativeFreeGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegFreeGateFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).NegativeDrownedGateFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegDrownGateFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).NegativeFreeWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegFreeWeirFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).NegativeDrownedWeirFlow = structureBranchCategory.ReadProperty<double>(StructureRegion.NegDrownWeirFlowCoeff.Key);
            ((GeneralStructureWeirFormula)(weir.WeirFormula)).NegativeContractionCoefficient = structureBranchCategory.ReadProperty<double>(StructureRegion.NegContrCoefFreeGate.Key);

            var extraResistance = structureBranchCategory.ReadProperty<double>(StructureRegion.ExtraResistance.Key);
            if (Math.Abs(extraResistance) > 0.0)
            {
                ((GeneralStructureWeirFormula) (weir.WeirFormula)).ExtraResistance = extraResistance;
                ((GeneralStructureWeirFormula)(weir.WeirFormula)).UseExtraResistance = true;
            }
            else
            {
                ((GeneralStructureWeirFormula)(weir.WeirFormula)).UseExtraResistance = false;
            }
           return weir;
        }   
    }
}