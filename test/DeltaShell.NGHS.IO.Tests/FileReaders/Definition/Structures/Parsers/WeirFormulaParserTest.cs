using System;
using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class WeirFormulaParserTest
    {
        private const string structuresFilePath = "structures.ini";
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> ReadFormulaFromDefinitionArgumentNullData()
        {
            var iniSection = new IniSection("some_section");
            var weir = new Weir();

            yield return new TestCaseData(null, weir, structuresFilePath, "iniSection");
            yield return new TestCaseData(iniSection, null, structuresFilePath, "weir");
            yield return new TestCaseData(iniSection, weir, null, "structuresFilePath");
        }

        [Test]
        [TestCaseSource(nameof(ReadFormulaFromDefinitionArgumentNullData))]
        public void ReadFormulaFromDefinition_ArgumentNull_ThrowsArgumentNullException(IniSection iniSection,
                                                                                       Weir weir,
                                                                                       string localStructuresFilePath,
                                                                                       string expectedParamName)
        {
            void Call() => WeirFormulaParser.ReadFormulaFromDefinition(iniSection, weir, localStructuresFilePath, referenceDateTime);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [Test]
        public void ReadFormulaFromDefinition_UnknownWeirFormulaType_ThrowsInvalidOperationException()
        {
            // Setup
            const string unknownFormulaType = "UnknownFormulaType";

            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, unknownFormulaType);

            var weir = new Weir();

            // Call
            void Call() => WeirFormulaParser.ReadFormulaFromDefinition(iniSection, 
                                                                       weir, 
                                                                       structuresFilePath, 
                                                                       referenceDateTime);

            // Assert
            string expectedMessage = string.Format(Resources.WeirFormulaParser_Unknow_formula_type, unknownFormulaType);
            Assert.That(Call, Throws.Exception.TypeOf<InvalidOperationException>()
                                    .With.Message.EqualTo(expectedMessage));
        }
        
        [Test]
        public void ReadFormulaFromDefinition_SimpleWeirFormulaType_ParsesWeirFormulaCorrectly()
        {
            // Setup
            const string formulaType = "weir";
            const double correctionCoefficient = 123.456;
            
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            iniSection.AddProperty(StructureRegion.CorrectionCoeff.Key, correctionCoefficient);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(iniSection, weir, structuresFilePath, referenceDateTime);

            // Assert
            Assert.That(parsedWeirFormula, Is.TypeOf<SimpleWeirFormula>());

            var simpleWeirFormula = (SimpleWeirFormula)parsedWeirFormula;
            Assert.That(simpleWeirFormula.CorrectionCoefficient, Is.EqualTo(correctionCoefficient));
            
        }
        
        [Test]
        public void ReadFormulaFromDefinition_UniversalWeirFormulaType_ParsesWeirFormulaCorrectly()
        {
            // Setup
            const string formulaType = "UniversalWeir";
            const double dischargeCoefficient = 123.456;
            const double crestLevel = 456.789;
            double[] yValues = {1.0, 2.0, 3.0};
            double[] zValues = {4.0, 5.0, 6.0};

            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            iniSection.AddProperty(StructureRegion.DischargeCoeff.Key, dischargeCoefficient);
            iniSection.AddProperty(StructureRegion.YValues.Key, string.Join(", ", yValues));
            iniSection.AddProperty(StructureRegion.ZValues.Key, string.Join(", ", zValues));

            var weir = new Weir() { CrestLevel = crestLevel };

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(iniSection, 
                                                                                         weir, 
                                                                                         structuresFilePath, 
                                                                                         referenceDateTime);

            // Assert
            Assert.That(parsedWeirFormula, Is.TypeOf<FreeFormWeirFormula>());
            var freeFormWeirFormula = (FreeFormWeirFormula) parsedWeirFormula;
            Assert.That(freeFormWeirFormula.DischargeCoefficient, Is.EqualTo(dischargeCoefficient));
            Assert.That(freeFormWeirFormula.CrestLevel, Is.EqualTo(crestLevel));

            Assert.That(freeFormWeirFormula.Y, Is.EqualTo(yValues));
            Assert.That(freeFormWeirFormula.Z, Is.EqualTo(zValues));
        }

        [Test]
        public void ReadFormulaFromDefinition_PierWeirFormulaType_ParsesWeirFormulaCorrectly()
        {
            // Setup
            const string formulaType = "AdvancedWeir";
            const int numberOfPiers = 123;
            const double upstreamFacePos = 12.34;
            const double designHeadPos = 23.45;
            const double pierContractionPos = 34.56;
            const double abutmentContractionPos = 45.67;
            const double upstreamFaceNeg = 56.78;
            const double designHeadNeg = 67.89;
            const double pierContractionNeg = 78.90;
            const double abutmentContractionNeg = 89.01;

            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            iniSection.AddProperty(StructureRegion.NPiers.Key, numberOfPiers);
            iniSection.AddProperty(StructureRegion.PosHeight.Key, upstreamFacePos);
            iniSection.AddProperty(StructureRegion.PosDesignHead.Key, designHeadPos);
            iniSection.AddProperty(StructureRegion.PosPierContractCoef.Key, pierContractionPos);
            iniSection.AddProperty(StructureRegion.PosAbutContractCoef.Key, abutmentContractionPos);
            iniSection.AddProperty(StructureRegion.NegHeight.Key, upstreamFaceNeg);
            iniSection.AddProperty(StructureRegion.NegDesignHead.Key, designHeadNeg);
            iniSection.AddProperty(StructureRegion.NegPierContractCoef.Key, pierContractionNeg);
            iniSection.AddProperty(StructureRegion.NegAbutContractCoef.Key, abutmentContractionNeg);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(iniSection, 
                                                                                         weir,
                                                                                         structuresFilePath,
                                                                                         referenceDateTime);

            // Assert
            Assert.That(parsedWeirFormula, Is.TypeOf<PierWeirFormula>());
            
            var pierWeirFormula = (PierWeirFormula) parsedWeirFormula;
            Assert.That(pierWeirFormula.NumberOfPiers, Is.EqualTo(numberOfPiers));
            Assert.That(pierWeirFormula.UpstreamFacePos, Is.EqualTo(upstreamFacePos));
            Assert.That(pierWeirFormula.DesignHeadPos, Is.EqualTo(designHeadPos));
            Assert.That(pierWeirFormula.PierContractionPos, Is.EqualTo(pierContractionPos));
            Assert.That(pierWeirFormula.AbutmentContractionPos, Is.EqualTo(abutmentContractionPos));
            Assert.That(pierWeirFormula.UpstreamFaceNeg, Is.EqualTo(upstreamFaceNeg));
            Assert.That(pierWeirFormula.DesignHeadNeg, Is.EqualTo(designHeadNeg));
            Assert.That(pierWeirFormula.PierContractionNeg, Is.EqualTo(pierContractionNeg));
            Assert.That(pierWeirFormula.AbutmentContractionNeg, Is.EqualTo(abutmentContractionNeg));
        }

        [Test]
        public void ReadFormulaFromDefinition_OrificeWeirFormulaType_ParsesWeirFormulaCorrectly()
        {
            // Setup
            const string formulaType = "Orifice";
            const double lateralContraction = 1.0;
            const double contractionCoefficient = 23.45;
            const bool useMaxFlowPos = true;
            const double maxFlowPos = 34.56;
            const bool useMaxFlowNeg = false;
            const double maxFlowNeg = 45.67;
            const double crestLevel = 1.234;
            const double lowerEdgeLevel = 5.678;
            double gateOpening = lowerEdgeLevel - crestLevel;
            
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            iniSection.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, lowerEdgeLevel);

            iniSection.AddProperty(StructureRegion.CorrectionCoeff.Key, contractionCoefficient);
            iniSection.AddProperty(StructureRegion.UseLimitFlowPos.Key, useMaxFlowPos.ToString());
            iniSection.AddProperty(StructureRegion.LimitFlowPos.Key, maxFlowPos);
            iniSection.AddProperty(StructureRegion.UseLimitFlowNeg.Key, useMaxFlowNeg.ToString());
            iniSection.AddProperty(StructureRegion.LimitFlowNeg.Key, maxFlowNeg);

            var weir = new Weir() { CrestLevel = crestLevel };

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(iniSection, 
                                                                                         weir, 
                                                                                         structuresFilePath, 
                                                                                         referenceDateTime);

            // Assert
            Assert.That(parsedWeirFormula, Is.TypeOf<GatedWeirFormula>());

            var gatedWeirFormula = (GatedWeirFormula) parsedWeirFormula;
            Assert.That(gatedWeirFormula.GateOpening, Is.EqualTo(gateOpening));
            Assert.That(gatedWeirFormula.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
            Assert.That(gatedWeirFormula.ContractionCoefficient, Is.EqualTo(contractionCoefficient));
            Assert.That(gatedWeirFormula.UseMaxFlowPos, Is.EqualTo(useMaxFlowPos));
            Assert.That(gatedWeirFormula.MaxFlowPos, Is.EqualTo(maxFlowPos));
            Assert.That(gatedWeirFormula.UseMaxFlowNeg, Is.EqualTo(useMaxFlowNeg));
            Assert.That(gatedWeirFormula.MaxFlowNeg, Is.EqualTo(maxFlowNeg));
            Assert.That(gatedWeirFormula.LateralContraction, Is.EqualTo(lateralContraction));
        }
        
        [Test]
        public void ReadFormulaFromDefinition_GeneralStructureWeirFormulaType_ParsesWeirFormulaCorrectly()
        {
            // Setup
            const string formulaType = "GeneralStructure";
            const double widthLeftSideOfStructure = 1.1;
            const double widthStructureLeftSide = 2.2;
            const double widthStructureCentre = 3.3;
            const double widthStructureRightSide = 4.4;
            const double widthRightSideOfStructure = 5.5;
            const double bedLevelLeftSideOfStructure = 6.6;
            const double bedLevelLeftSideStructure = 24.24;
            const double bedLevelStructureCentre = 7.7;
            const double bedLevelRightSideStructure = 8.8;
            const double bedLevelRightSideOfStructure = 9.9;
            const double positiveFreeGateFlow = 10.10;
            const double positiveDrownedGateFlow = 11.11;
            const double positiveFreeWeirFlow = 12.12;
            const double positiveDrownedWeirFlow = 13.13;
            const double positiveContractionCoefficient = 14.14;
            const double negativeFreeGateFlow = 15.15;
            const double negativeDrownedGateFlow = 16.16;
            const double negativeFreeWeirFlow = 17.17;
            const double negativeDrownedWeirFlow = 18.18;
            const double negativeContractionCoefficient = 19.19;
            const double gateHeight = 20.20;
            const double crestLength = 21.21;
            const double gateOpeningWidth = 22.22;
            const double lowerEdgeLevel = 23.23;
            const double extraResistance = 25.25;
            
            const double tolerance = 1e-10;
            bool useExtraResistance = Math.Abs(extraResistance) > tolerance;
            
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            iniSection.AddProperty(StructureRegion.Upstream1Width.Key, widthLeftSideOfStructure);
            iniSection.AddProperty(StructureRegion.Upstream2Width.Key, widthStructureLeftSide);
            iniSection.AddProperty(StructureRegion.CrestWidth.Key, widthStructureCentre);
            iniSection.AddProperty(StructureRegion.Downstream1Width.Key, widthStructureRightSide);
            iniSection.AddProperty(StructureRegion.Downstream2Width.Key, widthRightSideOfStructure);
            iniSection.AddProperty(StructureRegion.Upstream1Level.Key, bedLevelLeftSideOfStructure);
            iniSection.AddProperty(StructureRegion.Upstream2Level.Key, bedLevelLeftSideStructure);
            iniSection.AddProperty(StructureRegion.CrestLevel.Key, bedLevelStructureCentre);
            iniSection.AddProperty(StructureRegion.Downstream1Level.Key, bedLevelRightSideStructure);
            iniSection.AddProperty(StructureRegion.Downstream2Level.Key, bedLevelRightSideOfStructure);
            iniSection.AddProperty(StructureRegion.PosFreeGateFlowCoeff.Key, positiveFreeGateFlow);
            iniSection.AddProperty(StructureRegion.PosDrownGateFlowCoeff.Key, positiveDrownedGateFlow);
            iniSection.AddProperty(StructureRegion.PosFreeWeirFlowCoeff.Key, positiveFreeWeirFlow);
            iniSection.AddProperty(StructureRegion.PosDrownWeirFlowCoeff.Key, positiveDrownedWeirFlow);
            iniSection.AddProperty(StructureRegion.PosContrCoefFreeGate.Key, positiveContractionCoefficient);
            iniSection.AddProperty(StructureRegion.NegFreeGateFlowCoeff.Key, negativeFreeGateFlow);
            iniSection.AddProperty(StructureRegion.NegDrownGateFlowCoeff.Key, negativeDrownedGateFlow);
            iniSection.AddProperty(StructureRegion.NegFreeWeirFlowCoeff.Key, negativeFreeWeirFlow);
            iniSection.AddProperty(StructureRegion.NegDrownWeirFlowCoeff.Key, negativeDrownedWeirFlow);
            iniSection.AddProperty(StructureRegion.NegContrCoefFreeGate.Key, negativeContractionCoefficient);
            iniSection.AddProperty(StructureRegion.GateHeight.Key, gateHeight);
            iniSection.AddProperty(StructureRegion.CrestLength.Key, crestLength);
            iniSection.AddProperty(StructureRegion.GateOpeningWidth.Key, gateOpeningWidth);
            iniSection.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, lowerEdgeLevel);
            iniSection.AddProperty(StructureRegion.ExtraResistance.Key, extraResistance);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(iniSection, 
                                                                                         weir, 
                                                                                         structuresFilePath, 
                                                                                         referenceDateTime);

            // Assert
            Assert.That(parsedWeirFormula, Is.TypeOf<GeneralStructureWeirFormula>());
            
            var generalStructureWeirFormula = (GeneralStructureWeirFormula) parsedWeirFormula;
            Assert.That(generalStructureWeirFormula.WidthLeftSideOfStructure, Is.EqualTo(widthLeftSideOfStructure));
            Assert.That(generalStructureWeirFormula.WidthStructureLeftSide, Is.EqualTo(widthStructureLeftSide));
            Assert.That(generalStructureWeirFormula.WidthStructureCentre, Is.EqualTo(widthStructureCentre));
            Assert.That(generalStructureWeirFormula.WidthStructureRightSide, Is.EqualTo(widthStructureRightSide));
            Assert.That(generalStructureWeirFormula.WidthRightSideOfStructure, Is.EqualTo(widthRightSideOfStructure));
            Assert.That(generalStructureWeirFormula.BedLevelLeftSideOfStructure, Is.EqualTo(bedLevelLeftSideOfStructure));
            Assert.That(generalStructureWeirFormula.BedLevelStructureCentre, Is.EqualTo(bedLevelStructureCentre));
            Assert.That(generalStructureWeirFormula.BedLevelLeftSideStructure, Is.EqualTo(bedLevelLeftSideStructure));
            Assert.That(generalStructureWeirFormula.BedLevelRightSideStructure, Is.EqualTo(bedLevelRightSideStructure));
            Assert.That(generalStructureWeirFormula.BedLevelRightSideOfStructure, Is.EqualTo(bedLevelRightSideOfStructure));
            Assert.That(generalStructureWeirFormula.PositiveFreeGateFlow, Is.EqualTo(positiveFreeGateFlow));
            Assert.That(generalStructureWeirFormula.PositiveDrownedGateFlow, Is.EqualTo(positiveDrownedGateFlow));
            Assert.That(generalStructureWeirFormula.PositiveFreeWeirFlow, Is.EqualTo(positiveFreeWeirFlow));
            Assert.That(generalStructureWeirFormula.PositiveDrownedWeirFlow, Is.EqualTo(positiveDrownedWeirFlow));
            Assert.That(generalStructureWeirFormula.PositiveContractionCoefficient, Is.EqualTo(positiveContractionCoefficient));
            Assert.That(generalStructureWeirFormula.NegativeFreeGateFlow, Is.EqualTo(negativeFreeGateFlow));
            Assert.That(generalStructureWeirFormula.NegativeDrownedGateFlow, Is.EqualTo(negativeDrownedGateFlow));
            Assert.That(generalStructureWeirFormula.NegativeFreeWeirFlow, Is.EqualTo(negativeFreeWeirFlow));
            Assert.That(generalStructureWeirFormula.NegativeDrownedWeirFlow, Is.EqualTo(negativeDrownedWeirFlow));
            Assert.That(generalStructureWeirFormula.NegativeContractionCoefficient, Is.EqualTo(negativeContractionCoefficient));
            Assert.That(generalStructureWeirFormula.GateHeight, Is.EqualTo(gateHeight));
            Assert.That(generalStructureWeirFormula.CrestLength, Is.EqualTo(crestLength));
            Assert.That(generalStructureWeirFormula.GateOpeningWidth, Is.EqualTo(gateOpeningWidth));
            Assert.That(generalStructureWeirFormula.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
            Assert.That(generalStructureWeirFormula.ExtraResistance, Is.EqualTo(extraResistance));
            Assert.That(generalStructureWeirFormula.UseExtraResistance, Is.EqualTo(useExtraResistance));
        }

        [Test]
        [TestCase("symmetric", GateOpeningDirection.Symmetric)]
        [TestCase("FromLeft", GateOpeningDirection.FromLeft)]
        [TestCase("fRoMrIgHT", GateOpeningDirection.FromRight)]
        public void ReadFormulaFromDefinition_GeneralStructureWeirFormula_ParsesGateOpeningDirectionCorrectly(
            string gateOpeningDirection, GateOpeningDirection expectedGateOpeningDirection)
        {
            // Setup
            const string formulaType = "GeneralStructure";
            
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            iniSection.AddProperty(StructureRegion.GateHorizontalOpeningDirection.Key, gateOpeningDirection);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(iniSection, 
                                                                                         weir, 
                                                                                         structuresFilePath, 
                                                                                         referenceDateTime);

            // Assert
            Assert.That(parsedWeirFormula, Is.TypeOf<GeneralStructureWeirFormula>());
            
            var generalStructureWeirFormula = (GeneralStructureWeirFormula) parsedWeirFormula;
            Assert.That(generalStructureWeirFormula.GateOpeningHorizontalDirection, Is.EqualTo(expectedGateOpeningDirection));
        }

        [Test]
        public void ReadFormulaFromDefinition_GeneralStructureWeirFormula_UnknownOpeningDirectionThrowsArgumentOutOfRangeException()
        {
            // Setup
            const string formulaType = "GeneralStructure";
            const string unknownGateOpeningDirection = "SlightlyLeftButAlsoSlightlyRight";
            
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            iniSection.AddProperty(StructureRegion.GateHorizontalOpeningDirection.Key, unknownGateOpeningDirection);

            var weir = new Weir();

            // Call
            void Call() => WeirFormulaParser.ReadFormulaFromDefinition(iniSection, weir, structuresFilePath, referenceDateTime);

            // Assert
            string expectedMessage = string.Format(Resources.WeirFormulaParser_Could_not_parse_horizontal_gate_opening,
                                                   unknownGateOpeningDirection);
            Assert.That(Call, Throws.ArgumentException.With.Message.EqualTo(expectedMessage));
        }
    }
}