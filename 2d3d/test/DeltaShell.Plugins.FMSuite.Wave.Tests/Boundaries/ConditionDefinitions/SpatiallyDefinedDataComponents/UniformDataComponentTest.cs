using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    [TestFixture(typeof(ConstantParameters<PowerDefinedSpreading>))]
    [TestFixture(typeof(ConstantParameters<DegreesDefinedSpreading>))]
    public class UniformDataComponentTest<T> where T : class, IForcingTypeDefinedParameters
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var data = DataComponentTestUtils.ConstructParameters<T>();

            // Call
            var uniformDataComponent = new UniformDataComponent<T>(data);

            // Assert
            Assert.That(uniformDataComponent, Is.InstanceOf<ISpatiallyDefinedDataComponent>());
            Assert.That(uniformDataComponent.Data, Is.SameAs(data),
                        "Expected a different Data object:");
        }

        [Test]
        public void Constructor_DataNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new UniformDataComponent<T>(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("data"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void SetData_ValueNull_ThrowsArgumentNullException()
        {
            // Setup
            var data = DataComponentTestUtils.ConstructParameters<T>();
            var uniformDataComponent = new UniformDataComponent<T>(data);

            // Call
            void Call() => uniformDataComponent.Data = null;

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("value"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullExceptionForUniformDataComponent()
        {
            // Setup
            var data = DataComponentTestUtils.ConstructParameters<T>();
            var uniformDataComponent = new UniformDataComponent<T>(data);

            // Call
            void Call() => uniformDataComponent.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForUniformDataComponent()
        {
            // Setup
            var data = DataComponentTestUtils.ConstructParameters<T>();
            var uniformDataComponent = new UniformDataComponent<T>(data);
            var visitor = Substitute.For<ISpatiallyDefinedDataComponentVisitor>();

            // Call
            uniformDataComponent.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(uniformDataComponent);
        }
    }
}