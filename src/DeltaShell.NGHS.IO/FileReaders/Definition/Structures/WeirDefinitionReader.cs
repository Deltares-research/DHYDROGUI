using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    class WeirDefinitionReader : IStructureDefinitionReader
    {
        /// <inheritdoc/>
        public IStructure1D ReadDefinition(IDelftIniCategory category,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var allowedFlowDirValue = category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key, true);
            var allowedFlowDir = allowedFlowDirValue != null
                ? (FlowDirection) Enum.Parse(typeof(FlowDirection), allowedFlowDirValue, true)
                : 0;

            var weir = new Weir
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                CrestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key, true),
                CrestWidth = category.ReadProperty<double>(StructureRegion.CrestWidth.Key, true),
                FlowDirection = allowedFlowDir,
                Branch = branch,
                Chainage = category.ReadProperty<double>(StructureRegion.Chainage.Key)
            };

            weir.WeirFormula = ReadFormulaFromDefinition(category, weir);

            return weir;
        }

        private static IWeirFormula ReadFormulaFromDefinition(IDelftIniCategory category, Weir weir)
        {
            var type = (StructureType) Enum.Parse(typeof(StructureType),
                category.ReadProperty<string>(StructureRegion.DefinitionType.Key), true);

            switch (type)
            {
                case StructureType.Weir:
                    return new SimpleWeirFormula
                    {
                        DischargeCoefficient = category.ReadProperty<double>(StructureRegion.CorrectionCoeff.Key),
                        LateralContraction = 1
                    };
                case StructureType.UniversalWeir:
                    var readFormulaFromDefinition = new FreeFormWeirFormula
                    {
                        DischargeCoefficient = category.ReadProperty<double>(StructureRegion.DischargeCoeff.Key)
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
                        UpstreamFacePos = category.ReadProperty<int>(StructureRegion.PosHeight.Key),
                        DesignHeadPos = category.ReadProperty<int>(StructureRegion.PosDesignHead.Key),
                        PierContractionPos = category.ReadProperty<int>(StructureRegion.PosPierContractCoef.Key),
                        AbutmentContractionPos = category.ReadProperty<int>(StructureRegion.PosAbutContractCoef.Key),

                        UpstreamFaceNeg = category.ReadProperty<int>(StructureRegion.NegHeight.Key),
                        DesignHeadNeg = category.ReadProperty<int>(StructureRegion.NegDesignHead.Key),
                        PierContractionNeg = category.ReadProperty<int>(StructureRegion.NegPierContractCoef.Key),
                        AbutmentContractionNeg = category.ReadProperty<int>(StructureRegion.NegAbutContractCoef.Key)
                    };
                case StructureType.Orifice:
                    return new GatedWeirFormula
                    {
                        GateOpening = category.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key) - weir.CrestLevel,
                        ContractionCoefficient = category.ReadProperty<double>(StructureRegion.CorrectionCoeff.Key),
                        LateralContraction = 1
                    };
                case StructureType.GeneralStructure:
                    var extraResistance = category.ReadProperty<double>(StructureRegion.ExtraResistance.Key);

                    var tolerance = 1e-10;
                    var generalStructureWeirFormula = new GeneralStructureWeirFormula
                    {
                        WidthLeftSideOfStructure = category.ReadProperty<double>(StructureRegion.Upstream1Width.Key),
                        WidthStructureLeftSide = category.ReadProperty<double>(StructureRegion.Upstream2Width.Key),
                        WidthStructureCentre = category.ReadProperty<double>(StructureRegion.CrestWidth.Key),
                        WidthStructureRightSide = category.ReadProperty<double>(StructureRegion.Downstream1Width.Key),
                        WidthRightSideOfStructure = category.ReadProperty<double>(StructureRegion.Downstream2Width.Key),

                        BedLevelLeftSideOfStructure = category.ReadProperty<double>(StructureRegion.Upstream1Level.Key),
                        BedLevelLeftSideStructure = category.ReadProperty<double>(StructureRegion.Upstream2Level.Key),
                        BedLevelStructureCentre = category.ReadProperty<double>(StructureRegion.CrestLevel.Key),
                        BedLevelRightSideStructure = category.ReadProperty<double>(StructureRegion.Downstream1Level.Key),
                        BedLevelRightSideOfStructure = category.ReadProperty<double>(StructureRegion.Downstream2Level.Key),

                        PositiveFreeGateFlow = category.ReadProperty<double>(StructureRegion.PosFreeGateFlowCoeff.Key),
                        PositiveDrownedGateFlow =
                            category.ReadProperty<double>(StructureRegion.PosDrownGateFlowCoeff.Key),
                        PositiveFreeWeirFlow = category.ReadProperty<double>(StructureRegion.PosFreeWeirFlowCoeff.Key),
                        PositiveDrownedWeirFlow =
                            category.ReadProperty<double>(StructureRegion.PosDrownWeirFlowCoeff.Key),
                        PositiveContractionCoefficient =
                            category.ReadProperty<double>(StructureRegion.PosContrCoefFreeGate.Key),


                        NegativeFreeGateFlow = category.ReadProperty<double>(StructureRegion.NegFreeGateFlowCoeff.Key),
                        NegativeDrownedGateFlow =
                            category.ReadProperty<double>(StructureRegion.NegDrownGateFlowCoeff.Key),
                        NegativeFreeWeirFlow = category.ReadProperty<double>(StructureRegion.NegFreeWeirFlowCoeff.Key),
                        NegativeDrownedWeirFlow =
                            category.ReadProperty<double>(StructureRegion.NegDrownWeirFlowCoeff.Key),
                        NegativeContractionCoefficient =
                            category.ReadProperty<double>(StructureRegion.NegContrCoefFreeGate.Key),

                        ExtraResistance = extraResistance,
                        UseExtraResistance = Math.Abs(extraResistance) > tolerance,

                        GateOpening = category.ReadProperty<double>(StructureRegion.GateHeight.Key, true),
                    };

                    if (Math.Abs(generalStructureWeirFormula.GateOpening) < tolerance)
                    {
                        generalStructureWeirFormula.GateOpening = category.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key) - weir.CrestLevel;
                    }

                    return generalStructureWeirFormula;


                default:
                    throw new ArgumentOutOfRangeException("Weir type expected.");
            }
        }
    }
}