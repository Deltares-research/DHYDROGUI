using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
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

        private static IEnumerable<TestCaseData> GetForcingTypeData()
        {
            yield return  new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading())), ForcingViewType.Constant);
            yield return  new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading())), ForcingViewType.Constant);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), ForcingViewType.Constant);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), ForcingViewType.Constant);

            var powerDefinedFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var degreesDefinedFunction = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();
            yield return  new TestCaseData(new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(powerDefinedFunction)), ForcingViewType.TimeSeries);
            yield return  new TestCaseData(new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(degreesDefinedFunction)), ForcingViewType.TimeSeries);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(), ForcingViewType.TimeSeries);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(), ForcingViewType.TimeSeries);
        }

        [Test]
        [TestCaseSource(nameof(GetForcingTypeData))]
        public void GetForcingType_ReturnsExpectedResult(IBoundaryConditionDataComponent dataComponent, ForcingViewType expectedResult)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call
            ForcingViewType result = factory.GetForcingType(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
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
        public void GetForcingType_UnsupportedDataComponentType_ThrowsNotSupportedException()
        {
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            void Call() => factory.GetForcingType(dataComponent);

            var exception = Assert.Throws<NotSupportedException>(Call);
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
            yield return  new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading())), SpatialDefinitionViewType.Uniform);
            yield return  new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading())), SpatialDefinitionViewType.Uniform);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), SpatialDefinitionViewType.SpatiallyVarying);
            yield return  new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), SpatialDefinitionViewType.SpatiallyVarying);
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
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading())), typeof(UniformConstantParametersSettingsViewModel<PowerDefinedSpreading>));
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading())), typeof(UniformConstantParametersSettingsViewModel<DegreesDefinedSpreading>));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), typeof(SpatiallyVariantConstantParametersSettingsViewModel<PowerDefinedSpreading>));
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>(), typeof(SpatiallyVariantConstantParametersSettingsViewModel<DegreesDefinedSpreading>));
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
        public void ConstructBoundaryConditionDataComponent_UniformConstantPower_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0.0, 0.0, 0.0, new PowerDefinedSpreading()));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
        public void ConstructBoundaryConditionDataComponent_UniformConstantDegrees_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(new ConstantParameters<DegreesDefinedSpreading>(0.0, 0.0, 0.0, new DegreesDefinedSpreading()));
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
        public void ConstructBoundaryConditionDataComponent_SpatiallyVariantConstantDegrees_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
        public void ConstructBoundaryConditionDataComponent_SpatiallyVariantConstantPower_ExpectedResults()
        {
            // Setup
            var srcDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            modelDataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>>()
                                     .Returns(srcDataComponent);

            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
        [TestCase(ForcingViewType.TimeSeries, SpatialDefinitionViewType.Uniform, DirectionalSpreadingViewType.Power)]
        [TestCase(ForcingViewType.TimeSeries, SpatialDefinitionViewType.Uniform, DirectionalSpreadingViewType.Degrees)]
        [TestCase(ForcingViewType.TimeSeries, SpatialDefinitionViewType.SpatiallyVarying, DirectionalSpreadingViewType.Power)]
        [TestCase(ForcingViewType.TimeSeries, SpatialDefinitionViewType.SpatiallyVarying, DirectionalSpreadingViewType.Degrees)]
        [TestCase(ForcingViewType.FileBased, SpatialDefinitionViewType.Uniform, DirectionalSpreadingViewType.Power)]
        [TestCase(ForcingViewType.FileBased, SpatialDefinitionViewType.Uniform, DirectionalSpreadingViewType.Degrees)]
        [TestCase(ForcingViewType.FileBased, SpatialDefinitionViewType.SpatiallyVarying, DirectionalSpreadingViewType.Power)]
        [TestCase(ForcingViewType.FileBased, SpatialDefinitionViewType.SpatiallyVarying, DirectionalSpreadingViewType.Degrees)]
        public void ConstructBoundaryConditionDataComponent_UnsupportedType_ThrowsNotSupportedException(ForcingViewType viewType,
                                                                                                        SpatialDefinitionViewType spatialDefinition, 
                                                                                                        DirectionalSpreadingViewType spreadingType)
        {
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            void Call() => factory.ConstructBoundaryConditionDataComponent(viewType, 
                                                                           spatialDefinition, 
                                                                           spreadingType);
            Assert.Throws<NotSupportedException>(Call);
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
        }

        [Test]
        [TestCaseSource(nameof(GetDirectionalSpreadingViewTypeData))]
        public void GetDirectionalSpreadingViewType_ExpectedResults(IBoundaryConditionDataComponent dataComponent, 
                                                                    DirectionalSpreadingViewType expectedDirectionalSpreadingViewType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call | Assert
            void Call() => factory.GetDirectionalSpreadingViewType(new DummyDataComponent());
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetDirectionalSpreadingViewType_DataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call | Assert
            void Call() => factory.GetDirectionalSpreadingViewType(null);
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
        }

        [Test]
        [TestCaseSource(nameof(ConvertBoundaryConditionDataComponentSpreadingTypeSameTypeData))]
        public void ConvertBoundaryConditionDataComponentSpreadingType_NewSpreadingTypeEqualsDataComponentType_ReturnsDataComponent(IBoundaryConditionDataComponent dataComponent,
                                                                                                                                    DirectionalSpreadingViewType spreadingType)
        {
            // Setup
            var modelDataComponentFactory = Substitute.For<IBoundaryConditionDataComponentFactory>();
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call
            IBoundaryConditionDataComponent result = 
                factory.ConvertBoundaryConditionDataComponentSpreadingType(dataComponent, spreadingType);

            // Assert
            Assert.That(result, Is.SameAs(dataComponent));
        }

        private static IEnumerable<TestCaseData> ConvertBoundaryConditionDataComponentSpreadingTypeDifferentTypeData()
        {
            var uniformDataDegrees =
                new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(
                    new ConstantParameters<DegreesDefinedSpreading>(0, 0, 0, new DegreesDefinedSpreading()));
            var uniformDataPower =
                    new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(
                        new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading()));

            var spatVariantDegrees = new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>();
            var spatVariantPower = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();

            IBoundaryConditionDataComponent FuncDegreeToPowerUniform(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(uniformDataPower);
            yield return new TestCaseData(uniformDataPower,
                                          uniformDataDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncDegreeToPowerUniform);

            IBoundaryConditionDataComponent FuncPowerToDegreeUniform(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(uniformDataDegrees);
            yield return new TestCaseData(uniformDataDegrees,
                                          uniformDataPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncPowerToDegreeUniform);

            IBoundaryConditionDataComponent FuncDegreeToPowerSpat(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(spatVariantPower);
            yield return new TestCaseData(spatVariantPower,
                                          spatVariantDegrees,
                                          DirectionalSpreadingViewType.Degrees,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncDegreeToPowerSpat);

            IBoundaryConditionDataComponent FuncPowerToDegreeSpat(IBoundaryConditionDataComponentFactory fact) => 
                fact.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(spatVariantDegrees);
            yield return new TestCaseData(spatVariantDegrees,
                                          spatVariantPower,
                                          DirectionalSpreadingViewType.Power,
                                          (Func<IBoundaryConditionDataComponentFactory, IBoundaryConditionDataComponent>) FuncPowerToDegreeSpat);
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
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

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
            var factory = new ViewDataComponentFactory(modelDataComponentFactory);

            // Call | Assert
            void Call() => factory.ConvertBoundaryConditionDataComponentSpreadingType(null, DirectionalSpreadingViewType.Degrees);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("currentDataComponent"));
        }
    }
}