using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class ImportBoundaryConditionDataComponentFactoryTest<T> where T : class, IBoundaryConditionSpreading, new()
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ParametersFactoryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new ImportBoundaryConditionDataComponentFactory(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parametersFactory"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();

            // Call
            void Call() => new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void CreateUniformConstantComponent_ParametersBlockNull_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var constantParameters = new ConstantParameters<T>(random.NextDouble(),
                                                               random.NextDouble(),
                                                               random.NextDouble(),
                                                               new T());
            parametersFactory.ConstructDefaultConstantParameters<T>().Returns(constantParameters);

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            UniformDataComponent<ConstantParameters<T>> result = factory.CreateUniformConstantComponent<T>(null);

            // Assert
            Assert.That(result.Data, Is.SameAs(constantParameters));
        }

        [Test]
        public void CreateUniformConstantComponent_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();

            ConstantParameters<T> constantParameters = GetExpectedConstantParameters(parametersFactory,
                                                                                     out ParametersBlock parametersBlock);

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            UniformDataComponent<ConstantParameters<T>> result = factory.CreateUniformConstantComponent<T>(parametersBlock);

            // Assert
            Assert.That(result.Data, Is.SameAs(constantParameters));
        }

        [Test]
        public void CreateUniformTimeDependentComponent_WaveEnergyFunctionNull_ThrowsArgumentNullException()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            void Call() => factory.CreateUniformTimeDependentComponent<T>(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveEnergyFunction"));
        }

        [Test]
        public void CreateUniformTimeDependentComponent_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();

            TimeDependentParameters<T> timeDependentParameters = GetExpectedTimeDependentParameters(
                parametersFactory, out WaveEnergyFunction<T> waveEnergyFunction);

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            UniformDataComponent<TimeDependentParameters<T>> result = factory.CreateUniformTimeDependentComponent(waveEnergyFunction);

            // Assert
            Assert.That(result.Data, Is.SameAs(timeDependentParameters));
        }

        [Test]
        public void CreateSpatiallyVaryingConstantComponent_DataPerSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            void Call() => factory.CreateSpatiallyVaryingConstantComponent<T>(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataPerSupportPoint"));
        }

        [Test]
        public void CreateSpatiallyVaryingConstantComponent_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(random.NextDouble(), geometricDefinition);
            ConstantParameters<T> constantParameters1 = GetExpectedConstantParameters(parametersFactory,
                                                                                      out ParametersBlock parametersBlock1);

            var supportPoint2 = new SupportPoint(random.NextDouble(), geometricDefinition);
            ConstantParameters<T> constantParameters2 = GetExpectedConstantParameters(parametersFactory,
                                                                                      out ParametersBlock parametersBlock2);

            Tuple<SupportPoint, ParametersBlock>[] dataPerSupportPoint =
            {
                new Tuple<SupportPoint, ParametersBlock>(supportPoint1, parametersBlock1),
                new Tuple<SupportPoint, ParametersBlock>(supportPoint2, parametersBlock2),
            };

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            SpatiallyVaryingDataComponent<ConstantParameters<T>> result = factory.CreateSpatiallyVaryingConstantComponent<T>(dataPerSupportPoint);

            // Assert
            Assert.That(result.Data[supportPoint1], Is.SameAs(constantParameters1));
            Assert.That(result.Data[supportPoint2], Is.SameAs(constantParameters2));
        }

        [Test]
        public void CreateSpatiallyVaryingTimeDependentComponent_DataPerSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            void Call() => factory.CreateSpatiallyVaryingTimeDependentComponent<T>(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataPerSupportPoint"));
        }

        [Test]
        public void CreateSpatiallyVaryingTimeDependentComponent_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(random.NextDouble(), geometricDefinition);
            TimeDependentParameters<T> timeDependentParameters1 = GetExpectedTimeDependentParameters(
                parametersFactory, out WaveEnergyFunction<T> waveEnergyFunction1);

            var supportPoint2 = new SupportPoint(random.NextDouble(), geometricDefinition);
            TimeDependentParameters<T> timeDependentParameters2 = GetExpectedTimeDependentParameters(
                parametersFactory, out WaveEnergyFunction<T> waveEnergyFunction2);

            Tuple<SupportPoint, IWaveEnergyFunction<T>>[] dataPerSupportPoint =
            {
                new Tuple<SupportPoint, IWaveEnergyFunction<T>>(supportPoint1, waveEnergyFunction1),
                new Tuple<SupportPoint, IWaveEnergyFunction<T>>(supportPoint2, waveEnergyFunction2),
            };

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            SpatiallyVaryingDataComponent<TimeDependentParameters<T>> result =
                factory.CreateSpatiallyVaryingTimeDependentComponent(dataPerSupportPoint);

            // Assert
            Assert.That(result.Data[supportPoint1], Is.SameAs(timeDependentParameters1));
            Assert.That(result.Data[supportPoint2], Is.SameAs(timeDependentParameters2));
        }

        private ConstantParameters<T> GetExpectedConstantParameters(IBoundaryParametersFactory parametersFactory,
                                                                    out ParametersBlock parametersBlock)
        {
            double waveHeight = random.NextDouble();
            double period = random.NextDouble();
            double direction = random.NextDouble();
            double spreading = random.NextDouble();

            var constantParameters = new ConstantParameters<T>(waveHeight, period, direction, new T());

            parametersFactory.ConstructConstantParameters(waveHeight, period, direction, Arg.Any<T>())
                             .Returns(constantParameters);

            parametersBlock = new ParametersBlock(waveHeight, period, direction, spreading);

            return constantParameters;
        }

        private static TimeDependentParameters<T> GetExpectedTimeDependentParameters(
            IBoundaryParametersFactory parametersFactory, out WaveEnergyFunction<T> waveEnergyFunction)
        {
            waveEnergyFunction = new WaveEnergyFunction<T>();
            var timeDependentParameters = new TimeDependentParameters<T>(waveEnergyFunction);

            parametersFactory.ConstructTimeDependentParameters(waveEnergyFunction)
                             .Returns(timeDependentParameters);
            return timeDependentParameters;
        }
    }
}