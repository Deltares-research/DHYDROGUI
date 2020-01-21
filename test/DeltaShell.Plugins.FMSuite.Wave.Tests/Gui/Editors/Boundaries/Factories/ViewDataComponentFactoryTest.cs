using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
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
            // Setup 
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();

            // Call
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Assert
            Assert.That(factory, Is.InstanceOf<IViewDataComponentFactory>());
        }

        [Test]
        public void Constructor_DataComponentFactoryNull_ThrowsArgumentNullException()
        {
            void Call() => new ViewDataComponentFactory(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentFactory"));
        }

        [Test]
        public void GetForcingType_ReturnsConstant()
        {
            // Setup
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call
            ForcingViewType result = factory.GetForcingType(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(ForcingViewType.Constant));
        }

        [Test]
        public void GetForcingType_DataComponentNull_ThrowsArgumentNullException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            void Call() => factory.GetSpatialDefinition(dataComponent);

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetSpatialDefinition_DataComponentNull_ThrowsArgumentNullException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            void Call() => factory.GetSpatialDefinition(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        private static IEnumerable<TestCaseData> GetConstructParametersSettingsViewModelData()
        {
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters>(new ConstantParameters(0.0, 0.0, 0.0, 0.0)), typeof(UniformConstantParametersSettingsViewModel));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters>(), typeof(SpatiallyVariantConstantParametersSettingsViewModel));
        }

        [Test]
        [TestCaseSource(nameof(GetConstructParametersSettingsViewModelData))]
        public void ConstructParametersSettingsViewModel_ExpectedResults(IBoundaryConditionDataComponent dataComponent,
                                                                         Type expectedType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call
            IParametersSettingsViewModel viewModel = factory.ConstructParametersSettingsViewModel(dataComponent);

            // Assert
            Assert.That(viewModel, Is.InstanceOf(expectedType));
        }

        [Test]
        public void ConstructParametersSettingsViewModel_DataComponentNull_ThrowsArgumentNullException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            void Call() => factory.ConstructParametersSettingsViewModel(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        private class DummyDataComponent : IBoundaryConditionDataComponent { }

        [Test]
        public void ConstructParametersSettingsViewModel_UnsupportedType_ThrowsNotSupportedException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            void Call() => factory.ConstructParametersSettingsViewModel(new DummyDataComponent());

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_UniformConstant_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<ConstantParameters>(new ConstantParameters(0.0, 0.0, 0.0, 0.0));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters>>()
                                     .Returns(srcDataComponent);

            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.Constant, SpatialDefinitionViewType.Uniform);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_SpatiallyVariantConstant_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters>>()
                                     .Returns(srcDataComponent);

            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.Constant, SpatialDefinitionViewType.SpatiallyVarying);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters>>();
        }

        [Test]
        [TestCase(ForcingViewType.TimeSeries, SpatialDefinitionViewType.Uniform)]
        [TestCase(ForcingViewType.TimeSeries, SpatialDefinitionViewType.SpatiallyVarying)]
        [TestCase(ForcingViewType.FileBased, SpatialDefinitionViewType.Uniform)]
        [TestCase(ForcingViewType.FileBased, SpatialDefinitionViewType.SpatiallyVarying)]
        public void ConstructBoundaryConditionDataComponent_UnsupportedType_ThrowsNotSupportedException(ForcingViewType viewType,
                                                                                                        SpatialDefinitionViewType spatialDefinition)
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            void Call() => factory.ConstructBoundaryConditionDataComponent(viewType, spatialDefinition);
            Assert.Throws<NotSupportedException>(Call);
        }
    }
}