using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    public static class StructureDefinitionParser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StructureDefinitionParser));

        private const string invertedSiphonTypeName = "invertedSiphon";

        public static IStructure1D ReadStructure(this IDelftIniCategory category, IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch, string type)
        {
            if (!Enum.TryParse(type, true, out StructureType structureType))
            {
                if (type == "compound")
                {
                    structureType = StructureType.CompositeBranchStructure;
                }
                else
                {
                    throw new FileReadingException($"Couldn't parse this type '{type}' to an element of the structure type enum");
                }
            }
            
            switch (structureType)
            {
                case StructureType.Bridge:
                    return ReadBridgeDefinition(category,crossSectionDefinitions, branch);
                case StructureType.Culvert:
                    return ReadCulvertDefinition(category, crossSectionDefinitions, branch);
                case StructureType.ExtraResistance:
                    return ReadExtraResistanceDefinition(category, branch);
                case StructureType.Pump:
                    return ReadPumpDefinition(category, branch);
                case StructureType.Weir:
                case StructureType.UniversalWeir:
                case StructureType.GeneralStructure:
                    return ReadWeirDefinition(category, branch);
                case StructureType.Orifice:
                    return ReadOrificeDefinition(category, branch);
                case StructureType.CompositeBranchStructure:
                    return ReadCompositeBranchDefinition(category, branch);
                default:
                    throw new FileReadingException($"No definition reader available for this structure definition: {type}.{Environment.NewLine} This structure is not yet supported in the kernel");
            }
        }

        private static IStructure1D ReadCompositeBranchDefinition(IDelftIniCategory category, IBranch branch)
        {
            return new CompositeBranchStructure
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                Tag = category.ReadProperty<string>(StructureRegion.StructureIds.Key, true) // optional if numStructures == 0
            };
        }

        private static IStructure1D ReadBridgeDefinition(IDelftIniCategory category, IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var crossSectionDefinitionId = category.ReadProperty<string>(StructureRegion.CsDefId.Key, true); // pillar does not need cs def
            var definition = crossSectionDefinitionId == default(string) ? null : crossSectionDefinitions.FirstOrDefault(cd => string.Equals(cd.Name, crossSectionDefinitionId, StringComparison.CurrentCultureIgnoreCase));

            var standardCrossSectionDefinition = definition as CrossSectionDefinitionStandard;

            var name = category.ReadProperty<string>(StructureRegion.Id.Key);
            double shift = 0d;
            if (category.ContainsProperty(StructureRegion.Shift.Key))
            {
                shift = category.ReadProperty<double>(StructureRegion.Shift.Key);
            }
            else
            {
                if (category.ContainsProperty("bedLevel"))
                {
                    log.Warn($"Bridge {name}: \"bedLevel\" is not supported any more. Please provide the proper shift value (which has been set to 0)");
                }
            }

            var tabulatedCrossSectionDefinition = standardCrossSectionDefinition?.Shape?.GetTabulatedDefinition();
            var width = tabulatedCrossSectionDefinition?.Width ?? 50;
            var height = tabulatedCrossSectionDefinition?.ZWDataTable.Max(t => t.Z) - tabulatedCrossSectionDefinition?.ZWDataTable.Min(t => t.Z) ?? 3;
            return new Bridge
            {
                Name = name,
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                BridgeType = definition?.CrossSectionType == CrossSectionType.ZW ? BridgeType.Tabulated : definition?.CrossSectionType == CrossSectionType.YZ ? BridgeType.YzProfile : BridgeType.Rectangle,
                FlowDirection = (FlowDirection)category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key).GetEnumValueFromDisplayName(typeof(FlowDirection)),
                Shift = shift,
                Width = width,
                Height = height,
                TabulatedCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null && definition.CrossSectionType == CrossSectionType.ZW
                                                      ? definition as CrossSectionDefinitionZW
                                                      : tabulatedCrossSectionDefinition
                                                        ?? CrossSectionDefinitionZW.CreateDefault(crossSectionDefinitionId).SetAsRectangle(shift, width, height),
                YZCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null
                                               ? definition.CrossSectionType == CrossSectionType.YZ
                                                     ? definition as CrossSectionDefinitionYZ
                                                     : definition.CrossSectionType == CrossSectionType.ZW
                                                         ? CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).ConvertZWDataTableToYZ(((CrossSectionDefinitionZW)definition).ZWDataTable)
                                                         : CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).SetAsRectangle(shift, width, height)
                                               : CrossSectionDefinitionYZ.CreateDefault(crossSectionDefinitionId).SetAsRectangle(shift, width, height),
                Length = category.ReadProperty<double>(StructureRegion.Length.Key, true),
                InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key, true),
                OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key, true),
                PillarWidth = category.ReadProperty<double>(StructureRegion.PillarWidth.Key, true),
                ShapeFactor = category.ReadProperty<double>(StructureRegion.FormFactor.Key, true),
                FrictionDataType = (Friction)Enum.Parse(typeof(Friction), category.ReadProperty<string>(StructureRegion.FrictionType.Key), true),
                Friction = category.ReadProperty<double>(StructureRegion.Friction.Key)
            };
        }

        private static IStructure1D ReadExtraResistanceDefinition(IDelftIniCategory category, IBranch branch)
        {
            var extraResistance = new ExtraResistance
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
            };

            var levels = category.ReadProperty<string>(StructureRegion.Levels.Key).ToDoubleArray();
            var ksi = category.ReadProperty<string>(StructureRegion.Ksi.Key).ToDoubleArray();

            extraResistance.FrictionTable = extraResistance.FrictionTable.CreateFunctionFromArrays(levels, ksi);

            return extraResistance;
        }

        private static IStructure1D ReadPumpDefinition(IDelftIniCategory category, IBranch branch)
        {
            var pump = new Pump
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                DirectionIsPositive = category.ReadProperty<string>(StructureRegion.Orientation.Key, true, "positive")?.ToLower() == "positive",
                ControlDirection = GetControlDirectionFromString(category.ReadProperty<string>(StructureRegion.Direction.Key)),
                Capacity = category.ReadProperty<double>(StructureRegion.Capacity.Key),
            };

            pump.StartSuction = category.ReadProperty<double>(StructureRegion.StartLevelSuctionSide.Key, pump.ControlDirection == PumpControlDirection.DeliverySideControl);
            pump.StopSuction = category.ReadProperty<double>(StructureRegion.StopLevelSuctionSide.Key, pump.ControlDirection == PumpControlDirection.DeliverySideControl);
            pump.StartDelivery = category.ReadProperty<double>(StructureRegion.StartLevelDeliverySide.Key, pump.ControlDirection == PumpControlDirection.SuctionSideControl);
            pump.StopDelivery = category.ReadProperty<double>(StructureRegion.StopLevelDeliverySide.Key, pump.ControlDirection == PumpControlDirection.SuctionSideControl);


            var numReductionLevels = category.ReadProperty<int>(StructureRegion.ReductionFactorLevels.Key, true, 0);
            if (numReductionLevels > 0)
            {
                var headValues = category.ReadProperty<string>(StructureRegion.Head.Key, true).ToDoubleArray();
                var reductionFactorValues =
                    category.ReadProperty<string>(StructureRegion.ReductionFactor.Key, true).ToDoubleArray();

                pump.ReductionTable = pump.ReductionTable.CreateFunctionFromArrays(headValues, reductionFactorValues);
            }

            return pump;
        }

        private static PumpControlDirection GetControlDirectionFromString(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "suctionside": return PumpControlDirection.SuctionSideControl;
                case "deliveryside": return PumpControlDirection.DeliverySideControl;
                case "both": return PumpControlDirection.SuctionAndDeliverySideControl;
                default: return 0;
            }
        }

        private static IStructure1D ReadOrificeDefinition(IDelftIniCategory category, IBranch branch)
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

        private static IStructure1D ReadWeirDefinition(IDelftIniCategory category, IBranch branch)
        {
            var allowedFlowDirValue = category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key, true);
            var allowedFlowDir = allowedFlowDirValue != null
                ? (FlowDirection)Enum.Parse(typeof(FlowDirection), allowedFlowDirValue, true)
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

        private static IWeirFormula ReadFormulaFromDefinition(IDelftIniCategory category, Weir weir)
        {
            var type = (StructureType)Enum.Parse(typeof(StructureType),
                category.ReadProperty<string>(StructureRegion.DefinitionType.Key), true);

            switch (type)
            {
                case StructureType.Weir:
                    return new SimpleWeirFormula
                    {
                        CorrectionCoefficient = category.ReadProperty(StructureRegion.CorrectionCoeff.Key, true, 1.0)
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
                        LateralContraction = 1,
                        UseMaxFlowPos = category.ReadProperty<bool>(StructureRegion.UseLimitFlowPos.Key, true),
                        MaxFlowPos = category.ReadProperty<double>(StructureRegion.LimitFlowPos.Key, true),
                        UseMaxFlowNeg = category.ReadProperty<bool>(StructureRegion.UseLimitFlowNeg.Key, true),
                        MaxFlowNeg = category.ReadProperty<double>(StructureRegion.LimitFlowNeg.Key, true),
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
                            throw new ArgumentException("Could not parse horizontal_opening_direction of type: " + gateOpeningDirection);
                    }

                    return generalStructureWeirFormula;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Weir type expected.");
            }
        }

        private static IStructure1D ReadCulvertDefinition(IDelftIniCategory category, IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var crossSectionDefinitionId = category.ReadProperty<string>(StructureRegion.CsDefId.Key);
            var definition = crossSectionDefinitions.FirstOrDefault(cd => string.Equals(cd.Name, crossSectionDefinitionId, StringComparison.InvariantCultureIgnoreCase));

            var standardCrossSectionDefinition = definition as CrossSectionDefinitionStandard;

            var culvert = new Culvert
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                GeometryType = GetGeometryType(standardCrossSectionDefinition?.ShapeType),
                TabulatedCrossSectionDefinition = standardCrossSectionDefinition == null && definition != null && definition.CrossSectionType == CrossSectionType.ZW
                    ? definition as CrossSectionDefinitionZW
                    : standardCrossSectionDefinition?.Shape?.GetTabulatedDefinition() ?? CrossSectionDefinitionZW.CreateDefault(),
                FlowDirection = (FlowDirection)category.ReadProperty<string>(StructureRegion.AllowedFlowDir.Key).GetEnumValueFromDisplayName(typeof(FlowDirection)),
                InletLevel = category.ReadProperty<double>(StructureRegion.LeftLevel.Key),
                OutletLevel = category.ReadProperty<double>(StructureRegion.RightLevel.Key),
                Length = category.ReadProperty<double>(StructureRegion.Length.Key),
                InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key),
                OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key),
                IsGated = category.ReadProperty<string>(StructureRegion.ValveOnOff.Key) != "0",
                BendLossCoefficient = category.ReadProperty<double>(StructureRegion.BendLossCoef.Key, true),
                FrictionDataType = (Friction)Enum.Parse(typeof(Friction), category.ReadProperty<string>(StructureRegion.BedFrictionType.Key), true),
                Friction = category.ReadProperty<double>(StructureRegion.BedFriction.Key)
            };
            culvert.GateInitialOpening = category.ReadProperty<double>(StructureRegion.IniValveOpen.Key, !culvert.IsGated);

            SetCulvertDimensionsBasedOnProfile(culvert, definition);
            var numLossCoeff = category.ReadProperty<int>(StructureRegion.LossCoeffCount.Key, true);
            if (numLossCoeff > 0)
            {
                var relOpening = category.ReadProperty<string>(StructureRegion.RelativeOpening.Key).ToDoubleArray();
                var lossCoeff = category.ReadProperty<string>(StructureRegion.LossCoefficient.Key).ToDoubleArray();

                culvert.GateOpeningLossCoefficientFunction =
                    culvert.GateOpeningLossCoefficientFunction.CreateFunctionFromArrays(relOpening, lossCoeff);
            }

            culvert.CulvertType = string.Equals(category.GetProperty(StructureRegion.SubType.Key)?.Value, invertedSiphonTypeName, StringComparison.InvariantCultureIgnoreCase)
                                      ? CulvertType.InvertedSiphon
                                      : CulvertType.Culvert;

            return culvert;
        }

        private static void SetCulvertDimensionsBasedOnProfile(ICulvert culvert, ICrossSectionDefinition definition)
        {
            switch (culvert.GeometryType)
            {
                case CulvertGeometryType.Round:
                    {
                        var stdDef = definition as CrossSectionDefinitionStandard;
                        if (stdDef != null)
                        {
                            var round = stdDef.Shape as CrossSectionStandardShapeCircle;
                            if (round != null)
                                culvert.Diameter = round.Diameter;
                        }

                        break;
                    }
                case CulvertGeometryType.Rectangle:
                    {
                        var stdDef = definition as CrossSectionDefinitionStandard;
                        if (stdDef != null)
                        {
                            var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
                            if (heightbase != null)
                            {
                                culvert.Width = heightbase.Width;
                                culvert.Height = heightbase.Height;
                                culvert.Closed = (heightbase as ICrossSectionStandardShapeOpenClosed)?.Closed ?? false;
                            }
                        }
                    }
                    break;
                case CulvertGeometryType.Egg:
                case CulvertGeometryType.InvertedEgg:
                case CulvertGeometryType.Cunette:
                case CulvertGeometryType.Ellipse:
                    {
                        var stdDef = definition as CrossSectionDefinitionStandard;
                        if (stdDef != null)
                        {
                            var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
                            if (heightbase != null)
                            {
                                culvert.Width = heightbase.Width;
                                culvert.Height = heightbase.Height;
                            }
                        }
                    }
                    break;
                case CulvertGeometryType.Arch:
                case CulvertGeometryType.UShape:
                    {
                        var stdDef = definition as CrossSectionDefinitionStandard;
                        if (stdDef != null)
                        {
                            var arch = stdDef.Shape as CrossSectionStandardShapeArch;
                            if (arch != null)
                            {
                                culvert.Width = arch.Width;
                                culvert.Height = arch.Height;
                                culvert.ArcHeight = arch.ArcHeight;
                            }
                        }
                    }
                    break;
                case CulvertGeometryType.SteelCunette:
                    {
                        var stdDef = definition as CrossSectionDefinitionStandard;
                        if (stdDef != null)
                        {
                            var steelcunette = stdDef.Shape as CrossSectionStandardShapeSteelCunette;
                            if (steelcunette != null)
                            {
                                culvert.Angle = steelcunette.AngleA;
                                culvert.Angle1 = steelcunette.AngleA1;
                                culvert.Height = steelcunette.Height;
                                culvert.Radius = steelcunette.RadiusR;
                                culvert.Radius1 = steelcunette.RadiusR1;
                                culvert.Radius2 = steelcunette.RadiusR2;
                                culvert.Radius3 = steelcunette.RadiusR3;
                            }
                        }
                    }
                    break;
                case CulvertGeometryType.Tabulated:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static CulvertGeometryType GetGeometryType(CrossSectionStandardShapeType? standardCrossSectionDefinition)
        {
            switch (standardCrossSectionDefinition)
            {
                case CrossSectionStandardShapeType.Rectangle:
                    return CulvertGeometryType.Rectangle;

                case CrossSectionStandardShapeType.Arch:
                    return CulvertGeometryType.Arch;

                case CrossSectionStandardShapeType.Cunette:
                    return CulvertGeometryType.Cunette;

                case CrossSectionStandardShapeType.Elliptical:
                    return CulvertGeometryType.Ellipse;

                case CrossSectionStandardShapeType.SteelCunette:
                    return CulvertGeometryType.SteelCunette;

                case CrossSectionStandardShapeType.Egg:
                    return CulvertGeometryType.Egg;

                case CrossSectionStandardShapeType.Circle:
                    return CulvertGeometryType.Round;

                case CrossSectionStandardShapeType.InvertedEgg:
                    return CulvertGeometryType.InvertedEgg;

                case CrossSectionStandardShapeType.UShape:
                    return CulvertGeometryType.UShape;
                case null:
                    return CulvertGeometryType.Tabulated;
                default:
                    throw new ArgumentOutOfRangeException(nameof(standardCrossSectionDefinition), standardCrossSectionDefinition, null);
            }
        }
    }
}