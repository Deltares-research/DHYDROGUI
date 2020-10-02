using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
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
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            // Call
            void Call() => new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void CreateUniformConstantComponent_ParametersBlockNull_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
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
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

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
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
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
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            TimeDependentParameters<T> timeDependentParameters = GetExpectedTimeDependentParameters(
                parametersFactory, out IWaveEnergyFunction<T> waveEnergyFunction);

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            UniformDataComponent<TimeDependentParameters<T>> result = factory.CreateUniformTimeDependentComponent(waveEnergyFunction);

            // Assert
            Assert.That(result.Data, Is.SameAs(timeDependentParameters));
        }

        [Test]
        public void CreateUniformFileBasedComponent_ParametersBlockNull_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var fileBasedParameters = new FileBasedParameters("file path");
            parametersFactory.ConstructDefaultFileBasedParameters().Returns(fileBasedParameters);

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            UniformDataComponent<FileBasedParameters> result = factory.CreateUniformFileBasedComponent(null);

            // Assert
            Assert.That(result.Data, Is.SameAs(fileBasedParameters));
        }

        [Test]
        public void CreateUniformFileBasedComponent_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            const string filePath = "test file path";

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            UniformDataComponent<FileBasedParameters> result = factory.CreateUniformFileBasedComponent(filePath);

            // Assert
            Assert.That(result.Data.FilePath, Is.SameAs(filePath));
        }

        [Test]
        public void CreateSpatiallyVaryingConstantComponent_DataPerSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
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
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
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
                new Tuple<SupportPoint, ParametersBlock>(supportPoint2, parametersBlock2)
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
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
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
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(random.NextDouble(), geometricDefinition);
            TimeDependentParameters<T> timeDependentParameters1 = GetExpectedTimeDependentParameters(
                parametersFactory, out IWaveEnergyFunction<T> waveEnergyFunction1);

            var supportPoint2 = new SupportPoint(random.NextDouble(), geometricDefinition);
            TimeDependentParameters<T> timeDependentParameters2 = GetExpectedTimeDependentParameters(
                parametersFactory, out IWaveEnergyFunction<T> waveEnergyFunction2);

            Tuple<SupportPoint, IWaveEnergyFunction<T>>[] dataPerSupportPoint =
            {
                new Tuple<SupportPoint, IWaveEnergyFunction<T>>(supportPoint1, waveEnergyFunction1),
                new Tuple<SupportPoint, IWaveEnergyFunction<T>>(supportPoint2, waveEnergyFunction2)
            };

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            SpatiallyVaryingDataComponent<TimeDependentParameters<T>> result =
                factory.CreateSpatiallyVaryingTimeDependentComponent(dataPerSupportPoint);

            // Assert
            Assert.That(result.Data[supportPoint1], Is.SameAs(timeDependentParameters1));
            Assert.That(result.Data[supportPoint2], Is.SameAs(timeDependentParameters2));
        }

        [Test]
        public void CreateSpatiallyVaryingFileBasedComponent_DataPerSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            void Call() => factory.CreateSpatiallyVaryingFileBasedComponent(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataPerSupportPoint"));
        }

        [Test]
        public void CreateSpatiallyVaryingFileBasedComponent_ReturnsCorrectResult()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(random.NextDouble(), geometricDefinition);
            const string filePath1 = "file path 1";

            var supportPoint2 = new SupportPoint(random.NextDouble(), geometricDefinition);
            const string filePath2 = "file path 2";

            Tuple<SupportPoint, string>[] dataPerSupportPoint =
            {
                new Tuple<SupportPoint, string>(supportPoint1, filePath1),
                new Tuple<SupportPoint, string>(supportPoint2, filePath2)
            };

            var factory = new ImportBoundaryConditionDataComponentFactory(parametersFactory);

            // Call
            SpatiallyVaryingDataComponent<FileBasedParameters> result = factory.CreateSpatiallyVaryingFileBasedComponent(dataPerSupportPoint);

            // Assert
            Assert.That(result.Data[supportPoint1].FilePath, Is.SameAs(filePath1));
            Assert.That(result.Data[supportPoint2].FilePath, Is.SameAs(filePath2));
        }

        private ConstantParameters<T> GetExpectedConstantParameters(IForcingTypeDefinedParametersFactory parametersFactory,
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
            IForcingTypeDefinedParametersFactory parametersFactory, out IWaveEnergyFunction<T> waveEnergyFunction)
        {
            waveEnergyFunction = Substitute.For<IWaveEnergyFunction<T>>();
            var timeDependentParameters = new TimeDependentParameters<T>(waveEnergyFunction);

            parametersFactory.ConstructTimeDependentParameters(waveEnergyFunction)
                             .Returns(timeDependentParameters);
            return timeDependentParameters;
        }
    }
}