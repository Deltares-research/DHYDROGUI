using System;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    [TestFixture]
    public class ForcingTypeDefinedParametersFactoryTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var factory = new ForcingTypeDefinedParametersFactory();

            // Assert
            Assert.That(factory, Is.InstanceOf<IForcingTypeDefinedParametersFactory>());
        }

        [Test]
        public void ConstructDefaultFileBasedParameters_ExpectedValues()
        {
            // Setup
            var factory = new ForcingTypeDefinedParametersFactory();

            // Call
            FileBasedParameters parameters =
                factory.ConstructDefaultFileBasedParameters();

            // Assert
            Assert.That(parameters.FilePath, Is.Empty);
        }

        [TestFixture]
        [TestFixture(typeof(DegreesDefinedSpreading))]
        [TestFixture(typeof(PowerDefinedSpreading))]
        public class GivenDefinedSpreadingType<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            [Test]
            public void ConstructDefaultConstantParameters_ExpectedValues()
            {
                // Setup
                var factory = new ForcingTypeDefinedParametersFactory();

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
                var factory = new ForcingTypeDefinedParametersFactory();

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
                var factory = new ForcingTypeDefinedParametersFactory();

                void Call() => factory.ConvertConstantParameters<TSpreading, PowerDefinedSpreading>(null);
                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("parameters"));
            }

            [Test]
            public void ConvertConstantParameters_PowerDefined_ExpectedResults()
            {
                // Setup
                var initialParameters = new ConstantParameters<TSpreading>(5, 6, 7, new TSpreading());
                var factory = new ForcingTypeDefinedParametersFactory();

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
                var factory = new ForcingTypeDefinedParametersFactory();

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
                var factory = new ForcingTypeDefinedParametersFactory();
                var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();

                // Call
                TimeDependentParameters<TSpreading> parameters = factory.ConstructTimeDependentParameters(waveEnergyFunction);

                // Assert
                Assert.That(parameters.WaveEnergyFunction, Is.SameAs(waveEnergyFunction),
                            $"Expected a different {nameof(TimeDependentParameters<TSpreading>.WaveEnergyFunction)}:");
            }

            [Test]
            public void ConstructTimeDependentParameters_WaveEnergyFunctionNull_ThrowsArgumentNullException()
            {
                // Setup
                var factory = new ForcingTypeDefinedParametersFactory();

                // Call | Assert
                void Call() => factory.ConstructTimeDependentParameters<TSpreading>(null);

                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("waveEnergyFunction"));
            }

            [Test]
            public void ConstructDefaultTimeDependentParameters_WaveEnergyFunctionNotNull()
            {
                // Setup
                var factory = new ForcingTypeDefinedParametersFactory();

                // Call
                TimeDependentParameters<TSpreading> parameters = factory.ConstructDefaultTimeDependentParameters<TSpreading>();

                // Assert
                Assert.That(parameters.WaveEnergyFunction, Is.Not.Null);
            }

            [Test]
            public void ConvertTimeDependentParameters_ParametersNull_ThrowsArgumentNullException()
            {
                var factory = new ForcingTypeDefinedParametersFactory();

                void Call() => factory.ConvertTimeDependentParameters<TSpreading, DegreesDefinedSpreading>(null);
                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("parameters"));
            }

            [Test]
            public void ConvertTimeDependentParameters_PowerDefined_ExpectedResults()
            {
                // Setup
                var energyFunction = new WaveEnergyFunction<TSpreading>();
                var initialParameters = new TimeDependentParameters<TSpreading>(energyFunction);
                var factory = new ForcingTypeDefinedParametersFactory();

                // Call
                TimeDependentParameters<PowerDefinedSpreading> result = factory.ConvertTimeDependentParameters<TSpreading, PowerDefinedSpreading>(initialParameters);

                // Assert
                Unit expectedUnit = SpreadingConversion.GetSpreadingUnit<PowerDefinedSpreading>();

                Assert.That(result.WaveEnergyFunction.SpreadingComponent.Unit.Name, Is.EqualTo(expectedUnit.Name));
                Assert.That(result.WaveEnergyFunction.SpreadingComponent.Unit.Symbol, Is.EqualTo(expectedUnit.Symbol));
            }

            [Test]
            public void ConvertTimeDependentParameters_DegreesDefined_ExpectedResults()
            {
                // Setup
                var energyFunction = new WaveEnergyFunction<TSpreading>();
                var initialParameters = new TimeDependentParameters<TSpreading>(energyFunction);
                var factory = new ForcingTypeDefinedParametersFactory();

                // Call
                TimeDependentParameters<DegreesDefinedSpreading> result = factory.ConvertTimeDependentParameters<TSpreading, DegreesDefinedSpreading>(initialParameters);

                // Assert
                Unit expectedUnit = SpreadingConversion.GetSpreadingUnit<DegreesDefinedSpreading>();

                Assert.That(result.WaveEnergyFunction.SpreadingComponent.Unit.Name, Is.EqualTo(expectedUnit.Name));
                Assert.That(result.WaveEnergyFunction.SpreadingComponent.Unit.Symbol, Is.EqualTo(expectedUnit.Symbol));
            }
        }
    }
}