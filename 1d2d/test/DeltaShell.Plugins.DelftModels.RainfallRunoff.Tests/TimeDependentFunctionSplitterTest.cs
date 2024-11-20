using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class TimeDependentFunctionSplitterTest
    {
        private FeatureCoverage featureCoverage;
        private Catchment catchment1;
        private Catchment catchment2;
        private Catchment catchment3;

        private TimeDependentFunctionSplitter Splitter { get; set; }

        [SetUp]
        public void SetUp()
        {
            Splitter = new TimeDependentFunctionSplitter();

            featureCoverage = new FeatureCoverage("TimeDepFeatCov") { IsTimeDependent = true };
            featureCoverage.Arguments.Add(new Variable<IFeature>("Catchment"));
            featureCoverage.Components.Add(new Variable<double>("Value"));

            var basin = new DrainageBasin();
            catchment1 = new Catchment { Name = "catch1" };
            catchment2 = new Catchment { Name = "catch2" };
            catchment3 = new Catchment { Name = "catch3" };

            basin.Catchments.Add(catchment1);
            basin.Catchments.Add(catchment2);
            basin.Catchments.Add(catchment3);

            featureCoverage.Features.Add(catchment1);
            featureCoverage.Features.Add(catchment2);
            featureCoverage.Features.Add(catchment3);

            featureCoverage.Time.Values.Add(new DateTime(2000, 1, 1));
            featureCoverage.Time.Values.Add(new DateTime(2001, 1, 1));
            featureCoverage.Time.Values.Add(new DateTime(2002, 1, 1));
            featureCoverage.Time.Values.Add(new DateTime(2003, 1, 1));

            featureCoverage.FeatureVariable.Values.Add(catchment1);
            featureCoverage.FeatureVariable.Values.Add(catchment2);
            featureCoverage.FeatureVariable.Values.Add(catchment3);
        }

        [Test]
        public void ExtractSeries()
        {
            var timeSeries = Splitter.ExtractSeriesForArgumentValue(featureCoverage, catchment1);
            Assert.AreEqual(1, timeSeries.Arguments.Count);
            Assert.AreEqual(typeof (DateTime), timeSeries.Arguments[0].ValueType);
            Assert.AreEqual(4, timeSeries.Components[0].Values.Count);
        }
        [Test]
        public void SplitIntoFunctions()
        {
            var seriesPerFeature = Splitter.SplitIntoFunctionsPerArgumentValue(featureCoverage);
            Assert.AreEqual(3, seriesPerFeature.Count());

            foreach (var timeSeries in seriesPerFeature)
            {
                Assert.AreEqual(1, timeSeries.Arguments.Count);
                Assert.AreEqual(typeof (DateTime), timeSeries.Arguments[0].ValueType);
                Assert.AreEqual(4, timeSeries.Components[0].Values.Count);
            }
        }
    }
}
