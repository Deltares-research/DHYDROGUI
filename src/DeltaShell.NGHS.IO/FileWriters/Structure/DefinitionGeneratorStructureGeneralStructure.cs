using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class DefinitionGeneratorStructureGeneralStructure : DefinitionGeneratorStructure
    {
        public DefinitionGeneratorStructureGeneralStructure(CompoundStructureInfo compoundStructureInfo)
            : base(compoundStructureInfo)
        {
        }

        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.GeneralStructure);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as GeneralStructureWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.WidthLeftW1.Key, formula.WidthLeftSideOfStructure, StructureRegion.WidthLeftW1.Description, StructureRegion.WidthLeftW1.Format);
            IniCategory.AddProperty(StructureRegion.WidthLeftWsdl.Key, formula.WidthStructureLeftSide, StructureRegion.WidthLeftWsdl.Description, StructureRegion.WidthLeftWsdl.Format);
            IniCategory.AddProperty(StructureRegion.WidthCenter.Key, formula.WidthStructureCentre, StructureRegion.WidthCenter.Description, StructureRegion.WidthCenter.Format);
            IniCategory.AddProperty(StructureRegion.WidthRightWsdr.Key, formula.WidthStructureRightSide, StructureRegion.WidthRightWsdr.Description, StructureRegion.WidthRightWsdr.Format);
            IniCategory.AddProperty(StructureRegion.WidthRightW2.Key, formula.WidthRightSideOfStructure, StructureRegion.WidthRightW2.Description, StructureRegion.WidthRightW2.Format);

            IniCategory.AddProperty(StructureRegion.LevelLeftZb1.Key, formula.BedLevelLeftSideOfStructure, StructureRegion.LevelLeftZb1.Description, StructureRegion.LevelLeftZb1.Format);
            IniCategory.AddProperty(StructureRegion.LevelLeftZbsl.Key, formula.BedLevelLeftSideStructure, StructureRegion.LevelLeftZbsl.Description, StructureRegion.LevelLeftZbsl.Format);
            IniCategory.AddProperty(StructureRegion.LevelCenter.Key, formula.BedLevelStructureCentre, StructureRegion.LevelCenter.Description, StructureRegion.LevelCenter.Format);
            IniCategory.AddProperty(StructureRegion.LevelRightZbsr.Key, formula.BedLevelRightSideStructure, StructureRegion.LevelRightZbsr.Description, StructureRegion.LevelRightZbsr.Format);
            IniCategory.AddProperty(StructureRegion.LevelRightZb2.Key, formula.BedLevelRightSideOfStructure, StructureRegion.LevelRightZb2.Description, StructureRegion.LevelRightZb2.Format);

            IniCategory.AddProperty(StructureRegion.GateHeight.Key, (weir.CrestLevel + formula.GateOpening), StructureRegion.GateHeight.Description, StructureRegion.GateHeight.Format);

            IniCategory.AddProperty(StructureRegion.PosFreeGateFlowCoeff.Key, formula.PositiveFreeGateFlow, StructureRegion.PosFreeGateFlowCoeff.Description, StructureRegion.PosFreeGateFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.PosDrownGateFlowCoeff.Key, formula.PositiveDrownedGateFlow, StructureRegion.PosDrownGateFlowCoeff.Description, StructureRegion.PosDrownGateFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.PosFreeWeirFlowCoeff.Key, formula.PositiveFreeWeirFlow, StructureRegion.PosFreeWeirFlowCoeff.Description, StructureRegion.PosFreeWeirFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.PosDrownWeirFlowCoeff.Key, formula.PositiveDrownedWeirFlow, StructureRegion.PosDrownWeirFlowCoeff.Description, StructureRegion.PosDrownWeirFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.PosContrCoefFreeGate.Key, formula.PositiveContractionCoefficient, StructureRegion.PosContrCoefFreeGate.Description, StructureRegion.PosContrCoefFreeGate.Format);

            IniCategory.AddProperty(StructureRegion.NegFreeGateFlowCoeff.Key, formula.NegativeFreeGateFlow, StructureRegion.NegFreeGateFlowCoeff.Description, StructureRegion.NegFreeGateFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.NegDrownGateFlowCoeff.Key, formula.NegativeDrownedGateFlow, StructureRegion.NegDrownGateFlowCoeff.Description, StructureRegion.NegDrownGateFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.NegFreeWeirFlowCoeff.Key, formula.NegativeFreeWeirFlow, StructureRegion.NegFreeWeirFlowCoeff.Description, StructureRegion.NegFreeWeirFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.NegDrownWeirFlowCoeff.Key, formula.NegativeDrownedWeirFlow, StructureRegion.NegDrownWeirFlowCoeff.Description, StructureRegion.NegDrownWeirFlowCoeff.Format);
            IniCategory.AddProperty(StructureRegion.NegContrCoefFreeGate.Key, formula.NegativeContractionCoefficient, StructureRegion.NegContrCoefFreeGate.Description, StructureRegion.NegContrCoefFreeGate.Format);
            
            var extraResistance = formula.UseExtraResistance ? formula.ExtraResistance : 0.0;
            IniCategory.AddProperty(StructureRegion.ExtraResistance.Key, extraResistance, StructureRegion.ExtraResistance.Description, StructureRegion.ExtraResistance.Format);

            if (((IBranchFeature)hydroObject).Branch == null)
            {
                /* for FM, add the GateDoorHeight for a general structure. 
                 * Checking if it is 1D or 2D by checking the structure branch == null is not the most awesome way to do this.
                 * Refactoring and splitting 1D/2D functionality is recommended.*/
                IniCategory.AddProperty(StructureRegion.GateDoorHeight.Key, formula.GateOpening, StructureRegion.GateDoorHeight.Description , StructureRegion.GateDoorHeight.Format);
            }

            return IniCategory;
        }
    }
}