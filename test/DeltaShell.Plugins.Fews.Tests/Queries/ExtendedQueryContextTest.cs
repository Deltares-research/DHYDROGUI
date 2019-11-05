using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.ModelExchange.Queries;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests.Queries
{
    [TestFixture]
    public class ProjectQueryContextTest : FewsAdapterTestBase
    {
        private const string parameterIdWlts = FunctionAttributes.StandardNames.WaterLevel;//"water level time series";
        private const string locationIdNode3 = "Node3";

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Environment.SetEnvironmentVariable("UGLY_FEWS_HACK", "true");
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Environment.SetEnvironmentVariable("UGLY_FEWS_HACK", "false");
        }


        [Test]
        [Ignore("does not test anything and takes 7 sec")]
        [Category(TestCategory.Integration)]
        public void PrintAll()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            model.Initialize();

            var project = new Project();
            project.RootFolder.Add(model);

            var context = new ExtendedQueryContext(project);           

            // print all
            //IEnumerable<QueryResult> queryResults = context.GetAll();
            var groups = context.GetAllGroupedByFeatureOwner();

            foreach (var group in groups)
            {
                var name = group.Key as INameable;
                Console.WriteLine(name != null ? name.Name : group.GetType().Name);
                foreach (var queryResult in group)
                {
                    Console.WriteLine("\t" + queryResult);
                }                
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllGroupedByFeatureOwner_DemoModel_ShouldReturnResults()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            model.Initialize();
            var project = new Project();
            project.RootFolder.Add(model);
            var context = new ExtendedQueryContext(project);

            // call            
            var queryResults = context.GetAllGroupedByFeatureOwner();

            // checks
            var count = 0;
            foreach (var group in queryResults)
            {
                var name = group.Key as INameable;
                Console.WriteLine(name != null ? name.Name : group.GetType().Name);
                foreach (var queryResult in group)
                {
                    Console.WriteLine("\t" + queryResult);                    
                }

                count++;
            }

            Assert.IsTrue(count > 0, "There are no results returned for the query of GetAllByLocationType with argument 'grid_points'");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllByFeatureOwner_DemoModel_ShouldReturnResults()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            model.Initialize();
            var project = new Project();
            project.RootFolder.Add(model);
            var context = new ExtendedQueryContext(project);

            // call                  
            var queryResults = context.GetAllByFeatureOwner<Model1DBoundaryNodeData>();

            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any(), "There are no results found using the query GetAllByFeatureOwner");            
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllByFeature_DemoModel_ShouldReturnResults()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            model.Initialize();
            var project = new Project();
            project.RootFolder.Add(model);
            var context = new ExtendedQueryContext(project);

            // call (gets all boundary nodes)                
            var queryResults = context.GetAllByFeatureType<HydroNode>()
                .Where(r => !((HydroNode)r.Feature).IsConnectedToMultipleBranches);

            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any(), "There are no results found using the query GetAllByFeature");
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllByLocationType_GridPointsOnDemoModel_ShouldReturnResults()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            model.Initialize();
            var project = new Project();
            project.RootFolder.Add(model);
            var context = new ExtendedQueryContext(project);

            // call            
            IEnumerable<AggregationResult> queryResults = context.GetAllByLocationType(FunctionAttributes.StandardFeatureNames.GridPoint);
            
            // checks
            var count = 0;
            foreach (var result in queryResults)
            {
                Console.WriteLine(result);
                count++;
            }

            Assert.IsTrue(count > 0, "There are no results returned for the query of GetAllByLocationType with argument " + FunctionAttributes.StandardFeatureNames.GridPoint);
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void RelevantCoverageAttributesAreIncludedInQueryResults()
        {
            var model = CreateDemoNetworkWithLateralSources();
            var project = new Project();
            project.RootFolder.Add(model);
            var context = new ExtendedQueryContext(project);

            //enable staggered output coverage
            model.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet).
                AggregationOptions = AggregationOptions.None;
            model.OutputSettings.GetEngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals).
                AggregationOptions = AggregationOptions.Current;

            model.UseSalt = false;
            model.UseSaltInCalculation = false;

            model.Initialize();

            // print all
            IEnumerable<AggregationResult> results = context.GetAll();

            var resultsOnDischarge = results.Where(qr => qr.ParameterId == FunctionAttributes.StandardNames.WaterDischarge).ToList();
            Assert.AreEqual(2, resultsOnDischarge.Count);

            var lastResult = resultsOnDischarge.Last();
            Assert.AreEqual("LateralSource", lastResult.LocationType);
            Assert.AreEqual(FunctionAttributes.AggregationTypes.None, lastResult.AggregationType);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetDischargeParameter_DemoModel_ShoulFindTheParameter()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            var project = new Project();
            project.RootFolder.Add(model);
            var context = new ExtendedQueryContext(project);
            model.OutputSettings.GetEngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals).
               AggregationOptions = AggregationOptions.Current;

            model.Initialize();
            // print all
            IEnumerable<AggregationResult> queryResults = context.GetAllByParameterId(FunctionAttributes.StandardNames.WaterDischarge);
            int count = 0;
            foreach (var result in queryResults)
            {
                Console.WriteLine(result);
                count++;
            }
            Assert.IsTrue(count > 0, "No discharge parameters found");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAll_UsingDemoNetwork_ShouldNotReturnDuplicateItems()
        {
            // setup 
            var model = CreateDemoNetworkWithLateralSources();

            var project = new Project();
            project.RootFolder.Add(model);

            model.UseSalt = false;
            model.UseSaltInCalculation = false;

            model.Initialize();
            model.Execute();

            var strategy = new FeatureDataTimeSeriesAggregator();
            var context = new ExtendedQueryContext(project, strategy, model.NetworkDiscretization);
            
            //var context = ProjectQueryContext.CreateContext(project);
            
            // actual test             
            var queryResults = context.GetAll();

            // checks
            Assert.IsNotNull(queryResults);

            var items = new HashSet<string>();
            foreach (var result in queryResults)
            {
                var resultString = result.ToString();
                if (items.Contains(resultString))
                {
                    Assert.Fail("The result with value " + resultString + " already exists");
                }
                items.Add(resultString);
            }
           Assert.AreEqual(queryResults.Count(), items.Count);

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetInputTimeSeriesByLocation_ProjectContainsLinkedTimeSeries_ShouldReturnLinkedTimeSeries()
        {
            // setup
            var timeSeries = new TimeSeries { Name = "timeSerie 1.Q" };
            var sourceDataItem = new DataItem { Value = timeSeries, ValueType = typeof(IFunction), Role = DataItemRole.Input };

            var targetDataItem = new DataItem { Name = "target", ValueType = typeof(IFunction), Role = DataItemRole.Input };
            targetDataItem.LinkTo(sourceDataItem);

            var project = new Project();
            project.RootFolder.Add(sourceDataItem);
            project.RootFolder.Add(targetDataItem);

            var projectContext = new ExtendedQueryContext(project, new DataItemTimeSeriesAggregator());

            // call 
            var queryResult = projectContext.GetAllByLocationId("timeSerie 1").FirstOrDefault();

            // checks
            Assert.IsNotNull(queryResult);
            Assert.IsNotNull(queryResult);
            Assert.AreEqual(timeSeries, queryResult.TimeSeries);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllTimeSeriesByParameter_ProjectContainsLinkedTimeSeries_ShouldReturnLinkedTimeSeries()
        {
            // setup
            var timeSeries = new TimeSeries { Name = "timeSerie 1.Q" };
            var sourceDataItem = new DataItem { Value = timeSeries, ValueType = typeof(IFunction), Role = DataItemRole.Input };

            var targetDataItem = new DataItem { Name = "target", ValueType = typeof(IFunction), Role = DataItemRole.Input };
            targetDataItem.LinkTo(sourceDataItem);

            var project = new Project();
            project.RootFolder.Add(sourceDataItem);
            project.RootFolder.Add(targetDataItem);

            var projectContext = new ExtendedQueryContext(project, new DataItemTimeSeriesAggregator());

            // call 
            var queryResult = projectContext.GetAllByParameterId("Q").FirstOrDefault();

            // checks
            Assert.IsNotNull(queryResult);
            Assert.AreEqual(timeSeries, queryResult.TimeSeries);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllTimeSeriesByParameterAndLocation_ProjectContainsLinkedTimeSeries_ShouldReturnLinkedTimeSeries()
        {
            // setup
            var timeSeries = new TimeSeries { Name = "timeSerie 1.Q" };
            var sourceDataItem = new DataItem { Value = timeSeries, ValueType = typeof(IFunction), Role = DataItemRole.Input };

            var targetDataItem = new DataItem { Name = "target", ValueType = typeof(IFunction), Role = DataItemRole.Input };
            targetDataItem.LinkTo(sourceDataItem);

            var project = new Project();
            project.RootFolder.Add(sourceDataItem);
            project.RootFolder.Add(targetDataItem);

            var projectContext = new ExtendedQueryContext(project, new DataItemTimeSeriesAggregator());

            // call 
            var queryResult = projectContext.GetAllByParameterIdAndLocationId("Q", "timeSerie 1").FirstOrDefault();

            // checks
            Assert.IsNotNull(queryResult);
            Assert.AreEqual(timeSeries, queryResult.TimeSeries);
        }

        [Test]
        [Ignore]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void GetTimeSeriesByLocationId_ProjectContainsNetworkCoverage_shouldReturnTimeSeries()
        {
            // this test is hypothetical; at this moment, no other coverages, other than discretization,
            // hold location id's. In future this might change... 

            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";
            var networkCoverage = CreateNetworkCoverage(locationId, parameterId);

            var strategy = new NetworkCoverageTimeSeriesAggregator(NetworkDiscretization);

            var project = new Project();
            project.RootFolder.Add(networkCoverage);

            var projectContext = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResults = projectContext.GetAllByLocationId(locationId);

            // checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any(), "Should have query results, but returned none");
            Assert.AreEqual(networkCoverage, queryResults.First().TimeSeries);
        }

        [Test]
        [Ignore]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void GetTimeSeriesByParameterAndLocationId_ProjectContainsNetworkCoverage_ShouldHaveQueryResults()
        {
            // this test is hypothetical; at this moment, no other coverages, other than discretization,
            // hold location id's. In future this might change... 

            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";
            var networkCoverage = CreateNetworkCoverage(locationId, parameterId);

            var project = new Project();
            project.RootFolder.Add(networkCoverage);

            var strategy = new NetworkCoverageTimeSeriesAggregator(NetworkDiscretization);
            var context = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResults = context.GetAllByParameterIdAndLocationId(parameterId, locationId);

            // checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any(), "Should have query results, but returned none");
            Assert.AreEqual(networkCoverage, queryResults.First().TimeSeries);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllByLocationId_ProjectContainsNetworkCoverageButNotTimeDependent_shouldReturnNull()
        {

            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";
            var networkCoverage = CreateNetworkCoverage(locationId, parameterId);

            var project = new Project();
            project.RootFolder.Add(networkCoverage);

            var strategy = new NetworkCoverageTimeSeriesAggregator(NetworkDiscretization);
            var context = new ExtendedQueryContext(project, strategy);

            try
            {
                networkCoverage.IsTimeDependent = false;

                // actual test             
                var queryResults = context.GetAllByLocationId(locationId);

                // checks
                Assert.IsNotNull(queryResults);
                Assert.IsFalse(queryResults.Any(), "Should not have query results, but found some");
            }
            finally
            {
                networkCoverage.IsTimeDependent = true;
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllByParameterIdAndLocationId_ProjectContainsNetworkCoverageButNotTimeDependent_shouldReturnNull()
        {
            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";
            var networkCoverage = CreateNetworkCoverage(locationId, parameterId);

            var project = new Project();
            project.RootFolder.Add(networkCoverage);

            var strategy = new NetworkCoverageTimeSeriesAggregator(NetworkDiscretization);

            var context = new ExtendedQueryContext(project, strategy);

            try
            {
                networkCoverage.IsTimeDependent = false;

                // actual test             
                var queryResults = context.GetAllByParameterIdAndLocationId(parameterId, locationId);

                // checks
                Assert.IsNotNull(queryResults);
                Assert.IsFalse(queryResults.Any());
            }
            finally
            {
                networkCoverage.IsTimeDependent = true;
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetTimeSeriesByParameter_ProjectContainsNetworkCoverageButNotTimeDependent_ShouldNotReturnAnyQueryResults()
        {
            // setup
            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";
            var networkCoverage = CreateNetworkCoverage(locationId, parameterId);

            var project = new Project();
            project.RootFolder.Add(networkCoverage);

            var strategy = new NetworkCoverageTimeSeriesAggregator(NetworkDiscretization);

            var context = new ExtendedQueryContext(project, strategy);

            try
            {
                networkCoverage.IsTimeDependent = false;

                // actual test             
                var queryResults = context.GetAllByParameterId(parameterId);

                // checks
                Assert.IsNotNull(queryResults);
                Assert.IsFalse(queryResults.Any(), 
                    "Found query results matching parameterId: " + parameterId + ". This should not be the possible if the target object is not time dependent");

            }
            finally
            {
                networkCoverage.IsTimeDependent = true;
            }
        }

        [Test]
        [Ignore]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void GetTimeSeriesByParameter_ProjectContainsNetworkCoverage_ShouldReturnTimeSeries()
        {
            // this test is hypothetical; at this moment, no other coverages, other than discretization,
            // hold location id's. In future this might change... 

            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";
            var networkCoverage = CreateNetworkCoverage(locationId, parameterId);

            var project = new Project();
            project.RootFolder.Add(networkCoverage);

            var strategy = new NetworkCoverageTimeSeriesAggregator(NetworkDiscretization);

            var context = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResults = context.GetAllByParameterId(parameterId);

            // checks
            Assert.IsNotNull(queryResults);
            string failMessage = string.Format("There are no query result found matching parameterId: {0}. There should be at least one", parameterId);
            Assert.IsTrue(queryResults.Any(), failMessage);
            Assert.AreEqual(networkCoverage, queryResults.First().TimeSeries);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetInputTimeSeriesByLocationAndParameterIdShouldReturnBoundaryConditionTS()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            var strategy = new FeatureDataTimeSeriesAggregator();
            
            var project = new Project();
            project.RootFolder.Add(model);

            var context = new ExtendedQueryContext(project, strategy);

            //test
            var queryResults = context.GetAllByParameterIdAndLocationId(parameterIdWlts, locationIdNode3);

            //checks
            Model1DBoundaryNodeData boundaryCondition =
                model.BoundaryConditions.FirstOrDefault(n => n.Node.Name == locationIdNode3);
            Assert.IsNotNull(boundaryCondition);
            for (int i = 0; i < boundaryCondition.Data.Components[0].Values.Count; i++)
            {
                AggregationResult aggregationResult = queryResults.First();
                IFunction timeSeries = aggregationResult.TimeSeries;
                Assert.AreEqual(boundaryCondition.Data.Components[0].Values[i],
                                timeSeries.Components[0].Values[i]);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetInputTimeSeriesByLocationIdShouldReturnBoundaryConditionTS()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            var strategy = new FeatureDataTimeSeriesAggregator();

            var project = new Project();
            project.RootFolder.Add(model);

            var context = new ExtendedQueryContext(project, strategy);

            //test
            var queryResults = context.GetAllByLocationId(locationIdNode3);

            //checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any());
            Assert.AreEqual(2, queryResults.Count());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetInputTimeSeriesByParameterIdShouldReturnBoundaryConditionTS()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            var strategy = new FeatureDataTimeSeriesAggregator();

            var project = new Project();
            project.RootFolder.Add(model);

            var context = new ExtendedQueryContext(project, strategy);


            //test
            var queryResults = context.GetAllByParameterId(parameterIdWlts);

            //checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any());
            Assert.AreEqual(1, queryResults.Count());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetInputTimeSeries_ModelContainsLateralSourceSaltData_ShouldReturnSaltTimeSeries()
        {
            // setup
            const string parameterIdSaltDischLoadTs = "salt_discharge";//"Salt discharge load time series";        

            var model = CreateDemoNetworkWithLateralSources();
            var strategy = new FeatureDataTimeSeriesAggregator();

            var project = new Project();
            project.RootFolder.Add(model);

            var context = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResults = context.GetAllByParameterId(parameterIdSaltDischLoadTs);

            //checks
            Assert.IsNotNull(queryResults);
            string failMessage = string.Format("There are no query results found matching parameterId: {0}. There should be at least one", parameterIdSaltDischLoadTs);
            Assert.IsTrue(queryResults.Any(), failMessage);
            foreach (var timeSeriesQueryResult in queryResults)
            {
                Console.WriteLine("{0}{1}", timeSeriesQueryResult.ParameterId, timeSeriesQueryResult.LocationId);
            }
            Assert.AreEqual(1, queryResults.Count());

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllTimeSeriesByParameter_OneFeaturCoverageContainingOneFeatureLocation_ShouldReturnOneTimeSeries()
        {
            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";

            const bool timeDependent = true;
            var featureCoverage = CreateFeaturCoverage(locationId, parameterId, timeDependent);

            var project = new Project();
            project.RootFolder.Add(featureCoverage);

            var strategy = new FeatureCoverageTimeSeriesAggregator();
            var context = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResults = context.GetAllByLocationId(locationId);
            foreach (var result in queryResults)
            {
                var aap = result.LocationId;
            }
            // checks
            Assert.IsNotNull(queryResults);
            Assert.AreEqual(1, queryResults.Count());
            var queryResult = queryResults.First();
            Assert.AreEqual(featureCoverage, queryResult.TimeSeries);
            Assert.IsNotNull(queryResult.Geometry);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllTimeSeriesByParameter_OneFeaturCoverageContainingTheParameterName_ShouldReturnOneTimeSeries()
        {
            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";

            const bool timeDependent = true;
            var featureCoverage = CreateFeaturCoverage(locationId, parameterId, timeDependent);

            var project = new Project();
            project.RootFolder.Add(featureCoverage);

            var strategy = new FeatureCoverageTimeSeriesAggregator();
            var context = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResult = context.GetAllByParameterId(parameterId);

            // checks
            Assert.IsNotNull(queryResult);
            Assert.IsTrue(queryResult.Any());
            Assert.AreEqual(3, queryResult.Count());
            Assert.AreEqual(featureCoverage, queryResult.First().TimeSeries);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetTimeSeriesByParameterAndLocation_OneFeaturCoverageMatchesLocationAndParameter_ShouldReturnOneTimeSeries()
        {
            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";

            const bool timeDependent = true;
            var featureCoverage = CreateFeaturCoverage(locationId, parameterId, timeDependent);

            var project = new Project();
            project.RootFolder.Add(featureCoverage);

            var strategy = new FeatureCoverageTimeSeriesAggregator();
            var context = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResult = context.GetAllByParameterIdAndLocationId(parameterId, locationId);

            // checks
            Assert.IsNotNull(queryResult);
            Assert.IsTrue(queryResult.Any());
            Assert.AreEqual(1, queryResult.Count());
            Assert.AreEqual(featureCoverage, queryResult.First().TimeSeries);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetTimeSeriesByParameterAndLocation_ProjectContainsFeatureCoverageButNotTimeDependent_shouldReturnNull()
        {
            // setup
            const string locationId = "ergens";
            const string parameterId = "Q";

            const bool timeDependent = false;
            var featureCoverage = CreateFeaturCoverage(locationId, parameterId, timeDependent);

            var project = new Project();
            project.RootFolder.Add(featureCoverage);

            var strategy = new FeatureCoverageTimeSeriesAggregator();
            var context = new ExtendedQueryContext(project, strategy);

            // actual test             
            var queryResult = context.GetAllByParameterId(parameterId);

            // checks
            Assert.IsFalse(queryResult.Any());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAll_DemoNetwork_CanPrintResults()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();

            var project = new Project();
            project.RootFolder.Add(model);

            var context = new ExtendedQueryContext(project);

            //test
            var queryResults = context.GetAll();

            foreach (var queryResult in queryResults)
            {
                string value = queryResult.ToString();
                Assert.IsNotEmpty(value);
                Console.WriteLine(value);                
            }
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void GetAllByFeatureType_DemoNetwork_CanFindSpecificFeatures()
        {
            // setup
            var model = CreateDemoNetworkWithLateralSources();
            model.Initialize();

            var project = new Project();
            project.RootFolder.Add(model);

            var context = new ExtendedQueryContext(project);

            // actual test
            var queryResults = context.GetAllByFeatureType<NetworkLocation>();

            // checks
            Assert.IsNotNull(queryResults);
            Assert.IsTrue(queryResults.Any(), "There are no query results");

            foreach (var queryResult in queryResults)
            {
                string value = queryResult.ToString();
                Assert.IsNotEmpty(value);
                Console.WriteLine(value);
            }
        }
    }
}