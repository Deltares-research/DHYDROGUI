using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class GeneralStructureConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new GeneralStructureWeirFormula()
            };
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var weir = structure as Weir;
            var weirFormula = weir.WeirFormula as GeneralStructureWeirFormula;

            weirFormula.WidthLeftSideOfStructure = category.ReadProperty<double>(StructureRegion.WidthLeftW1.Key);
            weirFormula.WidthStructureLeftSide = category.ReadProperty<double>(StructureRegion.WidthLeftWsdl.Key);
            weirFormula.WidthStructureCentre = category.ReadProperty<double>(StructureRegion.WidthCenter.Key);
            weirFormula.WidthStructureRightSide = category.ReadProperty<double>(StructureRegion.WidthRightWsdr.Key);
            weirFormula.WidthRightSideOfStructure = category.ReadProperty<double>(StructureRegion.WidthRightW2.Key);

            weirFormula.BedLevelLeftSideOfStructure = category.ReadProperty<double>(StructureRegion.LevelLeftZb1.Key);
            weirFormula.BedLevelLeftSideStructure = category.ReadProperty<double>(StructureRegion.LevelLeftZbsl.Key);
            weirFormula.BedLevelStructureCentre = category.ReadProperty<double>(StructureRegion.LevelCenter.Key);
            weirFormula.BedLevelRightSideStructure = category.ReadProperty<double>(StructureRegion.LevelRightZbsr.Key);
            weirFormula.BedLevelRightSideOfStructure = category.ReadProperty<double>(StructureRegion.LevelRightZb2.Key);

            weirFormula.GateOpening = category.ReadProperty<double>(StructureRegion.GateHeight.Key) - weir.CrestLevel;

            weirFormula.PositiveFreeGateFlow = category.ReadProperty<double>(StructureRegion.PosFreeGateFlowCoeff.Key);
            weirFormula.PositiveDrownedGateFlow = category.ReadProperty<double>(StructureRegion.PosDrownGateFlowCoeff.Key);
            weirFormula.PositiveFreeWeirFlow = category.ReadProperty<double>(StructureRegion.PosFreeWeirFlowCoeff.Key);
            weirFormula.PositiveDrownedWeirFlow = category.ReadProperty<double>(StructureRegion.PosDrownWeirFlowCoeff.Key);
            weirFormula.PositiveContractionCoefficient = category.ReadProperty<double>(StructureRegion.PosContrCoefFreeGate.Key);

            weirFormula.NegativeFreeGateFlow = category.ReadProperty<double>(StructureRegion.NegFreeGateFlowCoeff.Key);
            weirFormula.NegativeDrownedGateFlow = category.ReadProperty<double>(StructureRegion.NegDrownGateFlowCoeff.Key);
            weirFormula.NegativeFreeWeirFlow = category.ReadProperty<double>(StructureRegion.NegFreeWeirFlowCoeff.Key);
            weirFormula.NegativeDrownedWeirFlow = category.ReadProperty<double>(StructureRegion.NegDrownWeirFlowCoeff.Key);
            weirFormula.NegativeContractionCoefficient = category.ReadProperty<double>(StructureRegion.NegContrCoefFreeGate.Key);

            var extraResistance = category.ReadProperty<double>(StructureRegion.ExtraResistance.Key);
            if (Math.Abs(extraResistance) > 0.0)
            {
                ((GeneralStructureWeirFormula)(weir.WeirFormula)).ExtraResistance = extraResistance;
                weirFormula.UseExtraResistance = true;
            }
            else
            {
                weirFormula.UseExtraResistance = false;
            }
        }
    }
}