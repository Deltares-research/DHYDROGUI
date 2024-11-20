using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Factories
{
    [TestFixture]
    public class ViewEnumFromDataComponentQuerierTest
    {
        [Test]
        [TestCaseSource(nameof(GetForcingTypeData))]
        public void GetForcingType_ReturnsExpectedResult(ISpatiallyDefinedDataComponent dataComponent, ForcingViewType expectedResult)
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call
            ForcingViewType result = converter.GetForcingType(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetForcingType_DataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call | Assert
            void Call() => converter.GetForcingType(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        [Test]
        public void GetForcingType_UnsupportedDataComponentType_ThrowsNotSupportedException()
        {
            // Setup
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call | Assert
            void Call() => converter.GetForcingType(dataComponent);

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        [TestCaseSource(nameof(GetSpatialDefinitionData))]
        public void GetSpatialDefinition_ValidData_ReturnsCorrectResults(ISpatiallyDefinedDataComponent dataComponent,
                                                                         SpatialDefinitionViewType expectedDefinitionViewType)
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call
            SpatialDefinitionViewType result = converter.GetSpatialDefinition(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDefinitionViewType));
        }

        [Test]
        public void GetSpatialDefinition_InvalidDataComponent_ThrowsNotSupportedException()
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            // Call | Assert
            void Call() => converter.GetSpatialDefinition(dataComponent);

            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetSpatialDefinition_DataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call | Assert
            void Call() => converter.GetSpatialDefinition(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
        }

        [Test]
        [TestCaseSource(nameof(GetDirectionalSpreadingViewTypeData))]
        public void GetDirectionalSpreadingViewType_ExpectedResults(ISpatiallyDefinedDataComponent dataComponent,
                                                                    DirectionalSpreadingViewType expectedDirectionalSpreadingViewType)
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call
            DirectionalSpreadingViewType result = converter.GetDirectionalSpreadingViewType(dataComponent);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDirectionalSpreadingViewType));
        }

        [Test]
        public void GetDirectionalSpreadingViewType_UnsupportedDataComponent_ThrowsNotSupportedException()
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call | Assert
            void Call() => converter.GetDirectionalSpreadingViewType(Substitute.For<ISpatiallyDefinedDataComponent>());
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void GetDirectionalSpreadingViewType_DataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new ViewEnumFromDataComponentQuerier();

            // Call | Assert
            void Call() => converter.GetDirectionalSpreadingViewType(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponent"));
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

            yield return new TestCaseData(new UniformDataComponent<FileBasedParameters>(new FileBasedParameters("path")), SpatialDefinitionViewType.Uniform);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(), SpatialDefinitionViewType.SpatiallyVarying);
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
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(new TimeDependentParameters<PowerDefinedSpreading>(powerDefinedFunction)),
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(new TimeDependentParameters<DegreesDefinedSpreading>(degreesDefinedFunction)),
                                          DirectionalSpreadingViewType.Degrees);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(),
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(),
                                          DirectionalSpreadingViewType.Degrees);

            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(),
                                          DirectionalSpreadingViewType.Power);
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<FileBasedParameters>(),
                                          DirectionalSpreadingViewType.Power);
        }
    }
}