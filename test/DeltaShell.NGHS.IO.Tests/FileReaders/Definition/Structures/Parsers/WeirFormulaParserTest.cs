using System;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class WeirFormulaParserTest
    {
        [Test]
        public void ReadFormulaFromDefinition_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            var weir = new Weir();

            // Call
            TestDelegate call = () => WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void ReadFormulaFromDefinition_WeirNull_ThrowsArgumentNullException()
        {
            // Setup
            var category = StructureParserTestHelper.CreateStructureCategory();
            Weir weir = null;

            // Call
            TestDelegate call = () => WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void ReadFormulaFromDefinition_UnknownWeirFormulaType_ThrowsInvalidOperationException()
        {
            // Setup
            const string unknownFormulaType = "UnknownFormulaType";

            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, unknownFormulaType);

            var weir = new Weir();

            // Call
            TestDelegate call = () => WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

            // Assert
            string expectedMessage = string.Format(Resources.WeirFormulaParser_Unknow_formula_type, unknownFormulaType);
            Assert.That(call, Throws.Exception.TypeOf<InvalidOperationException>()
                                    .With.Message.EqualTo(expectedMessage));
        }
        
        [Test]
        public void ReadFormulaFromDefinition_SimpleWeirFormulaType_ParsesWeirFormulaCorrectly()
        {
            // Setup
            const string formulaType = "weir";
            const double correctionCoefficient = 123.456;
            
            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            category.AddProperty(StructureRegion.CorrectionCoeff.Key, correctionCoefficient);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

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

            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            category.AddProperty(StructureRegion.DischargeCoeff.Key, dischargeCoefficient);
            category.AddProperty(StructureRegion.YValues.Key, string.Join(", ", yValues));
            category.AddProperty(StructureRegion.ZValues.Key, string.Join(", ", zValues));

            var weir = new Weir() { CrestLevel = crestLevel };

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

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

            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            category.AddProperty(StructureRegion.NPiers.Key, numberOfPiers);
            category.AddProperty(StructureRegion.PosHeight.Key, upstreamFacePos);
            category.AddProperty(StructureRegion.PosDesignHead.Key, designHeadPos);
            category.AddProperty(StructureRegion.PosPierContractCoef.Key, pierContractionPos);
            category.AddProperty(StructureRegion.PosAbutContractCoef.Key, abutmentContractionPos);
            category.AddProperty(StructureRegion.NegHeight.Key, upstreamFaceNeg);
            category.AddProperty(StructureRegion.NegDesignHead.Key, designHeadNeg);
            category.AddProperty(StructureRegion.NegPierContractCoef.Key, pierContractionNeg);
            category.AddProperty(StructureRegion.NegAbutContractCoef.Key, abutmentContractionNeg);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

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
            const double gateOpening = 12.34;
            const double contractionCoefficient = 23.45;
            const bool useMaxFlowPos = true;
            const double maxFlowPos = 34.56;
            const bool useMaxFlowNeg = false;
            const double maxFlowNeg = 45.67;
            const double crestLevel = 1.234;
            
            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            category.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, gateOpening);
            category.AddProperty(StructureRegion.CorrectionCoeff.Key, contractionCoefficient);
            category.AddProperty(StructureRegion.UseLimitFlowPos.Key, useMaxFlowPos.ToString());
            category.AddProperty(StructureRegion.LimitFlowPos.Key, maxFlowPos);
            category.AddProperty(StructureRegion.UseLimitFlowNeg.Key, useMaxFlowNeg.ToString());
            category.AddProperty(StructureRegion.LimitFlowNeg.Key, maxFlowNeg);

            var weir = new Weir() { CrestLevel = crestLevel };

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

            // Assert
            Assert.That(parsedWeirFormula, Is.TypeOf<GatedWeirFormula>());

            var gatedWeirFormula = (GatedWeirFormula) parsedWeirFormula;
            Assert.That(gatedWeirFormula.GateOpening, Is.EqualTo(gateOpening - crestLevel));
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
            
            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            category.AddProperty(StructureRegion.Upstream1Width.Key, widthLeftSideOfStructure);
            category.AddProperty(StructureRegion.Upstream2Width.Key, widthStructureLeftSide);
            category.AddProperty(StructureRegion.CrestWidth.Key, widthStructureCentre);
            category.AddProperty(StructureRegion.Downstream1Width.Key, widthStructureRightSide);
            category.AddProperty(StructureRegion.Downstream2Width.Key, widthRightSideOfStructure);
            category.AddProperty(StructureRegion.Upstream1Level.Key, bedLevelLeftSideOfStructure);
            category.AddProperty(StructureRegion.Upstream2Level.Key, bedLevelLeftSideStructure);
            category.AddProperty(StructureRegion.CrestLevel.Key, bedLevelStructureCentre);
            category.AddProperty(StructureRegion.Downstream1Level.Key, bedLevelRightSideStructure);
            category.AddProperty(StructureRegion.Downstream2Level.Key, bedLevelRightSideOfStructure);
            category.AddProperty(StructureRegion.PosFreeGateFlowCoeff.Key, positiveFreeGateFlow);
            category.AddProperty(StructureRegion.PosDrownGateFlowCoeff.Key, positiveDrownedGateFlow);
            category.AddProperty(StructureRegion.PosFreeWeirFlowCoeff.Key, positiveFreeWeirFlow);
            category.AddProperty(StructureRegion.PosDrownWeirFlowCoeff.Key, positiveDrownedWeirFlow);
            category.AddProperty(StructureRegion.PosContrCoefFreeGate.Key, positiveContractionCoefficient);
            category.AddProperty(StructureRegion.NegFreeGateFlowCoeff.Key, negativeFreeGateFlow);
            category.AddProperty(StructureRegion.NegDrownGateFlowCoeff.Key, negativeDrownedGateFlow);
            category.AddProperty(StructureRegion.NegFreeWeirFlowCoeff.Key, negativeFreeWeirFlow);
            category.AddProperty(StructureRegion.NegDrownWeirFlowCoeff.Key, negativeDrownedWeirFlow);
            category.AddProperty(StructureRegion.NegContrCoefFreeGate.Key, negativeContractionCoefficient);
            category.AddProperty(StructureRegion.GateHeight.Key, gateHeight);
            category.AddProperty(StructureRegion.CrestLength.Key, crestLength);
            category.AddProperty(StructureRegion.GateOpeningWidth.Key, gateOpeningWidth);
            category.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, lowerEdgeLevel);
            category.AddProperty(StructureRegion.ExtraResistance.Key, extraResistance);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

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
            
            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            category.AddProperty(StructureRegion.GateHorizontalOpeningDirection.Key, gateOpeningDirection);

            var weir = new Weir();

            // Call
            IWeirFormula parsedWeirFormula = WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

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
            
            var category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.DefinitionType.Key, formulaType);
            category.AddProperty(StructureRegion.GateHorizontalOpeningDirection.Key, unknownGateOpeningDirection);

            var weir = new Weir();

            // Call
            TestDelegate call = () => WeirFormulaParser.ReadFormulaFromDefinition(category, weir);

            // Assert
            string expectedMessage = string.Format(Resources.WeirFormulaParser_Could_not_parse_horizontal_gate_opening,
                                                   unknownGateOpeningDirection);
            Assert.That(call, Throws.ArgumentException.With.Message.EqualTo(expectedMessage));
        }
    }
}