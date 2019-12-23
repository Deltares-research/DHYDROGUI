using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.DataComponents
{
    [TestFixture]
    public class UniformDataTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var data = Substitute.For<IBoundaryConditionParameters>();

            // Call
            var uniformDataComponent = new UniformDataComponent(data);

            // Assert
            Assert.That(uniformDataComponent, Is.InstanceOf<IBoundaryConditionDataComponent>());
            Assert.That(uniformDataComponent.Data, Is.SameAs(data),
                        "Expected a different Data object:");
        }

        [Test]
        public void Constructor_DataNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new UniformDataComponent(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            
            Assert.That(exception.ParamName, Is.EqualTo("data"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void SetData_ValueNull_ThrowsArgumentNullException()
        {
            // Setup
            var data = Substitute.For<IBoundaryConditionParameters>();
            var uniformDataComponent = new UniformDataComponent(data);

            // Call
            void Call() => uniformDataComponent.Data = null;

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            
            Assert.That(exception.ParamName, Is.EqualTo("value"),
                        "Expected a different ParamName:");
        }
    }
}