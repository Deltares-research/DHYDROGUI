using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.WaveEnergyFunctions
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class WaveEnergyFunctionTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void ConstructTimeDependentParameters_WaveEnergyFunctionNull_ThrowsArgumentNullException()
        {
            // Call
            var function = new WaveEnergyFunction<TSpreading>();

            // Assert
            Assert.That(function.UnderlyingFunction, Has.Property(nameof(IFunction.Name))
                                     .EqualTo(WaveParametersConstants.WaveQuantityName),
                        "Expected a different function name:");

            Assert.That(function.UnderlyingFunction.Arguments, Has.Count.EqualTo(1),
                        "Expected a different number of Arguments:");
            Assert.That(function.TimeArgument, Has.Property(nameof(Variable<double>.Name))
                                     .EqualTo(WaveParametersConstants.TimeVariableName));

            Assert.That(function.UnderlyingFunction.Components, Has.Count.EqualTo(4),
                        "Expected a different number of Components:");

            AssertHasCorrectComponent(function.HeightComponent,
                                      WaveParametersConstants.HeightVariableName,
                                      WaveParametersConstants.MeterUnitName,
                                      WaveParametersConstants.MeterUnitSymbol);

            AssertHasCorrectComponent(function.PeriodComponent,
                                      WaveParametersConstants.PeriodVariableName,
                                      WaveParametersConstants.SecondUnitName,
                                      WaveParametersConstants.SecondUnitSymbol,
                                      1.0);

            AssertHasCorrectComponent(function.DirectionComponent,
                                      WaveParametersConstants.DirectionVariableName,
                                      WaveParametersConstants.DegreesUnitName,
                                      WaveParametersConstants.DegreesUnitSymbol);
            AssertHasCorrectComponent(function.SpreadingComponent, 
                                      WaveParametersConstants.SpreadingVariableName,
                                      SpreadingConversion.GetSpreadingUnit<TSpreading>().Name,
                                      SpreadingConversion.GetSpreadingUnit<TSpreading>().Symbol, 
                                      SpreadingConversion.GetSpreadingDefaultValue<TSpreading>());

            Assert.That(function.UnderlyingFunction.Attributes[BcwFile.TimeFunctionAttributeName], 
                        Is.EqualTo(WaveParametersConstants.NonEquidistantTimeFunctionAttributeName));
            Assert.That(function.UnderlyingFunction.Attributes[BcwFile.RefDateAttributeName], 
                        Is.EqualTo(new DateTime().ToString(BcwFile.DateFormatString)));
            Assert.That(function.UnderlyingFunction.Attributes[BcwFile.TimeUnitAttributeName], 
                        Is.EqualTo(WaveParametersConstants.MinuteUnitName));
        }

        private static void AssertHasCorrectComponent(IVariable component,
                                                      string componentName,
                                                      string expectedUnitName,
                                                      string expectedUnitSymbol,
                                                      double? defaultValue = null)
        {
            Assert.That(component, Is.Not.Null, $"Expected component with name: {componentName} to exist.");
            Assert.That(component.Unit.Name, Is.EqualTo(expectedUnitName), "Expected a different unit name:");
            Assert.That(component.Unit.Symbol, Is.EqualTo(expectedUnitSymbol), "Expected a different unit symbol:");

            if (defaultValue != null)
            {
                Assert.That(component.DefaultValue, Is.EqualTo(defaultValue), 
                            "Expected a different default value");
            }
        }
        
    }
}