using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="DefinitionGeneratorStructureUniversalWeir"/> generates the <see cref="IniSection"/> corresponding with a
    /// <see cref="Weir"/> with a <see cref="GeneralStructureWeirFormula"/> in the structures.ini file.
    /// </summary>
    public class DefinitionGeneratorStructureGeneralStructure : DefinitionGeneratorTimeSeriesStructure
    {
        public DefinitionGeneratorStructureGeneralStructure(IStructureFileNameGenerator structureFileNameGenerator) : base(structureFileNameGenerator) {}
        
        public override IniSection CreateStructureRegion(IHydroObject hydroObject)
        {
            Ensure.NotNull(hydroObject, nameof(hydroObject));

            AddCommonRegionElements(hydroObject, StructureRegion.StructureTypeName.GeneralStructure);

            var weir = hydroObject as Weir;
            if (weir == null) return IniSection;

            var formula = weir.WeirFormula as GeneralStructureWeirFormula;
            if (formula == null) return IniSection;

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Upstream1Width.Key, formula.WidthLeftSideOfStructure, StructureRegion.Upstream1Width.Description, StructureRegion.Upstream1Width.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Upstream2Width.Key, formula.WidthStructureLeftSide, StructureRegion.Upstream2Width.Description, StructureRegion.Upstream2Width.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CrestWidth.Key, formula.WidthStructureCentre, StructureRegion.CrestWidth.Description, StructureRegion.CrestWidth.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Downstream1Width.Key, formula.WidthStructureRightSide, StructureRegion.Downstream1Width.Description, StructureRegion.Downstream1Width.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Downstream2Width.Key, formula.WidthRightSideOfStructure, StructureRegion.Downstream2Width.Description, StructureRegion.Downstream2Width.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Upstream1Level.Key, formula.BedLevelLeftSideOfStructure, StructureRegion.Upstream1Level.Description, StructureRegion.Upstream1Level.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Upstream2Level.Key, formula.BedLevelLeftSideStructure, StructureRegion.Upstream2Level.Description, StructureRegion.Upstream2Level.Format);
            AddCrestLevel(weir, formula);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Downstream1Level.Key, formula.BedLevelRightSideStructure, StructureRegion.Downstream1Level.Description, StructureRegion.Downstream1Level.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.Downstream2Level.Key, formula.BedLevelRightSideOfStructure, StructureRegion.Downstream2Level.Description, StructureRegion.Downstream2Level.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateLowerEdgeLevel.Key, formula.LowerEdgeLevel, StructureRegion.GateLowerEdgeLevel.Description, StructureRegion.GateLowerEdgeLevel.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosFreeGateFlowCoeff.Key, formula.PositiveFreeGateFlow, StructureRegion.PosFreeGateFlowCoeff.Description, StructureRegion.PosFreeGateFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosDrownGateFlowCoeff.Key, formula.PositiveDrownedGateFlow, StructureRegion.PosDrownGateFlowCoeff.Description, StructureRegion.PosDrownGateFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosFreeWeirFlowCoeff.Key, formula.PositiveFreeWeirFlow, StructureRegion.PosFreeWeirFlowCoeff.Description, StructureRegion.PosFreeWeirFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosDrownWeirFlowCoeff.Key, formula.PositiveDrownedWeirFlow, StructureRegion.PosDrownWeirFlowCoeff.Description, StructureRegion.PosDrownWeirFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.PosContrCoefFreeGate.Key, formula.PositiveContractionCoefficient, StructureRegion.PosContrCoefFreeGate.Description, StructureRegion.PosContrCoefFreeGate.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegFreeGateFlowCoeff.Key, formula.NegativeFreeGateFlow, StructureRegion.NegFreeGateFlowCoeff.Description, StructureRegion.NegFreeGateFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegDrownGateFlowCoeff.Key, formula.NegativeDrownedGateFlow, StructureRegion.NegDrownGateFlowCoeff.Description, StructureRegion.NegDrownGateFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegFreeWeirFlowCoeff.Key, formula.NegativeFreeWeirFlow, StructureRegion.NegFreeWeirFlowCoeff.Description, StructureRegion.NegFreeWeirFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegDrownWeirFlowCoeff.Key, formula.NegativeDrownedWeirFlow, StructureRegion.NegDrownWeirFlowCoeff.Description, StructureRegion.NegDrownWeirFlowCoeff.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.NegContrCoefFreeGate.Key, formula.NegativeContractionCoefficient, StructureRegion.NegContrCoefFreeGate.Description, StructureRegion.NegContrCoefFreeGate.Format);

            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.CrestLength.Key, formula.CrestLength, StructureRegion.CrestLength.Description, StructureRegion.CrestLength.Format);
            IniSection.AddPropertyWithOptionalComment(StructureRegion.UseVelocityHeight.Key, weir.UseVelocityHeight.ToString().ToLower());

            var extraResistance = formula.UseExtraResistance ? formula.ExtraResistance : 0.0;
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.ExtraResistance.Key, extraResistance, StructureRegion.ExtraResistance.Description, StructureRegion.ExtraResistance.Format);
            
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateHeight.Key, formula.GateHeight, StructureRegion.GateHeight.Description , StructureRegion.GateHeight.Format);
            IniSection.AddPropertyWithOptionalCommentAndFormat(StructureRegion.GateOpeningWidth.Key, formula.GateOpeningWidth, StructureRegion.GateOpeningWidth.Description , StructureRegion.GateOpeningWidth.Format);

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
            IniSection.AddPropertyWithOptionalComment(StructureRegion.GateHorizontalOpeningDirection.Key, horizontalDirection,StructureRegion.GateHorizontalOpeningDirection.Description);


            return IniSection;
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
