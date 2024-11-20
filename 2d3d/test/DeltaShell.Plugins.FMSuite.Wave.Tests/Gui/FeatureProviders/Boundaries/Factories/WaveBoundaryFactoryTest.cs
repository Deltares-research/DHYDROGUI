using System;
using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Factories
{
    [TestFixture]
    public class WaveBoundaryFactoryTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factoryHelper = Substitute.For<IWaveBoundaryFactoryHelper>();
            var nameProvider = Substitute.For<IUniqueBoundaryNameProvider>();

            // Call
            var factory = new WaveBoundaryFactory(calculatorProvider,
                                                  factoryHelper,
                                                  nameProvider);

            // Assert
            Assert.That(factory, Is.InstanceOf(typeof(IWaveBoundaryFactory)),
                        $"Expected {typeof(WaveBoundaryFactory)} to implement {typeof(IWaveBoundaryFactory)}");
        }

        [Test]
        public void Constructor_SnappingCalculatorNull_ThrowsArgumentNullException()
        {
            // Setup
            var helper = Substitute.For<IWaveBoundaryFactoryHelper>();
            var nameProvider = Substitute.For<IUniqueBoundaryNameProvider>();

            // Call
            void Call() => new WaveBoundaryFactory(null, helper, nameProvider);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("snappingCalculatorProvider"));
        }

        [Test]
        public void Constructor_NameProviderNull_ThrowsArgumentNullException()
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var helper = Substitute.For<IWaveBoundaryFactoryHelper>();

            // Call
            void Call() => new WaveBoundaryFactory(calculatorProvider, helper, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("nameProvider"));
        }

        [Test]
        public void Constructor_FactoryHelperNull_ThrowsArgumentNullException()
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var nameProvider = Substitute.For<IUniqueBoundaryNameProvider>();

            // Call
            void Call() => new WaveBoundaryFactory(calculatorProvider, null, nameProvider);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("factoryHelper"));
        }

        [Test]
        public void ConstructWaveBoundary_GeometryNull_ThrowsArgumentNullException()
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var helper = Substitute.For<IWaveBoundaryFactoryHelper>();
            var nameProvider = Substitute.For<IUniqueBoundaryNameProvider>();

            var factory = new WaveBoundaryFactory(calculatorProvider, helper, nameProvider);

            // Call
            void Call() => factory.ConstructWaveBoundary(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("geometry"));
        }

        [Test]
        public void ConstructWaveBoundary_CorrectGeometricDefinition_ReturnsCorrectIWaveBoundary()
        {
            // Setup
            var calculator = Substitute.For<IBoundarySnappingCalculator>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            calculatorProvider.GetBoundarySnappingCalculator().Returns(calculator);

            var helper = Substitute.For<IWaveBoundaryFactoryHelper>();

            const string name = "BoundaryName";
            var nameProvider = Substitute.For<IUniqueBoundaryNameProvider>();
            nameProvider.GetUniqueName().Returns(name);

            var factory = new WaveBoundaryFactory(calculatorProvider, helper, nameProvider);

            var geometry = Substitute.For<ILineString>();
            var gridSide = random.NextEnumValue<GridSide>();

            var coordinates = new Coordinate[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(0.0, 20.0)
            };

            var snappedCoordinates = new List<GridBoundaryCoordinate>
            {
                new GridBoundaryCoordinate(gridSide, 0),
                new GridBoundaryCoordinate(gridSide, 5)
            };

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            geometry.Coordinates.Returns(coordinates);
            helper.GetSnappedEndPoints(calculator, coordinates).Returns(snappedCoordinates);
            helper.GetGeometricDefinition(snappedCoordinates, calculator).Returns(geometricDefinition);
            helper.GetConditionDefinition().Returns(conditionDefinition);

            // Call
            IWaveBoundary boundary = factory.ConstructWaveBoundary(geometry);

            // Assert
            Assert.That(boundary.GeometricDefinition, Is.SameAs(geometricDefinition));
            Assert.That(boundary.ConditionDefinition, Is.SameAs(conditionDefinition));
            Assert.That(boundary.Name, Is.EqualTo(name));

            helper.Received(1).GetSnappedEndPoints(calculator, coordinates);
            helper.Received(1).GetGeometricDefinition(snappedCoordinates, calculator);
            helper.Received(1).GetConditionDefinition();
            nameProvider.Received(1).GetUniqueName();
        }

        [Test]
        public void ConstructWaveBoundary_NullGeometricDefinition_ReturnsNull()
        {
            // Setup
            var calculator = Substitute.For<IBoundarySnappingCalculator>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();

            calculatorProvider.GetBoundarySnappingCalculator().Returns(calculator);

            var helper = Substitute.For<IWaveBoundaryFactoryHelper>();
            var nameProvider = Substitute.For<IUniqueBoundaryNameProvider>();

            var factory = new WaveBoundaryFactory(calculatorProvider, helper, nameProvider);

            var geometry = Substitute.For<ILineString>();
            var gridSide = random.NextEnumValue<GridSide>();

            var coordinates = new Coordinate[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(0.0, 20.0)
            };

            var snappedCoordinates = new List<GridBoundaryCoordinate>
            {
                new GridBoundaryCoordinate(gridSide, 0),
                new GridBoundaryCoordinate(gridSide, 5)
            };

            IWaveBoundaryGeometricDefinition geometricDefinition = null;

            geometry.Coordinates.Returns(coordinates);
            helper.GetSnappedEndPoints(calculator, coordinates).Returns(snappedCoordinates);
            helper.GetGeometricDefinition(snappedCoordinates, calculator).Returns(geometricDefinition);

            // Call
            IWaveBoundary boundary = factory.ConstructWaveBoundary(geometry);

            // Assert
            Assert.That(boundary, Is.Null);
            helper.Received(1).GetSnappedEndPoints(calculator, coordinates);
            helper.Received(1).GetGeometricDefinition(snappedCoordinates, calculator);
            helper.DidNotReceive().GetConditionDefinition();
            nameProvider.DidNotReceive().GetUniqueName();
        }
    }
}