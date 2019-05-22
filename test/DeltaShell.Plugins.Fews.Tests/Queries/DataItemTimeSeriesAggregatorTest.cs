using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests.Queries
{
    [TestFixture]
    [Category(TestCategory.Integration)] // starts Delta Shell in FewsAdapterTestBase, imports model! (20sec)
    public class DataItemTimeSeriesAggregatorTest: FewsAdapterTestBase
    {
        [Test]
        public void ModelContainsWindShouldReturn5TimeSeries()
        {
            // setup
            var model = CreateDemoModel();
            SetWindFunction(model);
            var project = new Project();
            project.RootFolder.Add(model);
            
            // call
            var strategy = new DataItemTimeSeriesAggregator { DataItems = project.GetAllItemsRecursive() };
            var queryResults = strategy.GetAll();

            // test             
            Assert.AreEqual(5,queryResults.Count());
            /* 2 for WindFunction
             * 3 for MeteoFunction */
        }

        [Test]
        public void ModelContainsWindShouldReturnWindVelocity()
        {
            // setup
            var model = CreateDemoModel();
            SetWindFunction(model);
            var project = new Project();
            project.RootFolder.Add(model);

            // call
            var strategy = new DataItemTimeSeriesAggregator { DataItems = project.GetAllItemsRecursive() };
            var queryResults = strategy.GetAll();
            var velocity = queryResults.Where(r => r.ParameterId == "Wind Velocity");

            // test             
            Assert.AreEqual(1, velocity.Count());
        }


        [Test]
        public void GetAllTimeSeriesProjectContainsLinkedTimeSeriesShouldReturnLinkedTimeSeries()
        {
            // setup
            var timeSeries = new TimeSeries { Name = "timeSerie 1.Q" };
            var sourceDataItem = new DataItem { Value = timeSeries, ValueType = typeof(IFunction), Role = DataItemRole.Input };

            var targetDataItem = new DataItem { Name = "target", ValueType = typeof(IFunction), Role = DataItemRole.Input };
            targetDataItem.LinkTo(sourceDataItem);

            var project = new Project();
            project.RootFolder.Add(sourceDataItem) ;
            project.RootFolder.Add(targetDataItem);

            var strategy = new DataItemTimeSeriesAggregator { DataItems = project.GetAllItemsRecursive() };

            // call 
            var queryResult = strategy.GetAll().FirstOrDefault();

            // checks
            Assert.IsNotNull(queryResult);
            Assert.AreEqual(timeSeries, queryResult.TimeSeries);
        }

        [Test]
        public void GetInputTimeSeriesProjectContainsMultipleLinkedTimeSeriesShouldReturnLinkedTimeSeries()
        {
            // setup
            var timeSeries = new TimeSeries {Name = "timeSerie 1.Q"};
            var sourceDataItem = new DataItem
                                     {Value = timeSeries, ValueType = typeof (IFunction), Role = DataItemRole.Input};

            var targetDataItem1 = new DataItem
                                      {Name = "target1", ValueType = typeof (IFunction), Role = DataItemRole.Input};
            var targetDataItem2 = new DataItem
                                      {Name = "target2", ValueType = typeof (IFunction), Role = DataItemRole.Input};
            targetDataItem1.LinkTo(sourceDataItem);
            targetDataItem2.LinkTo(sourceDataItem);

            var project = new Project();
            project.RootFolder.Add(targetDataItem1);
            project.RootFolder.Add(targetDataItem2);
            // add source item as the last one! to asure that
            // evaluation order is target1, target2, source...
            project.RootFolder.Add(sourceDataItem);

            var strategy = new DataItemTimeSeriesAggregator { DataItems = project.GetAllItemsRecursive() };

            // call 
            var queryResult = strategy.GetAll(); // <- should be exactly one!!

            // checks
            Assert.IsNotNull(queryResult);
            Assert.AreEqual(timeSeries, queryResult.First().TimeSeries);
        }

        private void SetWindFunction(WaterFlowModel1D model)
        {
            DateTime t = model.StartTime;
            model.Wind.Arguments[0].SetValues(new List<DateTime> { t, t.AddMinutes(5), t.AddMinutes(10) });
            var values = model.Wind.Arguments[0].Values;
            foreach (var value in values)
            {
                for (int i = 0; i < model.Wind.Components.Count; i++)
                {
                    model.Wind.Components[i][value] = 4.0 + i * 10;
                }
            }
        }
    }
}