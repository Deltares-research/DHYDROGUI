using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    class OrificeDefinitionReader : WeirDefinitionReader
    {
        public override IStructure1D ReadDefinition(IDelftIniCategory category,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var allowedFlowDirValue = category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key, true);
            var allowedFlowDir = allowedFlowDirValue != null
                ? (FlowDirection)Enum.Parse(typeof(FlowDirection), allowedFlowDirValue, true)
                : 0;

            var orifice = new Orifice
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                CrestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key),
                CrestWidth = category.ReadProperty<double>(StructureRegion.CrestWidth.Key, true),
                FlowDirection = allowedFlowDir,
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                UseVelocityHeight = category.ReadProperty<bool>(StructureRegion.UseVelocityHeight.Key, true, true)
            };

            orifice.WeirFormula = ReadFormulaFromDefinition(category, orifice);

            return orifice;
        }

    }
    class WeirDefinitionReader : IStructureDefinitionReader
    {
        /// <inheritdoc/>
        public virtual IStructure1D ReadDefinition(IDelftIniCategory category,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var allowedFlowDirValue = category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key, true);
            var allowedFlowDir = allowedFlowDirValue != null
                ? (FlowDirection) Enum.Parse(typeof(FlowDirection), allowedFlowDirValue, true)
                : 0;

            var weir = new Weir
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                CrestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key),
                CrestWidth = category.ReadProperty<double>(StructureRegion.CrestWidth.Key, true),
                FlowDirection = allowedFlowDir,
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                UseVelocityHeight = category.ReadProperty<bool>(StructureRegion.UseVelocityHeight.Key, true, true)
            };

            weir.WeirFormula = ReadFormulaFromDefinition(category, weir);

            return weir;
        }

        protected IWeirFormula ReadFormulaFromDefinition(IDelftIniCategory category, Weir weir)
        {
            var type = (StructureType) Enum.Parse(typeof(StructureType),
                category.ReadProperty<string>(StructureRegion.DefinitionType.Key), true);

            switch (type)
            {
                case StructureType.Weir:
                    return new SimpleWeirFormula
                    {
                        CorrectionCoefficient = category.ReadProperty<double>(StructureRegion.CorrectionCoeff.Key, true,1.0)
                    };
                case StructureType.UniversalWeir:
                    var readFormulaFromDefinition = new FreeFormWeirFormula
                    {
                        DischargeCoefficient = category.ReadProperty<double>(StructureRegion.DischargeCoeff.Key),
                        CrestLevel = weir.CrestLevel
                    };

                    var yValues = category.ReadProperty<string>(StructureRegion.YValues.Key).ToDoubleArray();
                    var zValues = category.ReadProperty<string>(StructureRegion.ZValues.Key).ToDoubleArray();
                    readFormulaFromDefinition.SetShape(yValues, zValues);

                    return readFormulaFromDefinition;
                case StructureType.RiverWeir:

                    var riverWeirFormula = new RiverWeirFormula
                    {
                        CorrectionCoefficientPos = category.ReadProperty<double>(StructureRegion.PosCwCoef.Key),
                        SubmergeLimitPos = category.ReadProperty<double>(StructureRegion.PosSlimLimit.Key),
                        CorrectionCoefficientNeg = category.ReadProperty<double>(StructureRegion.NegCwCoef.Key),
                        SubmergeLimitNeg = category.ReadProperty<double>(StructureRegion.NegSlimLimit.Key)
                    };

                    var posSf = category.ReadProperty<string>(StructureRegion.PosSf.Key, true).ToDoubleArray();
                    var posRed = category.ReadProperty<string>(StructureRegion.PosRed.Key, true).ToDoubleArray();

                    riverWeirFormula.SubmergeReductionPos =
                        riverWeirFormula.SubmergeReductionPos.CreateFunctionFromArrays(posSf, posRed);

                    var negSf = category.ReadProperty<string>(StructureRegion.NegSf.Key, true).ToDoubleArray();
                    var negRed = category.ReadProperty<string>(StructureRegion.NegRed.Key, true).ToDoubleArray();

                    riverWeirFormula.SubmergeReductionPos =
                        riverWeirFormula.SubmergeReductionNeg.CreateFunctionFromArrays(negSf, negRed);

                    return riverWeirFormula;
                case StructureType.AdvancedWeir:
                    return new PierWeirFormula
                    {
                        NumberOfPiers = category.ReadProperty<int>(StructureRegion.NPiers.Key),
                        UpstreamFacePos = category.ReadProperty<double>(StructureRegion.PosHeight.Key),
                        DesignHeadPos = category.ReadProperty<double>(StructureRegion.PosDesignHead.Key),
                        PierContractionPos = category.ReadProperty<double>(StructureRegion.PosPierContractCoef.Key),
                        AbutmentContractionPos = category.ReadProperty<double>(StructureRegion.PosAbutContractCoef.Key),

                        UpstreamFaceNeg = category.ReadProperty<double>(StructureRegion.NegHeight.Key),
                        DesignHeadNeg = category.ReadProperty<double>(StructureRegion.NegDesignHead.Key),
                        PierContractionNeg = category.ReadProperty<double>(StructureRegion.NegPierContractCoef.Key),
                        AbutmentContractionNeg = category.ReadProperty<double>(StructureRegion.NegAbutContractCoef.Key)
                    };
                case StructureType.Orifice:
                    return new GatedWeirFormula
                    {
                        GateOpening = category.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key) - weir.CrestLevel,
                        ContractionCoefficient = category.ReadProperty<double>(StructureRegion.CorrectionCoeff.Key, true, 1.0),
                        LateralContraction = 1
                    };
                case StructureType.GeneralStructure:
                    var extraResistance = category.ReadProperty<double>(StructureRegion.ExtraResistance.Key, true, 0.0);

                    var tolerance = 1e-10;
                    var generalStructureWeirFormula = new GeneralStructureWeirFormula
                    {
                        WidthLeftSideOfStructure = category.ReadProperty<double>(StructureRegion.Upstream1Width.Key, true, 10.0),
                        WidthStructureLeftSide = category.ReadProperty<double>(StructureRegion.Upstream2Width.Key, true, 10.0),
                        WidthStructureCentre = category.ReadProperty<double>(StructureRegion.CrestWidth.Key, true, 10.0),
                        WidthStructureRightSide = category.ReadProperty<double>(StructureRegion.Downstream1Width.Key, true, 10.0),
                        WidthRightSideOfStructure = category.ReadProperty<double>(StructureRegion.Downstream2Width.Key, true, 10.0),

                        BedLevelLeftSideOfStructure = category.ReadProperty<double>(StructureRegion.Upstream1Level.Key, true, 0.0),
                        BedLevelLeftSideStructure = category.ReadProperty<double>(StructureRegion.Upstream2Level.Key, true, 0.0),
                        BedLevelStructureCentre = category.ReadProperty<double>(StructureRegion.CrestLevel.Key, true, 0.0),
                        BedLevelRightSideStructure = category.ReadProperty<double>(StructureRegion.Downstream1Level.Key, true, 0.0),
                        BedLevelRightSideOfStructure = category.ReadProperty<double>(StructureRegion.Downstream2Level.Key, true, 0.0),

                        PositiveFreeGateFlow = category.ReadProperty<double>(StructureRegion.PosFreeGateFlowCoeff.Key, true, 1.0),
                        PositiveDrownedGateFlow = category.ReadProperty<double>(StructureRegion.PosDrownGateFlowCoeff.Key, true, 1.0),
                        PositiveFreeWeirFlow = category.ReadProperty<double>(StructureRegion.PosFreeWeirFlowCoeff.Key, true, 1.0),
                        PositiveDrownedWeirFlow = category.ReadProperty<double>(StructureRegion.PosDrownWeirFlowCoeff.Key, true, 1.0),
                        PositiveContractionCoefficient = category.ReadProperty<double>(StructureRegion.PosContrCoefFreeGate.Key, true, 1.0),


                        NegativeFreeGateFlow = category.ReadProperty<double>(StructureRegion.NegFreeGateFlowCoeff.Key, true, 1.0),
                        NegativeDrownedGateFlow = category.ReadProperty<double>(StructureRegion.NegDrownGateFlowCoeff.Key, true, 1.0),
                        NegativeFreeWeirFlow = category.ReadProperty<double>(StructureRegion.NegFreeWeirFlowCoeff.Key, true, 1.0),
                        NegativeDrownedWeirFlow = category.ReadProperty<double>(StructureRegion.NegDrownWeirFlowCoeff.Key, true, 1.0),
                        NegativeContractionCoefficient = category.ReadProperty<double>(StructureRegion.NegContrCoefFreeGate.Key, true, 1.0),

                        ExtraResistance = extraResistance,
                        UseExtraResistance = Math.Abs(extraResistance) > tolerance,

                        GateOpening = category.ReadProperty<double>(StructureRegion.GateHeight.Key, true, 1E10d),
                        CrestLength = category.ReadProperty<double>(StructureRegion.CrestLength.Key, true, 0.0),
                        GateOpeningWidth = category.ReadProperty<double>(StructureRegion.GateOpeningWidth.Key, true, 0.0),

                    };

                    var gateOpeningDirection = category.ReadProperty<string>(StructureRegion.GateHorizontalOpeningDirection.Key, true, "symmetric");
                    switch (gateOpeningDirection.ToLower())
                    {
                        case "symmetric":
                            generalStructureWeirFormula.GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric;
                            break;
                        case "fromleft":
                            generalStructureWeirFormula.GateOpeningHorizontalDirection = GateOpeningDirection.FromLeft;
                            break;
                        case "fromright":
                            generalStructureWeirFormula.GateOpeningHorizontalDirection = GateOpeningDirection.FromRight;
                            break;
                        default:
                            throw new ArgumentException("Could not parse horizontal_opening_direction of type: " + gateOpeningDirection);
                    }

                    if (Math.Abs(generalStructureWeirFormula.GateOpening) < tolerance)
                    {
                        generalStructureWeirFormula.GateOpening = category.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key, true, 11.0) - weir.CrestLevel;
                    }

                    return generalStructureWeirFormula;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Weir type expected.");
            }
        }
    }
}