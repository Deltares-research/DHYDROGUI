using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            void Call() => new ViewDataComponentFactory(modelDataComponentFactory, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("referenceDateTimeProvider"));
        }

        [Test]
        [TestCaseSource(nameof(GetConstructParametersSettingsViewModelData))]
        public void ConstructParametersSettingsViewModel_ExpectedResults(ISpatiallyDefinedDataComponent dataComponent,
                                                                         Type expectedType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            void Call() => factory.ConstructParametersSettingsViewModel(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        [Test]
        public void ConstructParametersSettingsViewModel_UnsupportedType_ThrowsNotSupportedException()
        {
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<FileBasedParameters>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<FileBasedParameters>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent dataComponent =
                factory.ConstructBoundaryConditionDataComponent(ForcingViewType.FileBased,
                                                                SpatialDefinitionViewType.SpatiallyVarying,
                                                                directionalSpreading);

            // Assert
            Assert.That(dataComponent, Is.SameAs(srcDataComponent));
            modelDataComponentFactory
                .Received(1)
                .ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<FileBasedParameters>>();
        }

        [Test]
        [TestCase(ForcingViewType.Constant)]
        [TestCase(ForcingViewType.TimeSeries)]
        [TestCase(ForcingViewType.FileBased)]
        public void ConstructBoundaryConditionDataComponent_UnsupportedData_ThrowsNotSupportedException(ForcingViewType forcingViewType)
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<FileBasedParameters>>()
                                     .Returns(srcDataComponent);

            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Assert
            void Call() => factory.ConstructBoundaryConditionDataComponent(forcingViewType,
                                                                           (SpatialDefinitionViewType) 99,
                                                                           DirectionalSpreadingViewType.Power);

            Assert.That(Call, Throws.InstanceOf<NotSupportedException>());
        }

        [Test]
        [TestCaseSource(nameof(ConvertBoundaryConditionDataComponentSpreadingTypeSameTypeData))]
        public void ConvertBoundaryConditionDataComponentSpreadingType_NewSpreadingTypeEqualsDataComponentType_ReturnsDataComponent(ISpatiallyDefinedDataComponent dataComponent,
                                                                                                                                    DirectionalSpreadingViewType spreadingType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call
            ISpatiallyDefinedDataComponent result =
                factory.ConvertBoundaryConditionDataComponentSpreadingType(dataComponent, spreadingType);

            // Assert
            Assert.That(result, Is.SameAs(dataComponent));
        }

        [Test]
        [TestCaseSource(nameof(ConvertBoundaryConditionDataComponentSpreadingTypeDifferentTypeData))]
        public void ConvertBoundaryConditionDataComponentSpreadingType_NewSpreadingTypeNotEqualsDataComponentType_ReturnsDataComponent(ISpatiallyDefinedDataComponent inputDataComponent,
                                                                                                                                       ISpatiallyDefinedDataComponent outputDataComponent,
                                                                                                                                       DirectionalSpreadingViewType spreadingType,
                                                                                                                                       Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent> convertDataComponentSpreadingCall)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            convertDataComponentSpreadingCall(modelDataComponentFactory).Returns(outputDataComponent);

            // Call
            ISpatiallyDefinedDataComponent result =
                factory.ConvertBoundaryConditionDataComponentSpreadingType(inputDataComponent, spreadingType);

            // Assert
            Assert.That(result, Is.SameAs(outputDataComponent));
            convertDataComponentSpreadingCall(modelDataComponentFactory.Received(1));
        }

        [Test]
        public void ConvertBoundaryConditionDataComponentSpreadingType_UnsupportedDataComponent_ThrowsNotSupportedException()
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
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
            var modelDataComponentFactory = Substitute.For<ISpatiallyDefinedDataComponentFactory>();
            var referenceDateProvider = Substitute.For<IReferenceDateTimeProvider>();

            var factory = new ViewDataComponentFactory(modelDataComponentFactory, referenceDateProvider);

            // Call | Assert
            void Call() => factory.ConvertBoundaryConditionDataComponentSpreadingType(null, DirectionalSpreadingViewType.Degrees);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("currentDataComponent"));
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

        private class DummyDataComponent : ISpatiallyDefinedDataComponent
        {
            public void AcceptVisitor(ISpatiallyDefinedDataComponentVisitor boundaryConditionVisitor) {}
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

            ISpatiallyDefinedDataComponent FuncDegreeToPowerUniform(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(uniformConstantDataPower);

            yield return new TestCaseData(uniformConstantDataPower,
                                          uniformConstantDataDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncDegreeToPowerUniform);

            ISpatiallyDefinedDataComponent FuncPowerToDegreeUniform(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(uniformConstantDataDegrees);

            yield return new TestCaseData(uniformConstantDataDegrees,
                                          uniformConstantDataPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncPowerToDegreeUniform);

            ISpatiallyDefinedDataComponent FuncDegreeToPowerSpat(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(spatVariantConstantPower);

            yield return new TestCaseData(spatVariantConstantPower,
                                          spatVariantConstantDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncDegreeToPowerSpat);

            ISpatiallyDefinedDataComponent FuncPowerToDegreeSpat(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(spatVariantConstantDegrees);

            yield return new TestCaseData(spatVariantConstantDegrees,
                                          spatVariantConstantPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncPowerToDegreeSpat);

            var uniformTimeDependentDataDegrees =
                new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(
                    new TimeDependentParameters<DegreesDefinedSpreading>(Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>()));
            var uniformTimeDependentDataPower =
                new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(
                    new TimeDependentParameters<PowerDefinedSpreading>(Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>()));

            var spatVariantTimeDependentDegrees = new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>();
            var spatVariantTimeDependentPower = new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>();

            ISpatiallyDefinedDataComponent FuncDegreeToPowerUniformTimeDependent(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(uniformTimeDependentDataPower);

            yield return new TestCaseData(uniformTimeDependentDataPower,
                                          uniformTimeDependentDataDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncDegreeToPowerUniformTimeDependent);

            ISpatiallyDefinedDataComponent FuncPowerToDegreeUniformTimeDependent(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(uniformTimeDependentDataDegrees);

            yield return new TestCaseData(uniformTimeDependentDataDegrees,
                                          uniformTimeDependentDataPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncPowerToDegreeUniformTimeDependent);

            ISpatiallyDefinedDataComponent FuncDegreeToPowerSpatTimeDependent(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(spatVariantTimeDependentPower);

            yield return new TestCaseData(spatVariantTimeDependentPower,
                                          spatVariantTimeDependentDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncDegreeToPowerSpatTimeDependent);

            ISpatiallyDefinedDataComponent FuncPowerToDegreeSpatTimeDependent(ISpatiallyDefinedDataComponentFactory fact) =>
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(spatVariantTimeDependentDegrees);

            yield return new TestCaseData(spatVariantTimeDependentDegrees,
                                          spatVariantTimeDependentPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<ISpatiallyDefinedDataComponentFactory, ISpatiallyDefinedDataComponent>) FuncPowerToDegreeSpatTimeDependent);
        }
    }
}