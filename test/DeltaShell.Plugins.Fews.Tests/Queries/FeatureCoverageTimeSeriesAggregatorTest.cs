using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.ModelExchange.Queries;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.Fews.Tests.Queries
{
    [TestFixture]
    public class FeatureCoverageTimeSeriesAggregatorTest
    {
        private FeatureCoverage featureCoverage1, featureCoverage2;
        private Project project;
        private EventedList<IFeature> features;
        private const double X = 12;
        private const double Y = 42;


        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            features = new EventedList<IFeature>
                           {
                               new MockFeature {Name = "feature1", Geometry = new Point(X, Y)},
                               new MockFeature {Name = "feature2", Geometry = new Point(1, 1)},
                               new MockFeature {Name = "feature3", Geometry = new Point(2, 2)},
                           };

            featureCoverage1 = new FeatureCoverage {Name = "Q", Features = features, IsTimeDependent = true};
            featureCoverage1.Arguments.Add(new Variable<DateTime>("time"));
            featureCoverage1.Arguments.Add(new Variable<MockFeature>("feature"));
            var componentZero = new Variable<double>("value");
            componentZero.Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterDischarge;
            componentZero.Attributes[FunctionAttributes.AggregationType] = FunctionAttributes.AggregationTypes.Minimum;
            featureCoverage1.Attributes["location_type"] = "features";
            featureCoverage1.Components.Add(componentZero);

            featureCoverage2 = new FeatureCoverage {Name = "H", Features = features, IsTimeDependent = false};
            featureCoverage2.Arguments.Add(new Variable<MockFeature>("feature"));
            featureCoverage2.Components.Add(new Variable<double>("value"));

            project = new Project();
            project.RootFolder.Add(new DataItem
                                   {
                                       Name = "Qinput",
                                       ValueType = typeof (IFunction),
                                       Value = featureCoverage1,
                                       Role = DataItemRole.Input
                                   });

            project.RootFolder.Add(new DataItem
                                   {
                                       Name = "Qoutput",
                                       ValueType = typeof (IFunction),
                                       Value = featureCoverage2,
                                       Role = DataItemRole.Output
                                   });

        }

        [Test]
        public void RelevantCoverageAttributesAreIncludedInQueryResults()
        {
            var strategy = new FeatureCoverageTimeSeriesAggregator { DataItems = project.GetAllItemsRecursive() };

            var results = strategy.GetAll();

            var resultsOnDischarge = results.Where(qr => qr.ParameterId == FunctionAttributes.StandardNames.WaterDischarge).ToList();
            Assert.AreEqual(3, resultsOnDischarge.Count);

            var firstResult = resultsOnDischarge.First();
            Assert.AreEqual("MockFeature", firstResult.LocationType);
            Assert.AreEqual(FunctionAttributes.AggregationTypes.Minimum, firstResult.AggregationType);
        }

        [Test]
        public void GetAllTimeSeries_OneFeatureCoverageInDataItems_ShouldReturnOneTimeSeries()
        {
            // setup
            var strategy = new FeatureCoverageTimeSeriesAggregator {DataItems = project.GetAllItemsRecursive()};

            // actual test             
            var queryResults = strategy.GetAll();
            Assert.IsNotNull(queryResults);

            IEnumerable<AggregationResult> aggregationResults = queryResults as IList<AggregationResult> ?? queryResults.ToList();
            IFunction retrievedTimeSeries = aggregationResults.First().TimeSeries;

            // checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(aggregationResults.Any());
            Assert.IsNotNull(retrievedTimeSeries);
            Assert.AreEqual(featureCoverage1, retrievedTimeSeries);
            Assert.AreEqual(features.Count, aggregationResults.Count());
        }
    }

    #region Mocks

    internal class MockFeature : IBranchFeature
    {
        [FeatureAttribute]
        public double Value { get; set; }

        #region IBranchFeature Members

        public string Name { get; set; }

        public long Id { get; set; }

        public Type GetEntityType()
        {
            return GetType();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public IGeometry Geometry { get; set; }
        public IFeatureAttributeCollection Attributes { get; set; }

        public int CompareTo(INetworkFeature other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public INetwork Network { get; set; }

        public string Description { get; set; }

        public int CompareTo(IBranchFeature other)
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(object source)
        {
            throw new NotImplementedException();
        }

        public IBranch Branch { get; set; }

        public double Chainage { get; set; }

        public double Length { get; set; }

        #endregion
    }

    #endregion
}