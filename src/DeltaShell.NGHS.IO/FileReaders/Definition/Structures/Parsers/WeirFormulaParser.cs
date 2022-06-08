using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    
    /// <summary>
    /// Parser for weir formulas.
    /// </summary>
    public static class WeirFormulaParser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WeirFormulaParser));

        /// <summary>
        /// Parses a weir formula.
        /// </summary>
        /// <param name="category">The <see cref="IDelftIniCategory"/> to parse the weir formula from.</param>
        /// <param name="weir">The weir to add the parsed weir formula to.</param>
        /// <param name="structuresFilePath">The path to the structures file.</param>
        /// <param name="referenceDateTime">The reference date time.</param>
        /// <param name="timFileReader">Optional tim file reader.</param>
        /// <returns>The parsed weir formula.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the gate opening direction could not be parsed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the type of weir formula is unknown.</exception>
        /// <remarks>
        /// If no tim file reader is provided a <see cref="TimFile"/> will be created if necessary.
        /// </remarks>
        public static IWeirFormula ReadFormulaFromDefinition(IDelftIniCategory category, 
                                                             Weir weir, 
                                                             string structuresFilePath, 
                                                             DateTime referenceDateTime,
                                                             ITimFileReader timFileReader = null)
        {
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(weir, nameof(weir));
            Ensure.NotNull(structuresFilePath, nameof(structuresFilePath));
            
            string definitionType = category.ReadProperty<string>(StructureRegion.DefinitionType.Key, true);
            if (!Enum.TryParse(definitionType, true, out StructureType type))
            {
                type = StructureType.Unknown;
            }

            switch (type)
            {
                case StructureType.Weir:
                    return new SimpleWeirFormula { CorrectionCoefficient = category.ReadProperty(StructureRegion.CorrectionCoeff.Key, true, 1.0) };
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
                    var gatedWeirFormula = new GatedWeirFormula(true)
                    {
                        ContractionCoefficient = category.ReadProperty<double>(StructureRegion.CorrectionCoeff.Key, true, 1.0),
                        LateralContraction = 1,
                        UseMaxFlowPos = category.ReadProperty<bool>(StructureRegion.UseLimitFlowPos.Key, true),
                        MaxFlowPos = category.ReadProperty<double>(StructureRegion.LimitFlowPos.Key, true),
                        UseMaxFlowNeg = category.ReadProperty<bool>(StructureRegion.UseLimitFlowNeg.Key, true),
                        MaxFlowNeg = category.ReadProperty<double>(StructureRegion.LimitFlowNeg.Key, true),
                    };
                    SetOrificeLowerEdgeLevel(gatedWeirFormula, category, weir, structuresFilePath, referenceDateTime, timFileReader);

                    return gatedWeirFormula;
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
                        GateHeight = category.ReadProperty<double>(StructureRegion.GateHeight.Key, true, 1E10d),
                        CrestLength = category.ReadProperty<double>(StructureRegion.CrestLength.Key, true, 0.0),
                        GateOpeningWidth = category.ReadProperty<double>(StructureRegion.GateOpeningWidth.Key, true, 0.0),
                        LowerEdgeLevel = category.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key, true, 11.0)
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
                            throw new ArgumentException(string.Format(Resources.WeirFormulaParser_Could_not_parse_horizontal_gate_opening, gateOpeningDirection));
                    }

                    return generalStructureWeirFormula;

                default:
                    throw new InvalidOperationException(string.Format(Resources.WeirFormulaParser_Unknow_formula_type, definitionType));
            }
        }

        private static void SetOrificeLowerEdgeLevel(GatedWeirFormula gatedWeirFormula, 
                                                     IDelftIniCategory category,
                                                     IWeir weir,
                                                     string structuresFilePath,
                                                     DateTime referenceDateTime,
                                                     ITimFileReader timFileReader)
        {
            var lowerEdgeLevelValue = category.ReadProperty<string>(StructureRegion.GateLowerEdgeLevel.Key);

            if (lowerEdgeLevelValue != null && lowerEdgeLevelValue.EndsWith(FileSuffices.TimFile))
            {
                ReadOrificeLowerEdgeLevelTimeSeries(gatedWeirFormula,
                                                    lowerEdgeLevelValue,
                                                    structuresFilePath,
                                                    referenceDateTime, 
                                                    timFileReader);
            }
            else
            {
                var gateLowerEdgeLevel = category.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key);
                gatedWeirFormula.GateOpening = gateLowerEdgeLevel - weir.CrestLevel;
            }
        }

        private static void ReadOrificeLowerEdgeLevelTimeSeries(GatedWeirFormula weirFormula, 
                                                                string relativeLowerEdgeLevelPath, 
                                                                string structuresFilePath, 
                                                                DateTime referenceDateTime,
                                                                ITimFileReader reader)
        {
            string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, relativeLowerEdgeLevelPath);
            weirFormula.UseLowerEdgeLevelTimeSeries = true;

            reader = reader ?? new TimFile();

            try
            {
                reader.Read(filePath, weirFormula.LowerEdgeLevelTimeSeries, referenceDateTime);
            }
            catch (FileReadingException e)
            {
                log.WarnFormat("Could not read the time series at {0} using default Lower Edge Level instead: {1}", filePath, e.Message);
                weirFormula.UseLowerEdgeLevelTimeSeries = false;
            }
        }
    }
}