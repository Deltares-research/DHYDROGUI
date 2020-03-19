using System;
using System.Collections.Generic;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Parameters
{
    [TestFixture]
    public class WaveParametersConstantsTests
    {
        private static IEnumerable<TestCaseData> GetConstructUnitTestData()
        {
            yield return new TestCaseData((Func<Unit>) WaveParametersConstants.ConstructMeterUnit, 
                                           WaveParametersConstants.MeterUnitName, 
                                           WaveParametersConstants.MeterUnitSymbol);
            yield return new TestCaseData((Func<Unit>) WaveParametersConstants.ConstructSecondUnit, 
                                           WaveParametersConstants.SecondUnitName, 
                                           WaveParametersConstants.SecondUnitSymbol);
            yield return new TestCaseData((Func<Unit>) WaveParametersConstants.ConstructEmptyUnit, 
                                           WaveParametersConstants.EmptyUnitName, 
                                           WaveParametersConstants.EmptyUnitSymbol);
            yield return new TestCaseData((Func<Unit>) WaveParametersConstants.ConstructDegreesUnit, 
                                           WaveParametersConstants.DegreesUnitName, 
                                           WaveParametersConstants.DegreesUnitSymbol);
            yield return new TestCaseData((Func<Unit>) WaveParametersConstants.ConstructPowerUnit, 
                                           WaveParametersConstants.PowerUnitName, 
                                           WaveParametersConstants.PowerUnitSymbol);
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