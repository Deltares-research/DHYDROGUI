using System;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions
{
    [TestFixture]
    public class WaveBoundaryConditionDefinitionTest
    {
        private readonly Random random = new Random(37);

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            // Call
            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          directionalSpreadingType,
                                                                          dataComponent);

            // Assert
            Assert.That(conditionDefinition, Is.InstanceOf<IWaveBoundaryConditionDefinition>());

            Assert.That(conditionDefinition.Shape, Is.SameAs(shape), 
                        "Expected a different Shape:");
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(periodType), 
                        "Expected a different PeriodType:");
            Assert.That(conditionDefinition.DirectionalSpreadingType, Is.EqualTo(directionalSpreadingType), 
                        "Expected a different DirectionalSpreadingType:");
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(dataComponent), 
                        "Expected a different DataComponent:");
        }

        [Test]
        public void Constructor_ShapeNull_ThrowsArgumentNullException()
        {
            // Setup
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            // Call
            void Call() => new WaveBoundaryConditionDefinition(null,
                                                               periodType,
                                                               directionalSpreadingType,
                                                               dataComponent);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("shape"), 
                        "Expected a different value for ParamName:");
        }

        [Test]
        public void Constructor_DataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();

            // Call
            void Call() => new WaveBoundaryConditionDefinition(shape,
                                                               periodType,
                                                               directionalSpreadingType,
                                                               null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"), 
                        "Expected a different value for ParamName:");
        }

        [Test]
        public void Constructor_PeriodTypeUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            const BoundaryConditionPeriodType periodType = (BoundaryConditionPeriodType) 99;
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            // Call
            void Call() => new WaveBoundaryConditionDefinition(shape,
                                                               periodType,
                                                               directionalSpreadingType,
                                                               dataComponent);

            // Assert
            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void Constructor_DirectionalSpreadingTypeUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            const BoundaryConditionDirectionalSpreadingType directionalSpreadingType = (BoundaryConditionDirectionalSpreadingType) 99;
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            // Call
            void Call() => new WaveBoundaryConditionDefinition(shape,
                                                               periodType,
                                                               directionalSpreadingType,
                                                               dataComponent);

            // Assert
            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void Shape_SetNull_ThrowsArgumentNullException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          directionalSpreadingType,
                                                                          dataComponent);

            // Call
            void Call() => conditionDefinition.Shape = null;

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("value"), 
                        "Expected a different value for ParamName:");
        }

        [Test]
        public void PeriodType_UndefinedEnum_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          directionalSpreadingType,
                                                                          dataComponent);

            // Call
            void Call() => conditionDefinition.PeriodType = (BoundaryConditionPeriodType) 99;

            // Assert
            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void DirectionalSpreadingType_UndefinedEnum_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          directionalSpreadingType,
                                                                          dataComponent);

            // Call
            void Call() => conditionDefinition.DirectionalSpreadingType = (BoundaryConditionDirectionalSpreadingType) 99;

            // Assert
            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void DataComponent_SetNull_ThrowsArgumentNullException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var directionalSpreadingType = random.NextEnumValue<BoundaryConditionDirectionalSpreadingType>();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          directionalSpreadingType,
                                                                          dataComponent);

            // Call
            void Call() => conditionDefinition.DataComponent= null;

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("value"), 
                        "Expected a different value for ParamName:");
        }
    }
}