using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Factories
{
    [TestFixture]
    public class ViewDataComponentFactoryTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var factory = new ViewDataComponentFactory();

            // Assert
            Assert.That(factory, Is.InstanceOf<IViewDataComponentFactory>());
        }

        [Test]
        public void GetForcingType_ReturnsConstant()
        {
            // Setup
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();
            var factory = new ViewDataComponentFactory();

            // Call
            ForcingViewType result = factory.GetForcingType(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(ForcingViewType.Constant));
        }

        [Test]
        public void GetForcingType_DataComponentNull_ThrowsArgumentNullException()
        {
            var factory = new ViewDataComponentFactory();

            void Call() => factory.GetForcingType(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        [Test]
        [TestCaseSource(nameof(GetSpatialDefinitionData))]
        public void GetSpatialDefinition_ValidData_ReturnsCorrectResults(IBoundaryConditionDataComponent dataComponent,
                                                                         SpatialDefinitionViewType expecteDefinitionViewType)
        {
            // Setup
            var factory = new ViewDataComponentFactory();

            // Call
            SpatialDefinitionViewType result = factory.GetSpatialDefinition(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expecteDefinitionViewType));
        }

        private static IEnumerable<TestCaseData> GetSpatialDefinitionData()
        {
            yield return  new TestCaseData(new UniformDataComponent<ConstantParameters>(new ConstantParameters(0.0, 0.0, 0.0, 0.0)), SpatialDefinitionViewType.Uniform);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters>(), SpatialDefinitionViewType.SpatiallyVarying);
        }

        [Test]
        public void GetSpatialDefinition_InvalidDataComponent_ThrowsNotSupportedException()
        { 
            var factory = new ViewDataComponentFactory();
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            void Call() => factory.GetSpatialDefinition(dataComponent);

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetSpatialDefinition_DataComponentNull_ThrowsArgumentNullException()
        {
            var factory = new ViewDataComponentFactory();

            void Call() => factory.GetSpatialDefinition(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        
    }
}