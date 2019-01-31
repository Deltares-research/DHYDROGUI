using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using System;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class GeneralStructureConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new GeneralStructureWeirFormula()
            };
        }

        protected override void SetStructureProperties()
        {
            var weir = Structure as Weir;
            var weirFormula = weir.WeirFormula as GeneralStructureWeirFormula;

            weirFormula.WidthLeftSideOfStructure = Category.ReadProperty<double>(StructureRegion.WidthLeftW1.Key);
            weirFormula.WidthStructureLeftSide = Category.ReadProperty<double>(StructureRegion.WidthLeftWsdl.Key);
            weirFormula.WidthStructureCentre = Category.ReadProperty<double>(StructureRegion.WidthCenter.Key);
            weirFormula.WidthStructureRightSide = Category.ReadProperty<double>(StructureRegion.WidthRightWsdr.Key);
            weirFormula.WidthRightSideOfStructure = Category.ReadProperty<double>(StructureRegion.WidthRightW2.Key);

            weirFormula.BedLevelLeftSideOfStructure = Category.ReadProperty<double>(StructureRegion.LevelLeftZb1.Key);
            weirFormula.BedLevelLeftSideStructure = Category.ReadProperty<double>(StructureRegion.LevelLeftZbsl.Key);
            weirFormula.BedLevelStructureCentre = Category.ReadProperty<double>(StructureRegion.LevelCenter.Key);
            weirFormula.BedLevelRightSideStructure = Category.ReadProperty<double>(StructureRegion.LevelRightZbsr.Key);
            weirFormula.BedLevelRightSideOfStructure = Category.ReadProperty<double>(StructureRegion.LevelRightZb2.Key);

            weirFormula.GateOpening = Category.ReadProperty<double>(StructureRegion.GateHeight.Key) - weir.CrestLevel;

            weirFormula.PositiveFreeGateFlow = Category.ReadProperty<double>(StructureRegion.PosFreeGateFlowCoeff.Key);
            weirFormula.PositiveDrownedGateFlow = Category.ReadProperty<double>(StructureRegion.PosDrownGateFlowCoeff.Key);
            weirFormula.PositiveFreeWeirFlow = Category.ReadProperty<double>(StructureRegion.PosFreeWeirFlowCoeff.Key);
            weirFormula.PositiveDrownedWeirFlow = Category.ReadProperty<double>(StructureRegion.PosDrownWeirFlowCoeff.Key);
            weirFormula.PositiveContractionCoefficient = Category.ReadProperty<double>(StructureRegion.PosContrCoefFreeGate.Key);

            weirFormula.NegativeFreeGateFlow = Category.ReadProperty<double>(StructureRegion.NegFreeGateFlowCoeff.Key);
            weirFormula.NegativeDrownedGateFlow = Category.ReadProperty<double>(StructureRegion.NegDrownGateFlowCoeff.Key);
            weirFormula.NegativeFreeWeirFlow = Category.ReadProperty<double>(StructureRegion.NegFreeWeirFlowCoeff.Key);
            weirFormula.NegativeDrownedWeirFlow = Category.ReadProperty<double>(StructureRegion.NegDrownWeirFlowCoeff.Key);
            weirFormula.NegativeContractionCoefficient = Category.ReadProperty<double>(StructureRegion.NegContrCoefFreeGate.Key);

            var extraResistance = Category.ReadProperty<double>(StructureRegion.ExtraResistance.Key);
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