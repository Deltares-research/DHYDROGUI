using System;
using System.Collections.Generic;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    [TestFixture]
    public class WaveTimeDependentParametersConstantsTest
    {
        [Test]
        [TestCaseSource(nameof(GetConstructUnitTestData))]
        public void ConstructUnit_ExpectedResults(Func<Unit> constructFunc, string expectedName, string expectedSymbol)
        {
            // Call
            Unit unit = constructFunc.Invoke();

            // Assert
            Assert.That(unit.Name, Is.EqualTo(expectedName), $"Expected a different {nameof(Unit.Name)}:");
            Assert.That(unit.Symbol, Is.EqualTo(expectedSymbol), $"Expected a different {nameof(Unit.Symbol)}:");
        }

        [Test]
        public void WaveQuantityName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.WaveQuantityName, Is.EqualTo("wave_energy_density"));
        }

        [Test]
        public void TimeVariableName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.TimeVariableName, Is.EqualTo("Time"));
        }

        [Test]
        public void HeightVariableName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.HeightVariableName, Is.EqualTo("Hs"));
        }

        [Test]
        public void PeriodVariableName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.PeriodVariableName, Is.EqualTo("Tp"));
        }

        [Test]
        public void DirectionVariableName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.DirectionVariableName, Is.EqualTo("Dir"));
        }

        [Test]
        public void SpreadingVariableName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.SpreadingVariableName, Is.EqualTo("Spreading"));
        }

        [Test]
        public void MinuteUnitName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.MinuteUnitName, Is.EqualTo("minutes"));
        }

        [Test]
        public void MeterUnitName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.MeterUnitName, Is.EqualTo("meter"));
        }

        [Test]
        public void MeterUnitSymbol_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.MeterUnitSymbol, Is.EqualTo("m"));
        }

        [Test]
        public void SecondUnitName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.SecondUnitName, Is.EqualTo("second"));
        }

        [Test]
        public void SecondUnitSymbol_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.SecondUnitSymbol, Is.EqualTo("s"));
        }

        [Test]
        public void DegreesUnitName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.DegreesUnitName, Is.EqualTo("degrees"));
        }

        [Test]
        public void DegreesUnitSymbol_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.DegreesUnitSymbol, Is.EqualTo("deg"));
        }

        [Test]
        public void PowerUnitName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.PowerUnitName, Is.EqualTo("power"));
        }

        [Test]
        public void PowerUnitSymbol_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.PowerUnitSymbol, Is.EqualTo("-"));
        }

        [Test]
        public void NonEquidistantTimeFunctionAttributeName_ExpectedResults()
        {
            Assert.That(WaveTimeDependentParametersConstants.NonEquidistantTimeFunctionAttributeName,
                        Is.EqualTo("non-equidistant"));
        }

        private static IEnumerable<TestCaseData> GetConstructUnitTestData()
        {
            yield return new TestCaseData((Func<Unit>) WaveTimeDependentParametersConstants.ConstructMeterUnit,
                                          WaveTimeDependentParametersConstants.MeterUnitName,
                                          WaveTimeDependentParametersConstants.MeterUnitSymbol);
            yield return new TestCaseData((Func<Unit>) WaveTimeDependentParametersConstants.ConstructSecondUnit,
                                          WaveTimeDependentParametersConstants.SecondUnitName,
                                          WaveTimeDependentParametersConstants.SecondUnitSymbol);
            yield return new TestCaseData((Func<Unit>) WaveTimeDependentParametersConstants.ConstructDegreesUnit,
                                          WaveTimeDependentParametersConstants.DegreesUnitName,
                                          WaveTimeDependentParametersConstants.DegreesUnitSymbol);
            yield return new TestCaseData((Func<Unit>) WaveTimeDependentParametersConstants.ConstructPowerUnit,
                                          WaveTimeDependentParametersConstants.PowerUnitName,
                                          WaveTimeDependentParametersConstants.PowerUnitSymbol);
        }
    }
}