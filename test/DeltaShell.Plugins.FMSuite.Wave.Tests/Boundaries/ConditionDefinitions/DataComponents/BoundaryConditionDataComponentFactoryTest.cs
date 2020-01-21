using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.DataComponents
{
    [TestFixture]
    public class BoundaryConditionDataComponentFactoryTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var parameterFactory = Substitute.For<IBoundaryParametersFactory>();

            // Call
            var componentFactory = new BoundaryConditionDataComponentFactory(parameterFactory);

            // Assert
            Assert.That(componentFactory, Is.InstanceOf<IBoundaryConditionDataComponentFactory>());
        }

        [Test]
        public void Constructor_ParameterFactoryNull_ThrowsArgumentNullException()
        {
            void Call() => new BoundaryConditionDataComponentFactory(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameterFactory"));
        }

        [Test]
        public void ConstructDefaultDataComponent_UniformDataComponentWithConstantParameters_ExpectedResult()
        {
            // Setup
            var parameterFactory = new BoundaryParametersFactory();
            var componentFactory = new BoundaryConditionDataComponentFactory(parameterFactory);

            // Call
            var dataComponent = 
                componentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters>>();

            // Assert
            Assert.That(dataComponent, Is.Not.Null);
            Assert.That(dataComponent.Data, Is.Not.Null);
        }

        [Test]
        public void ConstructDefaultDataComponent_SpatiallyVaryingDataComponentWithConstantParameters_ExpectedResult()
        {
            // Setup
            var parameterFactory = new BoundaryParametersFactory();
            var componentFactory = new BoundaryConditionDataComponentFactory(parameterFactory);

            // Call
            var dataComponent = 
                componentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters>>();

            // Assert
            Assert.That(dataComponent, Is.Not.Null);
            Assert.That(dataComponent.Data, Is.Not.Null);
        }

        private class DummyParameters : IBoundaryConditionDataComponent { }

        [Test]
        public void ConstructDefaultDataComponent_NotValidType_ThrowsNotSupportedException()
        {
            // Setup
            var parameterFactory = Substitute.For<IBoundaryParametersFactory>();
            var componentFactory = new BoundaryConditionDataComponentFactory(parameterFactory);
            
            // Call
            void Call() => componentFactory.ConstructDefaultDataComponent<DummyParameters>();

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }
    }
}