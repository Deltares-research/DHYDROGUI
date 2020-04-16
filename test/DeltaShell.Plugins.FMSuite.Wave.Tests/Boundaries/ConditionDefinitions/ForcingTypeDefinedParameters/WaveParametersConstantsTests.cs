using System;
using System.Collections.Generic;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    [TestFixture]
    public class WaveParametersConstantsTests
    {
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
    }
}