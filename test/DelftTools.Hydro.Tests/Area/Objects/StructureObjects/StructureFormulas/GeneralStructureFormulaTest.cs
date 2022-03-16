using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects.StructureObjects.StructureFormulas
{
    [TestFixture]
    public class GeneralStructureFormulaTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var formula = new GeneralStructureFormula();

            // Assert
            Assert.That(formula, Is.InstanceOf<IGatedStructureFormula>());

            Assert.That(formula.PositiveFreeGateFlow, Is.EqualTo(1.0));
            Assert.That(formula.PositiveContractionCoefficient, Is.EqualTo(1.0));
            Assert.That(formula.PositiveDrownedGateFlow, Is.EqualTo(1.0));
            Assert.That(formula.PositiveDrownedWeirFlow, Is.EqualTo(1.0));
            Assert.That(formula.PositiveFreeWeirFlow, Is.EqualTo(1.0));

            Assert.That(formula.NegativeContractionCoefficient, Is.EqualTo(1.0));
            Assert.That(formula.NegativeDrownedGateFlow, Is.EqualTo(1.0));
            Assert.That(formula.NegativeDrownedWeirFlow, Is.EqualTo(1.0));
            Assert.That(formula.NegativeFreeGateFlow, Is.EqualTo(1.0));
            Assert.That(formula.NegativeFreeWeirFlow, Is.EqualTo(1.0));

            Assert.That(formula.UseExtraResistance, Is.EqualTo(true));
            Assert.That(formula.ExtraResistance, Is.EqualTo(0.0));

            Assert.That(formula.GateOpening, Is.EqualTo(1.0));
            Assert.That(formula.DoorHeight, Is.EqualTo(0.0));

            Assert.That(formula.HorizontalDoorOpeningDirection, 
                        Is.EqualTo(GateOpeningDirection.Symmetric));
            Assert.That(formula.HorizontalDoorOpeningWidth, Is.EqualTo(0.0));
            Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries, Is.False);
            Assert.That(formula.HorizontalDoorOpeningWidthTimeSeries, Is.Null);

            Assert.That(formula.LowerEdgeLevel, Is.EqualTo(0.0));
            Assert.That(formula.UseLowerEdgeLevelTimeSeries, Is.False);
            Assert.That(formula.LowerEdgeLevelTimeSeries, Is.Null);

            Assert.That(formula.Name, Is.EqualTo("General Structure"));
        }

        [Test]
        public void Clone_ExpectedResults()
        {
            // Setup
            var formula = new GeneralStructureFormula
            {
                PositiveFreeGateFlow = 2.1,
                PositiveContractionCoefficient = 3.2,
                PositiveDrownedGateFlow = 4.3,
                PositiveDrownedWeirFlow = 5.4,
                PositiveFreeWeirFlow = 6.5,
                NegativeContractionCoefficient = 7.6,
                NegativeDrownedGateFlow = 8.7,
                NegativeDrownedWeirFlow = 9.8,
                NegativeFreeGateFlow = 10.9,
                NegativeFreeWeirFlow = 11.1,
                BedLevelLeftSideOfStructure = 12.11,
                BedLevelLeftSideStructure = 13.12,
                BedLevelStructureCentre = 14.13,
                BedLevelRightSideStructure = 15.14,
                BedLevelRightSideOfStructure = 16.15,
                WidthLeftSideOfStructure = 17.16,
                WidthStructureLeftSide = 18.17,
                WidthStructureCentre = 19.18,
                WidthStructureRightSide = 20.19,
                WidthRightSideOfStructure = 21.2,
                UseExtraResistance = true,
                ExtraResistance = 22.3,
                GateOpening = 23.4,
                DoorHeight = 24.5,
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                HorizontalDoorOpeningWidth = 25.6,
                LowerEdgeLevel = 26.7,
                UseHorizontalDoorOpeningWidthTimeSeries = true,
                UseLowerEdgeLevelTimeSeries =true 
            };

            // Call
            var clonedFormula = (GeneralStructureFormula) formula.Clone();

            // Assert
            Assert.That(clonedFormula, Is.Not.Null);
            Assert.That(clonedFormula, Is.InstanceOf<GeneralStructureFormula>());
            Assert.That(clonedFormula, Is.Not.SameAs(formula));

            Assert.That(clonedFormula.PositiveFreeGateFlow, Is.EqualTo(formula.PositiveFreeGateFlow));
            Assert.That(clonedFormula.PositiveContractionCoefficient, Is.EqualTo(formula.PositiveContractionCoefficient));
            Assert.That(clonedFormula.PositiveDrownedGateFlow, Is.EqualTo(formula.PositiveDrownedGateFlow));
            Assert.That(clonedFormula.PositiveDrownedWeirFlow, Is.EqualTo(formula.PositiveDrownedWeirFlow));
            Assert.That(clonedFormula.PositiveFreeWeirFlow, Is.EqualTo(formula.PositiveFreeWeirFlow));
            Assert.That(clonedFormula.NegativeContractionCoefficient, Is.EqualTo(formula.NegativeContractionCoefficient));
            Assert.That(clonedFormula.NegativeDrownedGateFlow, Is.EqualTo(formula.NegativeDrownedGateFlow));
            Assert.That(clonedFormula.NegativeDrownedWeirFlow, Is.EqualTo(formula.NegativeDrownedWeirFlow));
            Assert.That(clonedFormula.NegativeFreeGateFlow, Is.EqualTo(formula.NegativeFreeGateFlow));
            Assert.That(clonedFormula.NegativeFreeWeirFlow, Is.EqualTo(formula.NegativeFreeWeirFlow));
            Assert.That(clonedFormula.BedLevelLeftSideOfStructure, Is.EqualTo(formula.BedLevelLeftSideOfStructure));
            Assert.That(clonedFormula.BedLevelLeftSideStructure, Is.EqualTo(formula.BedLevelLeftSideStructure));
            Assert.That(clonedFormula.BedLevelStructureCentre, Is.EqualTo(formula.BedLevelStructureCentre));
            Assert.That(clonedFormula.BedLevelRightSideStructure, Is.EqualTo(formula.BedLevelRightSideStructure));
            Assert.That(clonedFormula.BedLevelRightSideOfStructure, Is.EqualTo(formula.BedLevelRightSideOfStructure));
            Assert.That(clonedFormula.WidthLeftSideOfStructure, Is.EqualTo(formula.WidthLeftSideOfStructure));
            Assert.That(clonedFormula.WidthStructureLeftSide, Is.EqualTo(formula.WidthStructureLeftSide));
            Assert.That(clonedFormula.WidthStructureCentre, Is.EqualTo(formula.WidthStructureCentre));
            Assert.That(clonedFormula.WidthStructureRightSide, Is.EqualTo(formula.WidthStructureRightSide));
            Assert.That(clonedFormula.WidthRightSideOfStructure, Is.EqualTo(formula.WidthRightSideOfStructure));
            Assert.That(clonedFormula.UseExtraResistance, Is.EqualTo(formula.UseExtraResistance));
            Assert.That(clonedFormula.ExtraResistance, Is.EqualTo(formula.ExtraResistance));
            Assert.That(clonedFormula.GateOpening, Is.EqualTo(formula.GateOpening));
            Assert.That(clonedFormula.DoorHeight, Is.EqualTo(formula.DoorHeight));
            Assert.That(clonedFormula.HorizontalDoorOpeningDirection, Is.EqualTo(formula.HorizontalDoorOpeningDirection));
            Assert.That(clonedFormula.HorizontalDoorOpeningWidth, Is.EqualTo(formula.HorizontalDoorOpeningWidth));
            Assert.That(clonedFormula.UseHorizontalDoorOpeningWidthTimeSeries, Is.EqualTo(formula.UseHorizontalDoorOpeningWidthTimeSeries));
            Assert.That(clonedFormula.LowerEdgeLevel, Is.EqualTo(formula.LowerEdgeLevel));
            Assert.That(clonedFormula.UseLowerEdgeLevelTimeSeries, Is.EqualTo(formula.UseLowerEdgeLevelTimeSeries));

            Assert.That(clonedFormula.HorizontalDoorOpeningWidthTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.HorizontalDoorOpeningWidthTimeSeries, 
                        Is.Not.SameAs(formula.HorizontalDoorOpeningWidthTimeSeries));
            Assert.That(clonedFormula.LowerEdgeLevelTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.LowerEdgeLevelTimeSeries, 
                        Is.Not.SameAs(formula.LowerEdgeLevelTimeSeries));
        }

        [Test]
        public void UseHorizontalDoorOpeningWidthTimeSeries_SetToTrue_CreatesTimeSeries()
        {
            // Setup
            var formula = new GeneralStructureFormula();

            Assert.That(formula.HorizontalDoorOpeningWidthTimeSeries, Is.Null);

            // Call
            formula.UseHorizontalDoorOpeningWidthTimeSeries = true;

            // Assert
            Assert.That(formula.HorizontalDoorOpeningWidthTimeSeries, Is.Not.Null);
        }

        [Test]
        public void UseLowerEdgeLevelTimeSeries_SetToTrue_CreatesTimeSeries()
        {
            // Setup
            var formula = new GeneralStructureFormula();

            Assert.That(formula.LowerEdgeLevelTimeSeries, Is.Null);

            // Call
            formula.UseLowerEdgeLevelTimeSeries= true;

            // Assert
            Assert.That(formula.LowerEdgeLevelTimeSeries, Is.Not.Null);
        }

        public static IEnumerable<TestCaseData> GetSetPropertyData()
        {
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream2Width, new Func<GeneralStructureFormula, double>(f => f.WidthLeftSideOfStructure));
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream1Width, new Func<GeneralStructureFormula, double>(f => f.WidthStructureLeftSide));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream1Width, new Func<GeneralStructureFormula, double>(f => f.WidthStructureRightSide));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream2Width, new Func<GeneralStructureFormula, double>(f => f.WidthRightSideOfStructure));
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream2Level, new Func<GeneralStructureFormula, double>(f => f.BedLevelLeftSideOfStructure));
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream1Level, new Func<GeneralStructureFormula, double>(f => f.BedLevelLeftSideStructure));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream1Level, new Func<GeneralStructureFormula, double>(f => f.BedLevelRightSideStructure));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream2Level, new Func<GeneralStructureFormula, double>(f => f.BedLevelRightSideOfStructure));
            yield return new TestCaseData(KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.PositiveFreeGateFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.PositiveDrownedGateFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.PositiveFreeWeirFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.PositiveDrownedWeirFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate, new Func<GeneralStructureFormula, double>(f => f.PositiveContractionCoefficient));
            yield return new TestCaseData(KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.NegativeFreeGateFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.NegativeDrownedGateFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.NegativeFreeWeirFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient, new Func<GeneralStructureFormula, double>(f => f.NegativeDrownedWeirFlow));
            yield return new TestCaseData(KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate, new Func<GeneralStructureFormula, double>(f => f.NegativeContractionCoefficient));
            yield return new TestCaseData(KnownGeneralStructureProperties.ExtraResistance, new Func<GeneralStructureFormula, double>(f => f.ExtraResistance));
            yield return new TestCaseData(KnownGeneralStructureProperties.GateHeight, new Func<GeneralStructureFormula, double>(f => f.DoorHeight));
        }

        [Test]
        [TestCaseSource(nameof(GetSetPropertyData))]
        public void SetPropertyValue_ExpectedResult(KnownGeneralStructureProperties property, 
                                                    Func<GeneralStructureFormula, double> propFunc)
        {
            // Setup
            var formula = new GeneralStructureFormula();
            const double value = 12.3;

            // Call
            formula.SetPropertyValue(property, value);

            // Assert
            Assert.That(propFunc(formula), Is.EqualTo(value));
        }

        [Test]
        public void SetPropertyValue_ExtraResistanceZero_UseExtraResistanceFalse()
        {
            // Setup
            var formula = new GeneralStructureFormula
            {
                UseExtraResistance = true,
                ExtraResistance = 15.0,
            };

            // Call
            formula.SetPropertyValue(KnownGeneralStructureProperties.ExtraResistance, 0.0);

            // Assert
            Assert.That(formula.ExtraResistance, Is.EqualTo(0.0));
            Assert.That(formula.UseExtraResistance, Is.False);
        }

        [Test]
        [TestCase(KnownGeneralStructureProperties.CrestWidth)]
        [TestCase(KnownGeneralStructureProperties.CrestLevel)]
        [TestCase(KnownGeneralStructureProperties.GateLowerEdgeLevel)]
        [TestCase(KnownGeneralStructureProperties.GateOpeningHorizontalDirection)]
        [TestCase(KnownGeneralStructureProperties.GateOpeningWidth)]
        public void SetPropertyValue_InvalidKnownGeneralStructureProperty_ThrowsArgumentOutOfRangeException(KnownGeneralStructureProperties property)
        {
            // Setup
            var formula = new GeneralStructureFormula();

            // Call
            void Call() => formula.SetPropertyValue(property, 1.23);

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("property"));
            Assert.That(e.Message, Does.StartWith($"Property {property} is not a valid property of a general structure weir formula."));
        }

        [Test]
        public void SetPropertyValue_UndefinedKnownGeneralStructureProperty_ThrowsException()
        {
            // Setup
            var formula = new GeneralStructureFormula();

            // Call | Assert
            void Call() => formula.SetPropertyValue((KnownGeneralStructureProperties)int.MaxValue, 5.0);

            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("property"));
        }
    }
}