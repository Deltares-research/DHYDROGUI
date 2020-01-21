using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.DataComponents
{
    [TestFixture(typeof(ConstantParameters))]
    public class UniformDataComponentTest<T> where T : class, IBoundaryConditionParameters
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var data = DataComponentTestUtils.ConstructParameters<T>();

            // Call
            var uniformDataComponent = new UniformDataComponent<T>(data);

            // Assert
            Assert.That(uniformDataComponent, Is.InstanceOf<IBoundaryConditionDataComponent>());
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
    }
}