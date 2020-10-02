using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class BcwTimeSeriesOfBoundaryCollectorTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private readonly Random random = new Random();

        [Test]
        public void Collect_ForUniformTimeDependentBoundary_Returns1TimeSeries()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var underlyingFunction = Substitute.For<IFunction>();
            waveEnergyFunction.UnderlyingFunction.Returns(underlyingFunction);

            var dataComponent = new UniformDataComponent<TimeDependentParameters<TSpreading>>(
                new TimeDependentParameters<TSpreading>(waveEnergyFunction));

            // Call
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            Assert.AreEqual(1, returnedTimeSeries.Count);
            Assert.AreSame(underlyingFunction, returnedTimeSeries.First());
        }

        [Test]
        public void Collect_ForSpatiallyVaryingTimeDependentBoundaryWith2ActiveSupportPoints_Returns2TimeSeries()
        {
            // Setup
            var waveEnergyFunction1 = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var underlyingFunction1 = Substitute.For<IFunction>();
            waveEnergyFunction1.UnderlyingFunction.Returns(underlyingFunction1);

            var waveEnergyFunction2 = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var underlyingFunction2 = Substitute.For<IFunction>();
            waveEnergyFunction2.UnderlyingFunction.Returns(underlyingFunction2);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var timeDependentParameters1 = new TimeDependentParameters<TSpreading>(
                waveEnergyFunction1);

            var supportPoint2 = new SupportPoint(22, geometryDefinition);
            var timeDependentParameters2 = new TimeDependentParameters<TSpreading>(
                waveEnergyFunction2);

            var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            dataComponent.AddParameters(supportPoint1, timeDependentParameters1);
            dataComponent.AddParameters(supportPoint2, timeDependentParameters2);

            // Call
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            Assert.AreEqual(2, returnedTimeSeries.Count);
            Assert.Contains(underlyingFunction1, returnedTimeSeries);
            Assert.Contains(underlyingFunction2, returnedTimeSeries);
        }

        [Test]
        public void Collect_ForUniformConstantBoundary_NothingShouldHappen()
        {
            // Setup
            var constantParameters = new ConstantParameters<TSpreading>(random.Next(), random.Next(), random.Next(), new TSpreading());
            var dataComponent = new UniformDataComponent<ConstantParameters<TSpreading>>(constantParameters);

            // Call
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            Assert.That(returnedTimeSeries, Is.Empty);
        }

        [Test]
        public void Collect_ForSpatiallyVaryingConstantBoundary_NothingShouldHappen()
        {
            // Setup
            var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            var constantParameters = new ConstantParameters<TSpreading>(random.Next(), random.Next(), random.Next(), new TSpreading());
            var supportPoint = new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            dataComponent.AddParameters(supportPoint, constantParameters);

            // Call
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            Assert.That(returnedTimeSeries, Is.Empty);
        }

        [Test]
        public void Collect_ForUniformFileBasedBoundary_NothingShouldHappen()
        {
            // Setup
            var constantParameters = new FileBasedParameters("path");
            var dataComponent = new UniformDataComponent<FileBasedParameters>(constantParameters);

            // Call
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            Assert.That(returnedTimeSeries, Is.Empty);
        }

        [Test]
        public void Collect_ForSpatiallyVaryingFileBasedBoundary_NothingShouldHappen()
        {
            // Setup
            var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            var constantParameters = new FileBasedParameters("path");
            var supportPoint = new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            dataComponent.AddParameters(supportPoint, constantParameters);

            // Call
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            Assert.That(returnedTimeSeries, Is.Empty);
        }
    }

    [TestFixture]
    public class BcwTimeSeriesOfBoundaryCollectorTest
    {
        [Test]
        public void Visit_UniformDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            dataComponent.When(x => x.AcceptVisitor(Arg.Any<ISpatiallyDefinedDataComponentVisitor>()))
                         .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit((UniformDataComponent<IForcingTypeDefinedParameters>) null));

            // Call
            void Call() => BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("uniformDataComponent"));
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            dataComponent.When(x => x.AcceptVisitor(Arg.Any<ISpatiallyDefinedDataComponentVisitor>()))
                         .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit((SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>) null));

            // Call
            void Call() => BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("spatiallyVaryingDataComponent"));
        }

        [Test]
        public void Visit_TimeDependentParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            dataComponent.When(x => x.AcceptVisitor(Arg.Any<ISpatiallyDefinedDataComponentVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit((TimeDependentParameters<PowerDefinedSpreading>) null));

            // Call
            void Call() => BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("timeDependentParameters"));
        }
    }
}