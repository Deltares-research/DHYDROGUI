using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    public class BcwTimeSeriesOfBoundaryCollectorTest
    {
        [Test]
        public void Collect_ForUniformTimeDependentBoundary()
        {
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var underlyingFunction = Substitute.For<IFunction>();
            waveEnergyFunction.UnderlyingFunction.Returns(underlyingFunction);
            
            var dataComponent = new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(
                new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction));

            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            Assert.AreEqual(1, returnedTimeSeries.Count);
            Assert.AreSame(underlyingFunction, returnedTimeSeries.First());
        }

        [Test]
        public void Collect_ForSpatiallyVaryingTimeDependentBoundary()
        {
            var waveEnergyFunction1 = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var underlyingFunction1 = Substitute.For<IFunction>();
            waveEnergyFunction1.UnderlyingFunction.Returns(underlyingFunction1);

            var waveEnergyFunction2 = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var underlyingFunction2 = Substitute.For<IFunction>();
            waveEnergyFunction2.UnderlyingFunction.Returns(underlyingFunction2);

            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var timeDependentParameters1 = new TimeDependentParameters<PowerDefinedSpreading>(
                waveEnergyFunction1);

            var supportPoint2 = new SupportPoint(22, geometryDefinition);
            var timeDependentParameters2 = new TimeDependentParameters<PowerDefinedSpreading>(
                waveEnergyFunction2);
            
            var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>();
            dataComponent.AddParameters(supportPoint1, timeDependentParameters1);
            dataComponent.AddParameters(supportPoint2, timeDependentParameters2);
            
            List<IFunction> returnedTimeSeries = BcwTimeSeriesOfBoundaryCollector.Collect(dataComponent);

            Assert.AreEqual(2, returnedTimeSeries.Count);
            Assert.Contains(underlyingFunction1, returnedTimeSeries);
            Assert.Contains(underlyingFunction2, returnedTimeSeries);
        }
    }
}