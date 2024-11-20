using System;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
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
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            // Call
            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          dataComponent);

            // Assert
            Assert.That(conditionDefinition, Is.InstanceOf<IWaveBoundaryConditionDefinition>());

            Assert.That(conditionDefinition.Shape, Is.SameAs(shape),
                        "Expected a different Shape:");
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(periodType),
                        "Expected a different PeriodType:");
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(dataComponent),
                        "Expected a different DataComponent:");
        }

        [Test]
        public void Constructor_ShapeNull_ThrowsArgumentNullException()
        {
            // Setup
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            // Call
            void Call() => new WaveBoundaryConditionDefinition(null,
                                                               periodType,
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

            // Call
            void Call() => new WaveBoundaryConditionDefinition(shape,
                                                               periodType,
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
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            // Call
            void Call() => new WaveBoundaryConditionDefinition(shape,
                                                               periodType,
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
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
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
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          dataComponent);

            // Call
            void Call() => conditionDefinition.PeriodType = (BoundaryConditionPeriodType) 99;

            // Assert
            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void DataComponent_SetNull_ThrowsArgumentNullException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          dataComponent);

            // Call
            void Call() => conditionDefinition.DataComponent = null;

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("value"),
                        "Expected a different value for ParamName:");
        }

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullException()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          dataComponent);

            // Call
            void Call() => conditionDefinition.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForConditionDefinition()
        {
            // Setup
            var shape = Substitute.For<IBoundaryConditionShape>();
            var periodType = random.NextEnumValue<BoundaryConditionPeriodType>();
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            var visitor = Substitute.For<IBoundaryConditionVisitor>();

            var conditionDefinition = new WaveBoundaryConditionDefinition(shape,
                                                                          periodType,
                                                                          dataComponent);

            // Call
            conditionDefinition.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(conditionDefinition);
        }
    }
}