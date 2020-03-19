using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Parameters
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class BoundaryParametersFactoryTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var factory = new BoundaryParametersFactory();

            // Assert
            Assert.That(factory, Is.InstanceOf<IBoundaryParametersFactory>());
        }

        [Test]
        public void ConstructDefaultConstantParameters_ExpectedValues()
        {
            // Setup
            var factory = new BoundaryParametersFactory();

            // Call
            ConstantParameters<TSpreading> parameters = 
                factory.ConstructDefaultConstantParameters<TSpreading>();

            // Assert
            Assert.That(parameters.Height, Is.EqualTo(0.0));
            Assert.That(parameters.Period, Is.EqualTo(1.0));
            Assert.That(parameters.Direction, Is.EqualTo(0.0));
            Assert.That(parameters.Spreading, Is.Not.Null);
            Assert.That(parameters.Spreading, Is.InstanceOf<TSpreading>());
        }

        [Test]
        public void ConstructConstantParameters_ExpectedValues()
        {
            // Setup
            var factory = new BoundaryParametersFactory();

            const double expectedHeight = 1.5;
            const double expectedPeriod = 2.5;
            const double expectedDirection = 3.5;

            var expectedSpreading = new TSpreading();

            // Call
            ConstantParameters<TSpreading> parameters =
                factory.ConstructConstantParameters(expectedHeight, 
                                                    expectedPeriod, 
                                                    expectedDirection,
                                                    expectedSpreading);

            // Assert
            Assert.That(parameters.Height, Is.EqualTo(expectedHeight));
            Assert.That(parameters.Period, Is.EqualTo(expectedPeriod));
            Assert.That(parameters.Direction, Is.EqualTo(expectedDirection));
            Assert.That(parameters.Spreading, Is.SameAs(expectedSpreading));
        }

        [Test]
        public void ConvertConstantParameters_ParametersNull_ThrowsArgumentNullException()
        {
            var factory = new BoundaryParametersFactory();

            void Call() => factory.ConvertConstantParameters<TSpreading, PowerDefinedSpreading>(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void ConvertConstantParameters_PowerDefined_ExpectedResults()
        {
            // Setup
            var initialParameters = new ConstantParameters<TSpreading>(5, 6, 7, new TSpreading());
            var factory = new BoundaryParametersFactory();

            // Call
            ConstantParameters<PowerDefinedSpreading> result = factory.ConvertConstantParameters<TSpreading, PowerDefinedSpreading>(initialParameters);

            // Assert
            Assert.That(result.Direction, Is.EqualTo(initialParameters.Direction));
            Assert.That(result.Period, Is.EqualTo(initialParameters.Period));
            Assert.That(result.Height, Is.EqualTo(initialParameters.Height));
        }

        [Test]
        public void ConvertConstantParameters_DegreesDefined_ExpectedResults()
        {
            // Setup
            var initialParameters = new ConstantParameters<TSpreading>(15, 16, 17, new TSpreading());
            var factory = new BoundaryParametersFactory();

            // Call
            ConstantParameters<DegreesDefinedSpreading> result = factory.ConvertConstantParameters<TSpreading, DegreesDefinedSpreading>(initialParameters);

            // Assert
            Assert.That(result.Direction, Is.EqualTo(initialParameters.Direction));
            Assert.That(result.Period, Is.EqualTo(initialParameters.Period));
            Assert.That(result.Height, Is.EqualTo(initialParameters.Height));
        }

        [Test]
        public void ConstructTimeDependentParameters_ExpectedResults()
        {
            // Setup
            var factory = new BoundaryParametersFactory();
            var waveEnergyFunction = Substitute.For<IFunction>();

            // Call
            TimeDependentParameters parameters = factory.ConstructTimeDependentParameters(waveEnergyFunction);

            // Assert
            Assert.That(parameters.WaveEnergyFunction, Is.SameAs(waveEnergyFunction),
                        $"Expected a different {nameof(TimeDependentParameters.WaveEnergyFunction)}:");
        }

        [Test]
        public void ConstructTimeDependentParameters_WaveEnergyFunctionNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = new BoundaryParametersFactory();

            // Call
            TimeDependentParameters parameters = factory.ConstructDefaultTimeDependentParameters<TSpreading>();

            // Assert
            Assert.That(parameters.WaveEnergyFunction, Has.Property(nameof(IFunction.Name))
                                                          .EqualTo(WaveParametersConstants.WaveQuantityName),
                        "Expected a different function name:");

            Assert.That(parameters.WaveEnergyFunction.Arguments, Has.Count.EqualTo(1),
                        "Expected a different number of Arguments:");
            IVariable argument = parameters.WaveEnergyFunction.Arguments.First();
            Assert.That(argument, Has.Property(nameof(Variable<double>.Name))
                                     .EqualTo(WaveParametersConstants.TimeVariableName));

            Assert.That(parameters.WaveEnergyFunction.Components, Has.Count.EqualTo(4),
                        "Expected a different number of Components:");

            AssertHasCorrectComponent(parameters.WaveEnergyFunction,
                                      WaveParametersConstants.HeightVariableName,
                                      WaveParametersConstants.MeterUnitName,
                                      WaveParametersConstants.MeterUnitSymbol);

            AssertHasCorrectComponent(parameters.WaveEnergyFunction,
                                      WaveParametersConstants.PeriodVariableName,
                                      WaveParametersConstants.SecondUnitName,
                                      WaveParametersConstants.SecondUnitSymbol,
                                      1.0);

            AssertHasCorrectComponent(parameters.WaveEnergyFunction,
                                      WaveParametersConstants.DirectionVariableName,
                                      WaveParametersConstants.DegreesUnitName,
                                      WaveParametersConstants.DegreesUnitSymbol);
            AssertHasCorrectComponent(parameters.WaveEnergyFunction, 
                                      WaveParametersConstants.SpreadingVariableName,
                                      SpreadingConversion.GetSpreadingUnit<TSpreading>().Name,
                                      SpreadingConversion.GetSpreadingUnit<TSpreading>().Symbol, 
                                      SpreadingConversion.GetSpreadingDefaultValue<TSpreading>());

            Assert.That(parameters.WaveEnergyFunction.Attributes[BcwFile.TimeFunctionAttributeName], 
                        Is.EqualTo(WaveParametersConstants.NonEquidistantTimeFunctionAttributeName));
            Assert.That(parameters.WaveEnergyFunction.Attributes[BcwFile.RefDateAttributeName], 
                        Is.EqualTo(new DateTime().ToString(BcwFile.DateFormatString)));
            Assert.That(parameters.WaveEnergyFunction.Attributes[BcwFile.TimeUnitAttributeName], 
                        Is.EqualTo(WaveParametersConstants.MinuteUnitName));
        }

        private static void AssertHasCorrectComponent(IFunction function,
                                                      string componentName,
                                                      string expectedUnitName,
                                                      string expectedUnitSymbol,
                                                      double? defaultValue = null)
        {
            IVariable component = function.Components
                                          .FirstOrDefault(c => c.Name == componentName);
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