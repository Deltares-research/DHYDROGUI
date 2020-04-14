using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
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
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            // Call
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Assert
            Assert.That(factory, Is.InstanceOf<IViewDataComponentFactory>());
        }

        [Test]
        public void Constructor_DataComponentFactoryNull_ThrowsArgumentNullException()
        {
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            void Call() => new ViewDataComponentFactory(null, referenceDateProvider);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentFactory"));
        }

        [Test]
        public void Constructor_ReferenceDateTimeProviderNull_ThrowsArgumentNullException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            void Call() => new ViewDataComponentFactory(modelDataComponentFactory, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("referenceDateTimeProvider"));
        }

        private static IEnumerable<TestCaseData> GetForcingTypeData()
        {
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading())), ForcingViewType.Constant);
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading())), ForcingViewType.Constant);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), ForcingViewType.Constant);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), ForcingViewType.Constant);

            var powerDefinedFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var degreesDefinedFunction = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(powerDefinedFunction)), ForcingViewType.TimeSeries);
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(degreesDefinedFunction)), ForcingViewType.TimeSeries);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(), ForcingViewType.TimeSeries);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(), ForcingViewType.TimeSeries);

            yield return new TestCaseData(new UniformDataComponent<FileBasedParameters>(new FileBasedParameters("path")), ForcingViewType.FileBased);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), ForcingViewType.FileBased);
        }

        [Test]
        [TestCaseSource(nameof(GetForcingTypeData))]
        public void GetForcingType_ReturnsExpectedResult(IBoundaryConditionDataComponent dataComponent, ForcingViewType expectedResult)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ForcingViewType result = factory.GetForcingType(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetForcingType_DataComponentNull_ThrowsArgumentNullException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            void Call() => factory.GetForcingType(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        [Test]
        public void GetForcingType_UnsupportedDataComponentType_ThrowsNotSupportedException()
        {
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            void Call() => factory.GetForcingType(dataComponent);
            
            Assert.Throws<NotSupportedException>(Call);
        }

        private static IEnumerable<TestCaseData> GetSpatialDefinitionData()
        {
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading())), SpatialDefinitionViewType.Uniform);
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading())), SpatialDefinitionViewType.Uniform);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), SpatialDefinitionViewType.SpatiallyVarying);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), SpatialDefinitionViewType.SpatiallyVarying);

            var powerDefinedFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var degreesDefinedFunction = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(powerDefinedFunction)), SpatialDefinitionViewType.Uniform);
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(degreesDefinedFunction)), SpatialDefinitionViewType.Uniform);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(), SpatialDefinitionViewType.SpatiallyVarying);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(), SpatialDefinitionViewType.SpatiallyVarying);
        }


        [Test]
        [TestCaseSource(nameof(GetSpatialDefinitionData))]
        public void GetSpatialDefinition_ValidData_ReturnsCorrectResults(IBoundaryConditionDataComponent dataComponent,
                                                                         SpatialDefinitionViewType expectedDefinitionViewType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            SpatialDefinitionViewType result = factory.GetSpatialDefinition(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDefinitionViewType));
        }

        [Test]
        public void GetSpatialDefinition_InvalidDataComponent_ThrowsNotSupportedException()
        { 
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            void Call() => factory.GetSpatialDefinition(dataComponent);

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetSpatialDefinition_DataComponentNull_ThrowsArgumentNullException()
        {
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            void Call() => factory.GetSpatialDefinition(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        private static IEnumerable<TestCaseData> GetConstructParametersSettingsViewModelData()
        {
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading())), typeof(UniformConstantParametersSettingsViewModel<PowerDefinedSpreading>));
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading())), typeof(UniformConstantParametersSettingsViewModel<DegreesDefinedSpreading>));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), typeof(SpatiallyVariantConstantParametersSettingsViewModel<PowerDefinedSpreading>));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), typeof(SpatiallyVariantConstantParametersSettingsViewModel<DegreesDefinedSpreading>));

            var powerDefinedFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var degreesDefinedFunction = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(powerDefinedFunction)), typeof(UniformTimeDependentParametersSettingsViewModel<PowerDefinedSpreading>));
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(degreesDefinedFunction)), typeof(UniformTimeDependentParametersSettingsViewModel<DegreesDefinedSpreading>));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(), typeof(SpatiallyVariantTimeDependentParametersSettingsViewModel<PowerDefinedSpreading>));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(), typeof(SpatiallyVariantTimeDependentParametersSettingsViewModel<DegreesDefinedSpreading>));
           
            yield return new TestCaseData(new UniformDataComponent<FileBasedParameters>(new FileBasedParameters("path")), typeof(UniformFileBasedParametersSettingsViewModel));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), typeof(SpatiallyVariantFileBasedParametersSettingsViewModel));
        }

        [Test]
        [TestCaseSource(nameof(GetConstructParametersSettingsViewModelData))]
        public void ConstructParametersSettingsViewModel_ExpectedResults(IBoundaryConditionDataComponent dataComponent,
                                                                         Type expectedType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IParametersSettingsViewModel viewModel = factory.ConstructParametersSettingsViewModel(dataComponent);

            // Assert
            Assert.That(viewModel, Is.InstanceOf(expectedType));
        }

        [Test]
        public void ConstructParametersSettingsViewModel_DataComponentNull_ThrowsArgumentNullException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            void Call() => factory.ConstructParametersSettingsViewModel(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        private class DummyDataComponent : IBoundaryConditionDataComponent {
            public void AcceptVisitor(IDataComponentVisitor boundaryConditionVisitor)
            {
            }
        }

        [Test]
        public void ConstructParametersSettingsViewModel_UnsupportedType_ThrowsNotSupportedException()
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            void Call() => factory.ConstructParametersSettingsViewModel(new DummyDataComponent());

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_UniformConstantPower_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading()));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.Constant, 
                                                                SpatialDefinitionViewType.Uniform,
                                                                DirectionalSpreadingViewType.Power);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_UniformTimeDependentPower_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>()));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.TimeSeries, 
                                                                SpatialDefinitionViewType.Uniform,
                                                                DirectionalSpreadingViewType.Power);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_UniformConstantDegrees_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading()));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.Constant, 
                                                                SpatialDefinitionViewType.Uniform,
                                                                DirectionalSpreadingViewType.Degrees);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_UniformTimeDependentDegrees_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>()));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.TimeSeries, 
                                                                SpatialDefinitionViewType.Uniform,
                                                                DirectionalSpreadingViewType.Degrees);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>();
        }

        [Test]
        [TestCase(DirectionalSpreadingViewType.Degrees)]
        [TestCase(DirectionalSpreadingViewType.Power)]
        public void ConstructBoundaryConditionDataComponent_UniformFileBased_ExpectedResults(DirectionalSpreadingViewType directionalSpreading)
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<FileBasedParameters>(new FileBasedParameters("path"));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<FileBasedParameters>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.FileBased, 
                                                                SpatialDefinitionViewType.Uniform,
                                                                directionalSpreading);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<UniformDataComponent<FileBasedParameters>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_SpatiallyVariantConstantDegrees_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.Constant, 
                                                                SpatialDefinitionViewType.SpatiallyVarying,
                                                                DirectionalSpreadingViewType.Degrees);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_SpatiallyVariantTimeDependentDegrees_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.TimeSeries, 
                                                                SpatialDefinitionViewType.SpatiallyVarying,
                                                                DirectionalSpreadingViewType.Degrees);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_SpatiallyVariantConstantPower_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.Constant, 
                                                                SpatialDefinitionViewType.SpatiallyVarying,
                                                                DirectionalSpreadingViewType.Power);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>>();
        }

        [Test]
        public void ConstructBoundaryConditionDataComponent_SpatiallyVariantTimeDependentPower_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.TimeSeries, 
                                                                SpatialDefinitionViewType.SpatiallyVarying,
                                                                DirectionalSpreadingViewType.Power);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>();
        }

        [Test]
        [TestCase(DirectionalSpreadingViewType.Degrees)]
        [TestCase(DirectionalSpreadingViewType.Power)]
        public void ConstructBoundaryConditionDataComponent_SpatiallyVaryingFileBased_ExpectedResults(DirectionalSpreadingViewType directionalSpreading)
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<FileBasedParameters>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent dataComponent = 
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.FileBased, 
                                                                SpatialDefinitionViewType.SpatiallyVarying,
                                                                directionalSpreading);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<FileBasedParameters>>();
        }


        private static IEnumerable<TestCaseData> GetDirectionalSpreadingViewTypeData()
        {
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading())),
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0, 0, 0, new DegreesDefinedSpreading())),
                                          DirectionalSpreadingViewType.Degrees);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), 
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), 
                                          DirectionalSpreadingViewType.Degrees);

            var powerDefinedFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var degreesDefinedFunction = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();
            yield return  new TestCaseData(new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(powerDefinedFunction)), 
                                           DirectionalSpreadingViewType.Power);
            yield return  new TestCaseData(new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(degreesDefinedFunction)),
                                           DirectionalSpreadingViewType.Degrees);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(), 
                                           DirectionalSpreadingViewType.Power);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(), 
                                           DirectionalSpreadingViewType.Degrees);

            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), 
                                           DirectionalSpreadingViewType.Power);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), 
                                           DirectionalSpreadingViewType.Power);
        }

        [Test]
        [TestCaseSource(nameof(GetDirectionalSpreadingViewTypeData))]
        public void GetDirectionalSpreadingViewType_ExpectedResults(IBoundaryConditionDataComponent dataComponent, 
                                                                    DirectionalSpreadingViewType expectedDirectionalSpreadingViewType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            DirectionalSpreadingViewType result = factory.GetDirectionalSpreadingViewType(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDirectionalSpreadingViewType));
        }

        [Test]
        public void GetDirectionalSpreadingViewType_UnsupportedDataComponent_ThrowsNotSupportedException()
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call | Assert
            void Call() => factory.GetDirectionalSpreadingViewType(new DummyDataComponent());
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetDirectionalSpreadingViewType_DataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call | Assert
            void Call() => factory.GetDirectionalSpreadingViewType(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        private static IEnumerable<TestCaseData> GetAreBoundaryWideParametersVisibleData()
        {
            yield return new TestCaseData(Substitute.For<IBoundaryConditionDataComponent>(), true);

            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), false);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), false);
        }

        [Test]
        [TestCaseSource(nameof(GetAreBoundaryWideParametersVisibleData))]
        public void GetAreBoundaryWideParametersVisible_ExpectedResults(IBoundaryConditionDataComponent dataComponent, bool expectedVisibility)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            bool result = factory.GetAreBoundaryWideParametersVisible(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedVisibility));
        }

        [Test]
        public void GetAreBoundaryWideParametersVisible_DataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call | Assert
            void Call() => factory.GetAreBoundaryWideParametersVisible(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        private static IEnumerable<TestCaseData> ConvertBoundaryConditionDataComponentSpreadingTypeSameTypeData()
        {
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading())), 
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0, 0, 0, new DegreesDefinedSpreading())), 
                                          DirectionalSpreadingViewType.Degrees);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), 
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), 
                                          DirectionalSpreadingViewType.Degrees);

            var powerDefinedFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(powerDefinedFunction)), 
                                          DirectionalSpreadingViewType.Power);

            var degreesDefinedFunction = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(degreesDefinedFunction)), 
                                          DirectionalSpreadingViewType.Degrees);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(), 
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(), 
                                          DirectionalSpreadingViewType.Degrees);

            yield return new TestCaseData(new UniformDataComponent<FileBasedParameters>(new FileBasedParameters("path")), 
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), 
                                          DirectionalSpreadingViewType.Power);
        }

        [Test]
        [TestCaseSource(nameof(ConvertBoundaryConditionDataComponentSpreadingTypeSameTypeData))]
        public void ConvertBoundaryConditionDataComponentSpreadingType_NewSpreadingTypeEqualsDataComponentType_ReturnsDataComponent(IBoundaryConditionDataComponent dataComponent,
                                                                                                                                    DirectionalSpreadingViewType spreadingType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            IBoundaryConditionDataComponent result = 
                factory.ConvertBoundaryConditionDataComponentSpreadingType(dataComponent, spreadingType);

            // Assert
            Assert.That(result, Is.SameAs(dataComponent));
        }

        private static IEnumerable<TestCaseData> ConvertBoundaryConditionDataComponentSpreadingTypeDifferentTypeData()
        {
            var uniformConstantDataDegrees =
                new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(
                    new ConstantParameters<DegreesDefinedSpreading>(0, 0, 0, new DegreesDefinedSpreading()));
            var uniformConstantDataPower =
                    new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(
                        new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading()));

            var spatVariantConstantDegrees = new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>();
            var spatVariantConstantPower = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();

            IBoundaryConditionDataComponent FuncDegreeToPowerUniform(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(uniformConstantDataPower);
            yield return new TestCaseData(uniformConstantDataPower,
                                          uniformConstantDataDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncDegreeToPowerUniform);

            IBoundaryConditionDataComponent FuncPowerToDegreeUniform(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(uniformConstantDataDegrees);
            yield return new TestCaseData(uniformConstantDataDegrees,
                                          uniformConstantDataPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncPowerToDegreeUniform);

            IBoundaryConditionDataComponent FuncDegreeToPowerSpat(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(spatVariantConstantPower);
            yield return new TestCaseData(spatVariantConstantPower,
                                          spatVariantConstantDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncDegreeToPowerSpat);

            IBoundaryConditionDataComponent FuncPowerToDegreeSpat(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(spatVariantConstantDegrees);
            yield return new TestCaseData(spatVariantConstantDegrees,
                                          spatVariantConstantPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncPowerToDegreeSpat);

            var uniformTimeDependentDataDegrees =
                new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(
                    new TimeDependentParameters<DegreesDefinedSpreading>(Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>()));
            var uniformTimeDependentDataPower =
                    new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(
                        new TimeDependentParameters<PowerDefinedSpreading>(Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>()));

            var spatVariantTimeDependentDegrees = new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>();
            var spatVariantTimeDependentPower = new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>();

            IBoundaryConditionDataComponent FuncDegreeToPowerUniformTimeDependent(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(uniformTimeDependentDataPower);
            yield return new TestCaseData(uniformTimeDependentDataPower,
                                          uniformTimeDependentDataDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncDegreeToPowerUniformTimeDependent);

            IBoundaryConditionDataComponent FuncPowerToDegreeUniformTimeDependent(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(uniformTimeDependentDataDegrees);
            yield return new TestCaseData(uniformTimeDependentDataDegrees,
                                          uniformTimeDependentDataPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncPowerToDegreeUniformTimeDependent);

            IBoundaryConditionDataComponent FuncDegreeToPowerSpatTimeDependent(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(spatVariantTimeDependentPower);
            yield return new TestCaseData(spatVariantTimeDependentPower,
                                          spatVariantTimeDependentDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncDegreeToPowerSpatTimeDependent);

            IBoundaryConditionDataComponent FuncPowerToDegreeSpatTimeDependent(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(spatVariantTimeDependentDegrees);
            yield return new TestCaseData(spatVariantTimeDependentDegrees,
                                          spatVariantTimeDependentPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncPowerToDegreeSpatTimeDependent);
        }

        [Test]
        [TestCaseSource(nameof(ConvertBoundaryConditionDataComponentSpreadingTypeDifferentTypeData))]
        public void ConvertBoundaryConditionDataComponentSpreadingType_NewSpreadingTypeNotEqualsDataComponentType_ReturnsDataComponent(IBoundaryConditionDataComponent inputDataComponent,
                                                                                                                                       IBoundaryConditionDataComponent outputDataComponent,
                                                                                                                                       DirectionalSpreadingViewType spreadingType,
                                                                                                                                       Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent> convertDataComponentSpreadingCall)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            convertDataComponentSpreadingCall(modelDataComponentFactory).Returns(outputDataComponent);

            // Call
            IBoundaryConditionDataComponent result = 
                factory.ConvertBoundaryConditionDataComponentSpreadingType(inputDataComponent, spreadingType);

            // Assert
            Assert.That(result, Is.SameAs(outputDataComponent));
            convertDataComponentSpreadingCall(modelDataComponentFactory.Received(1));
        }

        [Test]
        public void ConvertBoundaryConditionDataComponentSpreadingType_UnsupportedDataComponent_ThrowsNotSupportedException()
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call | Assert
            void Call() => factory.ConvertBoundaryConditionDataComponentSpreadingType(new DummyDataComponent(), 
                                                                                      DirectionalSpreadingViewType.Degrees);
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void ConvertBoundaryConditionDataComponentSpreadingType_CurrentDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call | Assert
            void Call() => factory.ConvertBoundaryConditionDataComponentSpreadingType(null, DirectionalSpreadingViewType.Degrees);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("currentDataComponent"));
        }
    }
}