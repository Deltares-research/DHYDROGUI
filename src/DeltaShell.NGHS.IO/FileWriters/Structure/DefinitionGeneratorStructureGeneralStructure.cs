using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorStructureUniversalWeir"/> generates the <see cref="DelftIniCategory"/> corresponding with a
    /// <see cref="Weir"/> with a <see cref="GeneralStructureWeirFormula"/> in the structures.ini file.
    /// </summary>
    public class DefinitionGeneratorStructureGeneralStructure : DefinitionGeneratorTimeSeriesStructure
    {
        public DefinitionGeneratorStructureGeneralStructure(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override DelftIniCategory CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.GeneralStructure);

            var weir = hydroObject as Weir;
            if (weir == null) return IniCategory;

            var formula = weir.WeirFormula as GeneralStructureWeirFormula;
            if (formula == null) return IniCategory;

            IniCategory.AddProperty(StructureRegion.Upstream1Width.Key, formula.WidthLeftSideOfStructure, StructureRegion.Upstream1Width.Description, StructureRegion.Upstream1Width.Format);
            IniCategory.AddProperty(StructureRegion.Upstream2Width.Key, formula.WidthStructureLeftSide, StructureRegion.Upstream2Width.Description, StructureRegion.Upstream2Width.Format);
            IniCategory.AddProperty(StructureRegion.CrestWidth.Key, formula.WidthStructureCentre, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            IniCategory.AddProperty(StructureRegion.Downstream1Width.Key, formula.WidthStructureRightSide, StructureRegion.Downstream1Width.Description, StructureRegion.Downstream1Width.Format);
            IniCategory.AddProperty(StructureRegion.Downstream2Width.Key, formula.WidthRightSideOfStructure, StructureRegion.Downstream2Width.Description, StructureRegion.Downstream2Width.Format);

            IniCategory.AddProperty(StructureRegion.Upstream1Level.Key, formula.BedLevelLeftSideOfStructure, StructureRegion.Upstream1Level.Description, StructureRegion.Upstream1Level.Format);
            IniCategory.AddProperty(StructureRegion.Upstream2Level.Key, formula.BedLevelLeftSideStructure, StructureRegion.Upstream2Level.Description, StructureRegion.Upstream2Level.Format);
            AddCrestLevel(weir, formula);
            IniCategory.AddProperty(StructureRegion.Downstream1Level.Key, formula.BedLevelRightSideStructure, StructureRegion.Downstream1Level.Description, StructureRegion.Downstream1Level.Format);
            IniCategory.AddProperty(StructureRegion.Downstream2Level.Key, formula.BedLevelRightSideOfStructure, StructureRegion.Downstream2Level.Description, StructureRegion.Downstream2Level.Format);

            IniCategory.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, formula.LowerEdgeLevel, StructureRegion.GateLowerEdgeLevel.Description, StructureRegion.GateLowerEdgeLevel.Format);

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

            IniCategory.AddProperty(StructureRegion.CrestLength.Key, formula.CrestLength, StructureRegion.CrestLength.Description, StructureRegion.CrestLength.Format);
            IniCategory.AddProperty(StructureRegion.UseVelocityHeight.Key, weir.UseVelocityHeight.ToString().ToLower());

            var extraResistance = formula.UseExtraResistance ? formula.ExtraResistance : 0.0;
            IniCategory.AddProperty(StructureRegion.ExtraResistance.Key, extraResistance, StructureRegion.ExtraResistance.Description, StructureRegion.ExtraResistance.Format);
            
            IniCategory.AddProperty(StructureRegion.GateHeight.Key, formula.GateHeight, StructureRegion.GateHeight.Description , StructureRegion.GateHeight.Format);
            IniCategory.AddProperty(StructureRegion.GateOpeningWidth.Key, formula.GateOpeningWidth, StructureRegion.GateOpeningWidth.Description , StructureRegion.GateOpeningWidth.Format);

            // switch the horizontal direction, because enums aren't used very nicely in the csv file (structure definition).
            string horizontalDirection;
            switch (formula.GateOpeningHorizontalDirection)
            {
                case GateOpeningDirection.Symmetric:
                    horizontalDirection = "symmetric"; break;
                case GateOpeningDirection.FromLeft:
                    horizontalDirection = "fromLeft"; break;
                case GateOpeningDirection.FromRight:
                    horizontalDirection = "fromRight"; break;
                default:
                    throw new ArgumentException("We can't write " + formula.GateOpeningHorizontalDirection);
            }
            IniCategory.AddProperty(StructureRegion.GateHorizontalOpeningDirection.Key, horizontalDirection,StructureRegion.GateHorizontalOpeningDirection.Description);


            return IniCategory;
        }

        private void AddCrestLevel(IWeir weir, GeneralStructureWeirFormula formula)
        {
            AddProperty(weir.IsUsingTimeSeriesForCrestLevel(),
                        StructureRegion.CrestLevel.Key, 
                        formula.BedLevelStructureCentre, 
                        StructureRegion.CrestLevel.Description, 
                        StructureRegion.CrestLevel.Format);
        }
    }
}
