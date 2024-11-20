using System;
using DelftTools.Functions;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
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
            Assert.That(function.UnderlyingFunction.Name, Is.EqualTo(WaveTimeDependentParametersConstants.WaveQuantityName),
                        "Expected a different function name:");

            Assert.That(function.UnderlyingFunction.Arguments, Has.Count.EqualTo(1),
                        "Expected a different number of Arguments:");
            Assert.That(function.TimeArgument.Name, Is.EqualTo(WaveTimeDependentParametersConstants.TimeVariableName));

            Assert.That(function.UnderlyingFunction.Components, Has.Count.EqualTo(4),
                        "Expected a different number of Components:");

            AssertHasCorrectComponent(function.HeightComponent,
                                      WaveTimeDependentParametersConstants.HeightVariableName,
                                      WaveTimeDependentParametersConstants.MeterUnitName,
                                      WaveTimeDependentParametersConstants.MeterUnitSymbol);

            AssertHasCorrectComponent(function.PeriodComponent,
                                      WaveTimeDependentParametersConstants.PeriodVariableName,
                                      WaveTimeDependentParametersConstants.SecondUnitName,
                                      WaveTimeDependentParametersConstants.SecondUnitSymbol,
                                      1.0);

            AssertHasCorrectComponent(function.DirectionComponent,
                                      WaveTimeDependentParametersConstants.DirectionVariableName,
                                      WaveTimeDependentParametersConstants.DegreesUnitName,
                                      WaveTimeDependentParametersConstants.DegreesUnitSymbol);
            AssertHasCorrectComponent(function.SpreadingComponent,
                                      WaveTimeDependentParametersConstants.SpreadingVariableName,
                                      SpreadingConversion.GetSpreadingUnit<TSpreading>().Name,
                                      SpreadingConversion.GetSpreadingUnit<TSpreading>().Symbol,
                                      SpreadingConversion.GetSpreadingDefaultValue<TSpreading>());

            Assert.That(function.UnderlyingFunction.Attributes[BcwFile.TimeFunctionAttributeName],
                        Is.EqualTo(WaveTimeDependentParametersConstants.NonEquidistantTimeFunctionAttributeName));
            Assert.That(function.UnderlyingFunction.Attributes[BcwFile.RefDateAttributeName],
                        Is.EqualTo(new DateTime().ToString(BcwFile.DateFormatString)));
            Assert.That(function.UnderlyingFunction.Attributes[BcwFile.TimeUnitAttributeName],
                        Is.EqualTo(WaveTimeDependentParametersConstants.MinuteUnitName));
        }

        [Test]
        public void ConvertSpreadingType_ToSameType_ExpectedResults()
        {
            // Setup
            var oldWaveEnergyFunction = new WaveEnergyFunction<TSpreading>();
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(oldWaveEnergyFunction.UnderlyingFunction, DateTime.Today, DateTime.Today + TimeSpan.FromDays(1), TimeSpan.FromHours(1));

            // Call
            IWaveEnergyFunction<TSpreading> newWaveFunction = WaveEnergyFunction<TSpreading>.ConvertSpreadingType(oldWaveEnergyFunction);

            // Assert
            Assert.That(newWaveFunction, Is.SameAs(oldWaveEnergyFunction));
        }

        [Test]
        public void ConvertSpreading_OldWaveEnergyFunctionNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => WaveEnergyFunction<TSpreading>.ConvertSpreadingType((IWaveEnergyFunction<TSpreading>) null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("oldWaveEnergyFunction"));
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