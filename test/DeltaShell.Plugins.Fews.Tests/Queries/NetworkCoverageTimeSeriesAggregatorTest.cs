using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests.Queries
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class NetworkCoverageTimeSeriesAggregatorTest
    {
        private Project project;
        private WaterFlowModel1D model;

        [SetUp]
        public void Setup()
        {
            model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            project = new Project();
            project.RootFolder.Add(model);
        }

        [TearDown]
        public void TearDown()
        {
            model.Dispose();
            model = null;
            project = null;
            GC.Collect(2,GCCollectionMode.Forced);
        }


        [Test]
        public void StaggeredOutputCoveragesDischargeFoundByLocationType()
        {
            //enable staggered output coverage
            model.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet).
                AggregationOptions = AggregationOptions.Current;
            
            var strategy = new NetworkCoverageTimeSeriesAggregator() { DataItems = project.GetAllItemsRecursive() };

            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var results = strategy.GetAll();
            var resultOnStaggered = results.Where(r => r.LocationType == FunctionAttributes.StandardFeatureNames.ReachSegment);
            Assert.IsTrue(resultOnStaggered.Any());
            Assert.Greater(results.Count(qr => qr.ParameterId == FunctionAttributes.StandardNames.WaterDischarge),0);
        }

        [Test]
        public void StaggeredOutputCoveragesVelocityFoundByParameterId()
        {
            //enable staggered output coverage
            model.OutputSettings.GetEngineParameter(QuantityType.Velocity, ElementSet.ReachSegElmSet).
                AggregationOptions = AggregationOptions.Current;

            var strategy = new NetworkCoverageTimeSeriesAggregator() { DataItems = project.GetAllItemsRecursive() };

            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var results = strategy.GetAll();
            var waterVelocityResults = results.Where(r => r.ParameterId == FunctionAttributes.StandardNames.WaterVelocity);
            Assert.IsTrue(waterVelocityResults.Any());
            foreach (var queryResult in waterVelocityResults)
            {
                Assert.AreEqual(FunctionAttributes.StandardFeatureNames.ReachSegment, queryResult.LocationType);
            }
        }

        [Test]
        public void StaggeredOutputCoveragesFlowAreaFoundByParameterId()
        {
            //enable staggered output coverage
            model.OutputSettings.GetEngineParameter(QuantityType.FlowArea, ElementSet.ReachSegElmSet).
                AggregationOptions = AggregationOptions.Current;

            var strategy = new NetworkCoverageTimeSeriesAggregator() { DataItems = project.GetAllItemsRecursive() };

            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var results = strategy.GetAll();
            var waterFlowAreaResults = results.Where(r => r.ParameterId == FunctionAttributes.StandardNames.WaterFlowArea);
            Assert.IsTrue(waterFlowAreaResults.Any());
            Assert.IsNotNull(waterFlowAreaResults.Where(r => r.LocationType == FunctionAttributes.StandardFeatureNames.ReachSegment));
        }

        [Test]
        public void OutputCoveragesBasedOnComputationalGridShouldReturnTimeSeriesOnGridPoint()
        {
            //enable output coverage on discretization
            model.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).
                AggregationOptions = AggregationOptions.Current;

            var strategy = new NetworkCoverageTimeSeriesAggregator() {DataItems = project.GetAllItemsRecursive()};

            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var results = strategy.GetAll();
            var gridResults = results.Where(r => r.LocationType == FunctionAttributes.StandardFeatureNames.GridPoint);
            Assert.IsTrue(gridResults.Any(), "Output TimeSeries on discretization should not be empty");
        }

        [Test]
        public void OutputCoveragesBasedOnComputationalGridShouldReturnTimeSeriesOnLocationTypeGridPoint()
        {
            //enable output coverage on discretization
            model.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches).
                AggregationOptions = AggregationOptions.Current;

            var strategy = new NetworkCoverageTimeSeriesAggregator() { DataItems = project.GetAllItemsRecursive() };

            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var results = strategy.GetAll();
            var resultsOnGrid = results.Where(r => r.LocationType == FunctionAttributes.StandardFeatureNames.GridPoint);
            Assert.IsTrue(resultsOnGrid.Any(), "Output TimeSeries on discretization should not be empty");
        }

        [Test]
        public void RelevantCoverageAttributesAreIncludedInQueryResults()
        {
            //enable staggered output coverage
            model.OutputSettings.GetEngineParameter(QuantityType.WaterDepth, ElementSet.GridpointsOnBranches).
                AggregationOptions = AggregationOptions.Current;
            
            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var strategy = new NetworkCoverageTimeSeriesAggregator() { DataItems = project.GetAllItemsRecursive() };

            var results = strategy.GetAll();

            var resultsOnWaterDepth = results.Where(qr => qr.ParameterId == FunctionAttributes.StandardNames.WaterDepth).ToList();
            Assert.AreEqual(27, resultsOnWaterDepth.Count);

            var firstResult = resultsOnWaterDepth.First();
            Assert.AreEqual(FunctionAttributes.StandardFeatureNames.GridPoint, firstResult.LocationType);
            Assert.AreEqual(FunctionAttributes.AggregationTypes.None, firstResult.AggregationType);
        }

        [Test]
        public void GetAllTimeSeries_ProjectContainsDemoModel_shouldReturnTimeSeries()
        {
            // setup

            var strategy = new NetworkCoverageTimeSeriesAggregator()
                           {DataItems = project.GetAllItemsRecursive()};

            // actual test             
            var queryResults = strategy.GetAll();

            // checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any());
        }

        [Test]
        public void GetAllTimeSeries_ProjectContainsDemoModel_ShouldReturnOutputTimeSeries()
        {
            // setup

            var strategy = new NetworkCoverageTimeSeriesAggregator() { DataItems = project.GetAllItemsRecursive() };

            // actual test             
            var queryResults = strategy.GetAll();
            // checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any(i => i.ExchangeType == ExchangeType.Output));
        }

        [Test]
        public void GetAll_ShouldNotOutputDuplicateItems()
        {
            // setup            
            model.UseSalt = true;
            model.UseSaltInCalculation = true;
            model.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches).
            AggregationOptions = AggregationOptions.Current;

            model.Initialize();
            // normally (flowModel1D.Status != ActivityStatus.Finished) is checked during execute
            // we only need one timestep, just to add the observationpoint to a networkcoverage
            model.Execute();

            var strategy = new NetworkCoverageTimeSeriesAggregator{ DataItems = project.GetAllItemsRecursive() };

            // actual test             
            var queryResults = strategy.GetAll();

            // checks
            Assert.IsNotNull(queryResults);

            var items = new HashSet<string>();
            foreach (string value in queryResults.Select(queryResult => queryResult.ToString()))
            {
                if (items.Contains(value))
                {
                    Assert.Fail("The result with value " + value + " already exists");
                }
                items.Add(value);
            }

            Assert.AreEqual(queryResults.Count(), items.Count);
        }
    }
}