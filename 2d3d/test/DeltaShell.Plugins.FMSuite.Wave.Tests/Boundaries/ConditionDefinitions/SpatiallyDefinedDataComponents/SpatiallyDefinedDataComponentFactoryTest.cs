using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    [TestFixture]
    public class SpatiallyDefinedDataComponentFactoryTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            // Call
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            // Assert
            Assert.That(componentFactory, Is.InstanceOf<ISpatiallyDefinedDataComponentFactory>());
        }

        [Test]
        public void Constructor_ParameterFactoryNull_ThrowsArgumentNullException()
        {
            void Call() => new SpatiallyDefinedDataComponentFactory(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parametersFactory"));
        }

        [Test]
        public void ConstructDefaultDataComponent_NotValidType_ThrowsNotSupportedException()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            // Call
            void Call() => componentFactory.ConstructDefaultDataComponent<DummyParameters>();

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void ConvertDataComponentSpreading_OldDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            // Call
            void Call() => componentFactory.ConstructDefaultDataComponent<DummyParameters>();

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void ConvertDataComponentSpreading_SameSpreadingType_ThrowsInvalidOperationException()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            // Call
            var component = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();
            void Call() => componentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, PowerDefinedSpreading>(component);

            // Assert
            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void ConvertDataComponentSpreading_UniformConstantDataComponentConvertedCorrectly()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            var parametersDegrees = new ConstantParameters<DegreesDefinedSpreading>(0, 0, 0, new DegreesDefinedSpreading());
            var component = new UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>(parametersDegrees);

            var parametersPower = new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading());
            parameterFactory.ConvertConstantParameters<DegreesDefinedSpreading, PowerDefinedSpreading>(parametersDegrees)
                            .Returns(parametersPower);

            // Call
            ISpatiallyDefinedDataComponent result =
                componentFactory.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(component);

            // Assert
            Assert.That(result, Is.InstanceOf<UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>>());
            var resultPower = (UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>) result;

            Assert.That(resultPower.Data, Is.SameAs(parametersPower));
            parameterFactory
                .Received(1)
                .ConvertConstantParameters<DegreesDefinedSpreading, PowerDefinedSpreading>(parametersDegrees);
        }

        [Test]
        public void ConvertDataComponentSpreading_SpatiallyVaryingConstantDataComponentConvertedCorrectly()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            var component = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();

            var supportPoints = new List<SupportPoint>();
            var parametersDegrees = new Dictionary<SupportPoint, ConstantParameters<DegreesDefinedSpreading>>();

            var geomDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geomDef.Length.Returns(100.0);
            for (var i = 0; i < 5; i++)
            {
                var supportPoint = new SupportPoint(10.0 * i, geomDef);
                supportPoints.Add(supportPoint);

                var powerParam = new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading());
                var degreesParam = new ConstantParameters<DegreesDefinedSpreading>(0, 0, 0, new DegreesDefinedSpreading());
                parametersDegrees[supportPoint] = degreesParam;

                component.AddParameters(supportPoint, powerParam);
                parameterFactory.ConvertConstantParameters<PowerDefinedSpreading, DegreesDefinedSpreading>(powerParam)
                                .Returns(degreesParam);
            }

            // Call
            ISpatiallyDefinedDataComponent result =
                componentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(component);

            // Assert
            Assert.That(result, Is.InstanceOf<SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>>());
            var componentDegrees = (SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>) result;

            Assert.That(componentDegrees.Data.Keys.Count(), Is.EqualTo(supportPoints.Count));
            foreach (SupportPoint supportPoint in supportPoints)
            {
                Assert.That(componentDegrees.Data.ContainsKey(supportPoint));
                Assert.That(componentDegrees.Data[supportPoint], Is.EqualTo(parametersDegrees[supportPoint]));
            }
        }

        [Test]
        public void ConvertDataComponentSpreading_UniformTimeDependentDataComponentConvertedCorrectly()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            var parametersDegrees = new TimeDependentParameters<DegreesDefinedSpreading>(Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>());
            var component = new UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>(parametersDegrees);

            var parametersPower = new TimeDependentParameters<PowerDefinedSpreading>(Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>());
            parameterFactory.ConvertTimeDependentParameters<DegreesDefinedSpreading, PowerDefinedSpreading>(parametersDegrees)
                            .Returns(parametersPower);

            // Call
            ISpatiallyDefinedDataComponent result =
                componentFactory.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(component);

            // Assert
            Assert.That(result, Is.InstanceOf<UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>());
            var resultPower = (UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>) result;

            Assert.That(resultPower.Data, Is.SameAs(parametersPower));
            parameterFactory
                .Received(1)
                .ConvertTimeDependentParameters<DegreesDefinedSpreading, PowerDefinedSpreading>(parametersDegrees);
        }

        [Test]
        public void ConvertDataComponentSpreading_SpatiallyVaryingTimeDependentDataComponentConvertedCorrectly()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            var component = new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>();

            var supportPoints = new List<SupportPoint>();
            var parametersDegrees = new Dictionary<SupportPoint, TimeDependentParameters<DegreesDefinedSpreading>>();

            var geomDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geomDef.Length.Returns(100.0);
            for (var i = 0; i < 5; i++)
            {
                var supportPoint = new SupportPoint(10.0 * i, geomDef);
                supportPoints.Add(supportPoint);

                var powerParam = new TimeDependentParameters<PowerDefinedSpreading>(Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>());
                var degreesParam = new TimeDependentParameters<DegreesDefinedSpreading>(Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>());
                parametersDegrees[supportPoint] = degreesParam;

                component.AddParameters(supportPoint, powerParam);
                parameterFactory.ConvertTimeDependentParameters<PowerDefinedSpreading, DegreesDefinedSpreading>(powerParam)
                                .Returns(degreesParam);
            }

            // Call
            ISpatiallyDefinedDataComponent result =
                componentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(component);

            // Assert
            Assert.That(result, Is.InstanceOf<SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>());
            var componentDegrees = (SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>) result;

            Assert.That(componentDegrees.Data.Keys.Count(), Is.EqualTo(supportPoints.Count));

            foreach (SupportPoint supportPoint in supportPoints)
            {
                Assert.That(componentDegrees.Data.ContainsKey(supportPoint));
                Assert.That(componentDegrees.Data[supportPoint], Is.EqualTo(parametersDegrees[supportPoint]));
            }
        }

        [Test]
        public void ConvertDataComponentSpreading_UnsupportedDataComponent_ThrowsNotSupportedException()
        {
            // Setup
            var parameterFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var componentFactory = new SpatiallyDefinedDataComponentFactory(parameterFactory);

            // Call
            void Call() => componentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(new DummyParameters());

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        private class DummyParameters : ISpatiallyDefinedDataComponent
        {
            public void AcceptVisitor(ISpatiallyDefinedDataComponentVisitor visitor) {}
        }
    }
}