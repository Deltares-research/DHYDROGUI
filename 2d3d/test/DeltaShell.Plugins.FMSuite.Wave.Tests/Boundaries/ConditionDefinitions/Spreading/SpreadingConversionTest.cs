using System;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Spreading
{
    [TestFixture]
    public class SpreadingConversionTest
    {
        private readonly Random random = new Random();

        [Test]
        public void FromDouble_UnsupportedSpreading_ThrowsNotSupportedException()
        {
            // Setup
            double spreadingValue = random.NextDouble();

            // Call
            void Call() => SpreadingConversion.FromDouble<DummyConditionSpreading>(spreadingValue);

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void FromDouble_DegreesDefinedSpreading_ReturnsCorrectResult()
        {
            // Setup
            double spreadingValue = random.NextDouble();

            // Call
            var result = SpreadingConversion.FromDouble<DegreesDefinedSpreading>(spreadingValue);

            // Assert
            Assert.That(result.DegreesSpreading, Is.EqualTo(spreadingValue));
        }

        [Test]
        public void FromDouble_PowerDefinedSpreading_ReturnsCorrectResult()
        {
            // Setup
            double spreadingValue = random.NextDouble();

            // Call
            var result = SpreadingConversion.FromDouble<PowerDefinedSpreading>(spreadingValue);

            // Assert
            Assert.That(result.SpreadingPower, Is.EqualTo(spreadingValue));
        }

        [Test]
        public void GetSpreadingUnit_DegreesDefinedSpreading_ExpectedResults()
        {
            // Call
            Unit unit = SpreadingConversion.GetSpreadingUnit<DegreesDefinedSpreading>();

            // Assert
            Assert.That(unit.Name, Is.EqualTo(WaveTimeDependentParametersConstants.DegreesUnitName));
            Assert.That(unit.Symbol, Is.EqualTo(WaveTimeDependentParametersConstants.DegreesUnitSymbol));
        }

        [Test]
        public void GetSpreadingUnit_PowerDefinedSpreading_ExpectedResults()
        {
            // Call
            Unit unit = SpreadingConversion.GetSpreadingUnit<PowerDefinedSpreading>();

            // Assert
            Assert.That(unit.Name, Is.EqualTo(WaveTimeDependentParametersConstants.PowerUnitName));
            Assert.That(unit.Symbol, Is.EqualTo(WaveTimeDependentParametersConstants.PowerUnitSymbol));
        }

        [Test]
        public void GetSpreadingUnit_UnsupportedSpreading_ThrowsNotSupportedException()
        {
            // Call | Assert
            void Call() => SpreadingConversion.GetSpreadingUnit<DummyConditionSpreading>();

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetSpreadingDefaultValue_DegreesDefinedSpreading_ExpectedResults()
        {
            // Call
            double defaultValue = SpreadingConversion.GetSpreadingDefaultValue<DegreesDefinedSpreading>();

            // Assert
            Assert.That(defaultValue, Is.EqualTo(WaveSpreadingConstants.DegreesDefaultSpreading));
        }

        [Test]
        public void GetSpreadingDefaultValue_PowerDefinedSpreading_ExpectedResults()
        {
            // Call
            double defaultValue = SpreadingConversion.GetSpreadingDefaultValue<PowerDefinedSpreading>();

            // Assert
            Assert.That(defaultValue, Is.EqualTo(WaveSpreadingConstants.PowerDefaultSpreading));
        }

        [Test]
        public void GetSpreadingDefaultValue_UnsupportedSpreading_ThrowsNotSupportedException()
        {
            // Call | Assert
            void Call() => SpreadingConversion.GetSpreadingDefaultValue<DummyConditionSpreading>();

            Assert.Throws<NotSupportedException>(Call);
        }

        private class DummyConditionSpreading : IBoundaryConditionSpreading
        {
            public void AcceptVisitor(ISpreadingVisitor visitor) {}
        }
    }
}