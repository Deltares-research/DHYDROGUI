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
            Assert.That(formula.GateHeight, Is.EqualTo(0.0));

            Assert.That(formula.GateOpeningHorizontalDirection, 
                        Is.EqualTo(GateOpeningDirection.Symmetric));
            Assert.That(formula.HorizontalGateOpeningWidth, Is.EqualTo(0.0));
            Assert.That(formula.UseHorizontalGateOpeningWidthTimeSeries, Is.False);
            Assert.That(formula.HorizontalGateOpeningWidthTimeSeries, Is.Null);

            Assert.That(formula.GateLowerEdgeLevel, Is.EqualTo(0.0));
            Assert.That(formula.UseGateLowerEdgeLevelTimeSeries, Is.False);
            Assert.That(formula.GateLowerEdgeLevelTimeSeries, Is.Null);

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
                Upstream1Level = 12.11,
                Upstream2Level = 13.12,
                CrestLevel = 14.13,
                Downstream1Level = 15.14,
                Downstream2Level = 16.15,
                Upstream1Width = 17.16,
                Upstream2Width = 18.17,
                CrestWidth = 19.18,
                Downstream1Width = 20.19,
                Downstream2Width = 21.2,
                UseExtraResistance = true,
                ExtraResistance = 22.3,
                GateOpening = 23.4,
                GateHeight = 24.5,
                GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                HorizontalGateOpeningWidth = 25.6,
                GateLowerEdgeLevel = 26.7,
                UseHorizontalGateOpeningWidthTimeSeries = true,
                UseGateLowerEdgeLevelTimeSeries =true 
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
            Assert.That(clonedFormula.Upstream1Level, Is.EqualTo(formula.Upstream1Level));
            Assert.That(clonedFormula.Upstream2Level, Is.EqualTo(formula.Upstream2Level));
            Assert.That(clonedFormula.CrestLevel, Is.EqualTo(formula.CrestLevel));
            Assert.That(clonedFormula.Downstream1Level, Is.EqualTo(formula.Downstream1Level));
            Assert.That(clonedFormula.Downstream2Level, Is.EqualTo(formula.Downstream2Level));
            Assert.That(clonedFormula.Upstream1Width, Is.EqualTo(formula.Upstream1Width));
            Assert.That(clonedFormula.Upstream2Width, Is.EqualTo(formula.Upstream2Width));
            Assert.That(clonedFormula.CrestWidth, Is.EqualTo(formula.CrestWidth));
            Assert.That(clonedFormula.Downstream1Width, Is.EqualTo(formula.Downstream1Width));
            Assert.That(clonedFormula.Downstream2Width, Is.EqualTo(formula.Downstream2Width));
            Assert.That(clonedFormula.UseExtraResistance, Is.EqualTo(formula.UseExtraResistance));
            Assert.That(clonedFormula.ExtraResistance, Is.EqualTo(formula.ExtraResistance));
            Assert.That(clonedFormula.GateOpening, Is.EqualTo(formula.GateOpening));
            Assert.That(clonedFormula.GateHeight, Is.EqualTo(formula.GateHeight));
            Assert.That(clonedFormula.GateOpeningHorizontalDirection, Is.EqualTo(formula.GateOpeningHorizontalDirection));
            Assert.That(clonedFormula.HorizontalGateOpeningWidth, Is.EqualTo(formula.HorizontalGateOpeningWidth));
            Assert.That(clonedFormula.UseHorizontalGateOpeningWidthTimeSeries, Is.EqualTo(formula.UseHorizontalGateOpeningWidthTimeSeries));
            Assert.That(clonedFormula.GateLowerEdgeLevel, Is.EqualTo(formula.GateLowerEdgeLevel));
            Assert.That(clonedFormula.UseGateLowerEdgeLevelTimeSeries, Is.EqualTo(formula.UseGateLowerEdgeLevelTimeSeries));

            Assert.That(clonedFormula.HorizontalGateOpeningWidthTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.HorizontalGateOpeningWidthTimeSeries, 
                        Is.Not.SameAs(formula.HorizontalGateOpeningWidthTimeSeries));
            Assert.That(clonedFormula.GateLowerEdgeLevelTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.GateLowerEdgeLevelTimeSeries, 
                        Is.Not.SameAs(formula.GateLowerEdgeLevelTimeSeries));
        }

        [Test]
        public void UseHorizontalGateOpeningWidthTimeSeries_SetToTrue_CreatesTimeSeries()
        {
            // Setup
            var formula = new GeneralStructureFormula();

            Assert.That(formula.HorizontalGateOpeningWidthTimeSeries, Is.Null);

            // Call
            formula.UseHorizontalGateOpeningWidthTimeSeries = true;

            // Assert
            Assert.That(formula.HorizontalGateOpeningWidthTimeSeries, Is.Not.Null);
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_SetToTrue_CreatesTimeSeries()
        {
            // Setup
            var formula = new GeneralStructureFormula();

            Assert.That(formula.GateLowerEdgeLevelTimeSeries, Is.Null);

            // Call
            formula.UseGateLowerEdgeLevelTimeSeries= true;

            // Assert
            Assert.That(formula.GateLowerEdgeLevelTimeSeries, Is.Not.Null);
        }

        public static IEnumerable<TestCaseData> GetSetPropertyData()
        {
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream1Width, new Func<GeneralStructureFormula, double>(f => f.Upstream1Width));
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream2Width, new Func<GeneralStructureFormula, double>(f => f.Upstream2Width));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream1Width, new Func<GeneralStructureFormula, double>(f => f.Downstream1Width));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream2Width, new Func<GeneralStructureFormula, double>(f => f.Downstream2Width));
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream1Level, new Func<GeneralStructureFormula, double>(f => f.Upstream1Level));
            yield return new TestCaseData(KnownGeneralStructureProperties.Upstream2Level, new Func<GeneralStructureFormula, double>(f => f.Upstream2Level));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream1Level, new Func<GeneralStructureFormula, double>(f => f.Downstream1Level));
            yield return new TestCaseData(KnownGeneralStructureProperties.Downstream2Level, new Func<GeneralStructureFormula, double>(f => f.Downstream2Level));
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
            yield return new TestCaseData(KnownGeneralStructureProperties.GateHeight, new Func<GeneralStructureFormula, double>(f => f.GateHeight));
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