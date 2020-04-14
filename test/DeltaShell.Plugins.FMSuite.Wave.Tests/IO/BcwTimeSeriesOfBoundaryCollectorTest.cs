using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class BcwTimeSeriesOfBoundaryCollectorTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Collect_ForUniformTimeDependentBoundary_Returns1TimeSerie()
        {
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var underlyingFunction = Substitute.For<IFunction>();
            waveEnergyFunction.UnderlyingFunction.Returns(underlyingFunction);
            
            var dataComponent = new UniformDataComponent<TimeDependentParameters<TSpreading>>(
                new TimeDependentParameters<TSpreading>(waveEnergyFunction));

            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            Assert.AreEqual(1, returnedTimeSeries.Count);
            Assert.AreSame(underlyingFunction, returnedTimeSeries.First());
        }

        [Test]
        public void Collect_ForSpatiallyVaryingTimeDependentBoundaryWith2ActiveSupportPoints_Returns2TimeSeries()
        {
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
            
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            Assert.AreEqual(2, returnedTimeSeries.Count);
            Assert.Contains(underlyingFunction1, returnedTimeSeries);
            Assert.Contains(underlyingFunction2, returnedTimeSeries);
        }
    }
}