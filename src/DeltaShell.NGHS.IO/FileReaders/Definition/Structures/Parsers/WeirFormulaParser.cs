using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
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
        /// <param name="iniSection">The <see cref="IniSection"/> to parse the weir formula from.</param>
        /// <param name="weir">The weir to add the parsed weir formula to.</param>
        /// <param name="structuresFilePath">The path to the structures file.</param>
        /// <param name="referenceDateTime">The reference date time.</param>
        /// <param name="FileReader">Optional file reader.</param>
        /// <returns>The parsed weir formula.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the gate opening direction could not be parsed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the type of weir formula is unknown.</exception>
        /// <remarks>
        /// If no file reader is provided a <see cref="TimFile"/> will be created if necessary.
        /// </remarks>
        public static IWeirFormula ReadFormulaFromDefinition(IniSection iniSection, 
                                                             Weir weir, 
                                                             string structuresFilePath, 
                                                             DateTime referenceDateTime,
                                                             ITimeSeriesFileReader fileReader = null)
        {
            Ensure.NotNull(iniSection, nameof(iniSection));
            Ensure.NotNull(weir, nameof(weir));
            Ensure.NotNull(structuresFilePath, nameof(structuresFilePath));
            
            string definitionType = iniSection.ReadProperty<string>(StructureRegion.DefinitionType.Key, true);
            if (!Enum.TryParse(definitionType, true, out StructureType type))
            {
                type = StructureType.Unknown;
            }

            switch (type)
            {
                case StructureType.Weir:
                    return new SimpleWeirFormula { CorrectionCoefficient = iniSection.ReadProperty(StructureRegion.CorrectionCoeff.Key, true, 1.0) };
                case StructureType.UniversalWeir:
                    var readFormulaFromDefinition = new FreeFormWeirFormula
                    {
                        DischargeCoefficient = iniSection.ReadProperty<double>(StructureRegion.DischargeCoeff.Key),
                        CrestLevel = weir.CrestLevel
                    };

                    var yValues = iniSection.ReadProperty<string>(StructureRegion.YValues.Key).ToDoubleArray();
                    var zValues = iniSection.ReadProperty<string>(StructureRegion.ZValues.Key).ToDoubleArray();
                    readFormulaFromDefinition.SetShape(yValues, zValues);

                    return readFormulaFromDefinition;
                case StructureType.RiverWeir:

                    var riverWeirFormula = new RiverWeirFormula
                    {
                        CorrectionCoefficientPos = iniSection.ReadProperty<double>(StructureRegion.PosCwCoef.Key),
                        SubmergeLimitPos = iniSection.ReadProperty<double>(StructureRegion.PosSlimLimit.Key),
                        CorrectionCoefficientNeg = iniSection.ReadProperty<double>(StructureRegion.NegCwCoef.Key),
                        SubmergeLimitNeg = iniSection.ReadProperty<double>(StructureRegion.NegSlimLimit.Key)
                    };

                    var posSf = iniSection.ReadProperty<string>(StructureRegion.PosSf.Key, true).ToDoubleArray();
                    var posRed = iniSection.ReadProperty<string>(StructureRegion.PosRed.Key, true).ToDoubleArray();

                    riverWeirFormula.SubmergeReductionPos =
                        riverWeirFormula.SubmergeReductionPos.CreateFunctionFromArrays(posSf, posRed);

                    var negSf = iniSection.ReadProperty<string>(StructureRegion.NegSf.Key, true).ToDoubleArray();
                    var negRed = iniSection.ReadProperty<string>(StructureRegion.NegRed.Key, true).ToDoubleArray();

                    riverWeirFormula.SubmergeReductionPos =
                        riverWeirFormula.SubmergeReductionNeg.CreateFunctionFromArrays(negSf, negRed);

                    return riverWeirFormula;
                case StructureType.AdvancedWeir:
                    return new PierWeirFormula
                    {
                        NumberOfPiers = iniSection.ReadProperty<int>(StructureRegion.NPiers.Key),
                        UpstreamFacePos = iniSection.ReadProperty<double>(StructureRegion.PosHeight.Key),
                        DesignHeadPos = iniSection.ReadProperty<double>(StructureRegion.PosDesignHead.Key),
                        PierContractionPos = iniSection.ReadProperty<double>(StructureRegion.PosPierContractCoef.Key),
                        AbutmentContractionPos = iniSection.ReadProperty<double>(StructureRegion.PosAbutContractCoef.Key),
                        UpstreamFaceNeg = iniSection.ReadProperty<double>(StructureRegion.NegHeight.Key),
                        DesignHeadNeg = iniSection.ReadProperty<double>(StructureRegion.NegDesignHead.Key),
                        PierContractionNeg = iniSection.ReadProperty<double>(StructureRegion.NegPierContractCoef.Key),
                        AbutmentContractionNeg = iniSection.ReadProperty<double>(StructureRegion.NegAbutContractCoef.Key)
                    };
                case StructureType.Orifice:
                    var gatedWeirFormula = new GatedWeirFormula(true)
                    {
                        ContractionCoefficient = iniSection.ReadProperty<double>(StructureRegion.CorrectionCoeff.Key, true, 1.0),
                        LateralContraction = 1,
                        UseMaxFlowPos = iniSection.ReadProperty<bool>(StructureRegion.UseLimitFlowPos.Key, true),
                        MaxFlowPos = iniSection.ReadProperty<double>(StructureRegion.LimitFlowPos.Key, true),
                        UseMaxFlowNeg = iniSection.ReadProperty<bool>(StructureRegion.UseLimitFlowNeg.Key, true),
                        MaxFlowNeg = iniSection.ReadProperty<double>(StructureRegion.LimitFlowNeg.Key, true),
                    };
                    SetOrificeLowerEdgeLevel(gatedWeirFormula, iniSection, weir, structuresFilePath, referenceDateTime, fileReader);

                    return gatedWeirFormula;
                case StructureType.GeneralStructure:
                    var extraResistance = iniSection.ReadProperty<double>(StructureRegion.ExtraResistance.Key, true, 0.0);

                    var tolerance = 1e-10;
                    var generalStructureWeirFormula = new GeneralStructureWeirFormula
                    {
                        WidthLeftSideOfStructure = iniSection.ReadProperty<double>(StructureRegion.Upstream1Width.Key, true, 10.0),
                        WidthStructureLeftSide = iniSection.ReadProperty<double>(StructureRegion.Upstream2Width.Key, true, 10.0),
                        WidthStructureCentre = iniSection.ReadProperty<double>(StructureRegion.CrestWidth.Key, true, 10.0),
                        WidthStructureRightSide = iniSection.ReadProperty<double>(StructureRegion.Downstream1Width.Key, true, 10.0),
                        WidthRightSideOfStructure = iniSection.ReadProperty<double>(StructureRegion.Downstream2Width.Key, true, 10.0),
                        BedLevelLeftSideOfStructure = iniSection.ReadProperty<double>(StructureRegion.Upstream1Level.Key, true, 0.0),
                        BedLevelLeftSideStructure = iniSection.ReadProperty<double>(StructureRegion.Upstream2Level.Key, true, 0.0),
                        BedLevelStructureCentre = iniSection.ReadProperty<double>(StructureRegion.CrestLevel.Key, true, 0.0),
                        BedLevelRightSideStructure = iniSection.ReadProperty<double>(StructureRegion.Downstream1Level.Key, true, 0.0),
                        BedLevelRightSideOfStructure = iniSection.ReadProperty<double>(StructureRegion.Downstream2Level.Key, true, 0.0),
                        PositiveFreeGateFlow = iniSection.ReadProperty<double>(StructureRegion.PosFreeGateFlowCoeff.Key, true, 1.0),
                        PositiveDrownedGateFlow = iniSection.ReadProperty<double>(StructureRegion.PosDrownGateFlowCoeff.Key, true, 1.0),
                        PositiveFreeWeirFlow = iniSection.ReadProperty<double>(StructureRegion.PosFreeWeirFlowCoeff.Key, true, 1.0),
                        PositiveDrownedWeirFlow = iniSection.ReadProperty<double>(StructureRegion.PosDrownWeirFlowCoeff.Key, true, 1.0),
                        PositiveContractionCoefficient = iniSection.ReadProperty<double>(StructureRegion.PosContrCoefFreeGate.Key, true, 1.0),
                        NegativeFreeGateFlow = iniSection.ReadProperty<double>(StructureRegion.NegFreeGateFlowCoeff.Key, true, 1.0),
                        NegativeDrownedGateFlow = iniSection.ReadProperty<double>(StructureRegion.NegDrownGateFlowCoeff.Key, true, 1.0),
                        NegativeFreeWeirFlow = iniSection.ReadProperty<double>(StructureRegion.NegFreeWeirFlowCoeff.Key, true, 1.0),
                        NegativeDrownedWeirFlow = iniSection.ReadProperty<double>(StructureRegion.NegDrownWeirFlowCoeff.Key, true, 1.0),
                        NegativeContractionCoefficient = iniSection.ReadProperty<double>(StructureRegion.NegContrCoefFreeGate.Key, true, 1.0),
                        ExtraResistance = extraResistance,
                        UseExtraResistance = Math.Abs(extraResistance) > tolerance,
                        GateHeight = iniSection.ReadProperty<double>(StructureRegion.GateHeight.Key, true, 1E10d),
                        CrestLength = iniSection.ReadProperty<double>(StructureRegion.CrestLength.Key, true, 0.0),
                        GateOpeningWidth = iniSection.ReadProperty<double>(StructureRegion.GateOpeningWidth.Key, true, 0.0),
                        LowerEdgeLevel = iniSection.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key, true, 11.0)
                    };

                    var gateOpeningDirection = iniSection.ReadProperty<string>(StructureRegion.GateHorizontalOpeningDirection.Key, true, "symmetric");
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
                                                     IniSection iniSection,
                                                     IWeir weir,
                                                     string structuresFilePath,
                                                     DateTime referenceDateTime,
                                                     ITimeSeriesFileReader fileReader)
        {
            var lowerEdgeLevelValue = iniSection.ReadProperty<string>(StructureRegion.GateLowerEdgeLevel.Key);

            if (lowerEdgeLevelValue != null && fileReader != null && fileReader.IsTimeSeriesProperty(lowerEdgeLevelValue))
            {
                ReadOrificeLowerEdgeLevelTimeSeries(gatedWeirFormula,
                                                    lowerEdgeLevelValue,
                                                    structuresFilePath,
                                                    referenceDateTime, 
                                                    fileReader, 
                                                    weir);
            }
            else
            {
                var gateLowerEdgeLevel = iniSection.ReadProperty<double>(StructureRegion.GateLowerEdgeLevel.Key);
                gatedWeirFormula.LowerEdgeLevel = gateLowerEdgeLevel;
                gatedWeirFormula.GateOpening = gateLowerEdgeLevel - weir.CrestLevel;
            }
        }

        private static void ReadOrificeLowerEdgeLevelTimeSeries(GatedWeirFormula weirFormula, 
                                                                string relativeLowerEdgeLevelPath, 
                                                                string structuresFilePath, 
                                                                DateTime referenceDateTime,
                                                                ITimeSeriesFileReader reader,
                                                                IWeir orifice)
        {
            string filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, relativeLowerEdgeLevelPath);
            weirFormula.UseLowerEdgeLevelTimeSeries = true;

            try
            {
                reader.Read(relativeLowerEdgeLevelPath, filePath, new StructureTimeSeries(orifice, weirFormula.LowerEdgeLevelTimeSeries), referenceDateTime);
            }
            catch (FileReadingException e)
            {
                log.WarnFormat("Could not read the time series at {0} using default Lower Edge Level instead: {1}", filePath, e.Message);
                weirFormula.UseLowerEdgeLevelTimeSeries = false;
            }
        }
    }
}