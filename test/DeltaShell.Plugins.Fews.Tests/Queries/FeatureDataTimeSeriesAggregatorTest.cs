using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.ModelExchange.Queries;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests.Queries
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class FeatureDataTimeSeriesAggregatorTest : FewsAdapterTestBase
    {
        private const string LocationIdLateral = "lateral test";
        private const string ParameterIdSaltDischLoadTs = "salt_discharge";
        private RealTimeControlModel rtcModel;
        private ControlGroup controlGroup;
       
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
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SerializeTimeSeriesInBoundaryNodeData()
        {
            // create network
            var timeSeries = HydroTimeSeriesFactory.CreateFlowTimeSeries();

            const string testValue = "test";
            timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] = testValue;
            timeSeries.Attributes[FunctionAttributes.StandardName] = testValue;
            var boundaryData = new WaterFlowModel1DBoundaryNodeData
            {
                Name = "aap",
                DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries,
                Data = timeSeries
            };

            DeltaShellApplication app;
            using (app = GetRunningDSApplication())
            {
                var project = new Project("test project");
                app.Project = project;
                project.RootFolder.Add(new DataItem(boundaryData));
                string projPath = "boundProj";
                app.SaveProjectAs(projPath);
                
                //reload
                app.OpenProject(projPath);
                var retrievedProject = app.Project;
                IDataItem[] retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
                var retrievedTimeSeries = (WaterFlowModel1DBoundaryNodeData)retrievedDataItems[0].Value;

                //compare
                Assert.AreEqual(retrievedTimeSeries.Data.Components[0].Attributes[FunctionAttributes.StandardName], boundaryData.Data.Components[0].Attributes[FunctionAttributes.StandardName]);
                Assert.AreEqual(retrievedTimeSeries.Data.Attributes[FunctionAttributes.StandardName], boundaryData.Data.Attributes[FunctionAttributes.StandardName]);
            
            }
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllModelContainsNonTimeSeriesBoundariesShouldNotReturnBoundaries()
        {
            var model = CreateDemoModel();

            // Set boundary conditions
            var constantFlowBoundary = model.BoundaryConditions.First(bc => bc.Feature == model.Network.Nodes.First(s => !s.IsConnectedToMultipleBranches));
            constantFlowBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            constantFlowBoundary.Data = null; // remove time function
            constantFlowBoundary.Flow = 2.0;

            var boundaryFeature = model.Network.Branches[1].Target;
            boundaryFeature.Name = "Q(h)";

            var qhBoundary = model.BoundaryConditions.First(bc => bc.Feature == boundaryFeature);
            qhBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable;
            qhBoundary.WaterLevel = 1.0;

            // Get a flow timeseries 
            var flowWaterLevelSeries = new FlowWaterLevelTable();
            flowWaterLevelSeries[2.0] = 5.0;
            qhBoundary.Data = flowWaterLevelSeries;
           
            var project = new Project();
            project.RootFolder.Add(model);
            var strategy = new FeatureDataTimeSeriesAggregator { DataItems = project.GetAllItemsRecursive() };

            // Actual test             
            var queryResults = strategy.GetAll().ToList();

            Assert.IsFalse(queryResults.Any(), "Should not return any boundary");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllModelContainsLateralSourceDataShouldReturn3TimeSeries()
        {
            // setup
            var model = CreateDemoModel();
            WaterFlowModel1DLateralSourceData waterFlowModel1DLateralSourceData =
                GetWaterFlowModel1DLateralSourceData(LocationIdLateral, false);
            model.LateralSourceData.Add(waterFlowModel1DLateralSourceData);
            var project = new Project();
            project.RootFolder.Add(model);
            var strategy = new FeatureDataTimeSeriesAggregator {DataItems = project.GetAllItemsRecursive()};

            // actual test             
            var queryResults = strategy.GetAll();

            IEnumerable<AggregationResult> aggregationResults = queryResults as IList<AggregationResult> ?? queryResults.ToList();
            PrintResults(aggregationResults);

            //checks
            Assert.AreEqual(3, aggregationResults.Count());
        }
       
        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllModelContainsSaltTimeSeriesShouldHaveResults()
        {
            // setup
            var model = CreateDemoModel();
            WaterFlowModel1DLateralSourceData waterFlowModel1DLateralSourceData =
                GetWaterFlowModel1DLateralSourceData(LocationIdLateral, true);
            model.LateralSourceData.Add(waterFlowModel1DLateralSourceData);
            model.UseSalt = true;
            model.Initialize();

            var project = CreateProjectWithModel(model);
            var strategy = new FeatureDataTimeSeriesAggregator { DataItems = project.GetAllItemsRecursive() };

            // call 
            var results = strategy.GetAll();
            var queryResult =
                strategy.GetAll().SingleOrDefault(i => i.ParameterId == ParameterIdSaltDischLoadTs && i.LocationId == LocationIdLateral);
            
            //checks
            Assert.IsNotNull(queryResult);
            Assert.IsTrue(queryResult.TimeSeries.Arguments.Any());
            Assert.IsTrue(queryResult.TimeSeries.Components.Any());
            var lateralType = results.Where(r => r.LocationType == "LateralSource");
            Assert.IsTrue(lateralType.Any());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllByLocationIdRTCModelTimeconditionShouldHaveDataOnReturnedTimeSeries()
        {
            // setup
            const string conditionName = "TimeCondition";
            
            IDataItem weirDataItem;
            var locId = GetRtcModelAndValueConverter(conditionName, out weirDataItem);
            var timeSeries = CreateTimeSeries(false);
            
            var input = new Input();
            controlGroup.Inputs.Add(input);

            rtcModel.GetDataItemByValue(input).LinkTo(weirDataItem);

            var timeCondition = new TimeCondition
            {
                Name = conditionName,
                LongName = "TimeConditionTimeCondition",
                Reference = "Implicit",
                Input = input,
                TimeSeries = timeSeries,
                Extrapolation = ExtrapolationType.Periodic,
                InterpolationOptionsTime = InterpolationType.Linear
            };
            
            var output = new Output();
            controlGroup.Outputs.Add(output);

            weirDataItem.LinkTo(rtcModel.GetDataItemByValue(output));
            
            controlGroup.Conditions.Add(timeCondition);

            var proj = new Project("test fews proj");
            proj.RootFolder.Add(rtcModel);
            
            var projectContext = new ExtendedQueryContext(proj, new FeatureDataTimeSeriesAggregator());

            // call 
            IEnumerable<AggregationResult> queryResults = projectContext.GetAll();
            foreach (var result in queryResults)
            {
                Console.WriteLine(result);
            }
            // checks
            var queryResult = projectContext.GetAllByLocationId(locId).FirstOrDefault();
            Assert.IsNotNull(queryResult);
            Assert.AreEqual(timeSeries, queryResult.TimeSeries);
            foreach (var tuple in queryResult.TimeSeriesIterator())
            {
                Console.WriteLine(tuple.First.ToString(CultureInfo.InvariantCulture), tuple.Second.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllByLocationIdProjContainsRTCModelPidRuleShouldHaveTimeSeriesDataOnReturnedResults()
        {
            // setup
            string pidRuleName = "PIDRule";
            IDataItem weirDataItem;
            var locId = GetRtcModelAndValueConverter(pidRuleName, out weirDataItem);
            var timeSeries = CreateTimeSeries(true);
            var rule = new PIDRule
            {
                Name = pidRuleName,
                Kp = 0,
                Ki = 0,
                Kd = 0,
                Setting = new Setting { Min = 0, Max = 0, MaxSpeed = 0 },
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries
            };
            
            var input = new Input();
            rule.Inputs.Add(input);
            rtcModel.ControlGroups[0].Inputs.Add(input);

            rtcModel.GetDataItemByValue(input).LinkTo(weirDataItem);
            
            var output = new Output();
            rule.Outputs.Add(output);
            rtcModel.ControlGroups[0].Outputs.Add(output);
            controlGroup.Rules.Add(rule);

            weirDataItem.LinkTo(rtcModel.GetDataItemByValue(output));


            var proj = new Project("test fews proj");
            proj.RootFolder.Add(rtcModel);

            var projectContext = new ExtendedQueryContext(proj, new FeatureDataTimeSeriesAggregator());

            // call 
            var queryResult = projectContext.GetAllByLocationId(locId).FirstOrDefault();
            foreach (var tuple in queryResult.TimeSeriesIterator())
            {
                Console.WriteLine(tuple.First.ToString(CultureInfo.InvariantCulture), tuple.Second.ToString(CultureInfo.InvariantCulture));
            }
            // checks
            Assert.IsNotNull(queryResult);
            Assert.AreEqual(timeSeries, queryResult.TimeSeries);
          }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GetAllProjContainsRTCModelPidRuleShouldCreateCsv()
        {
            // setup
            string pidRuleName = "PIDRule";
            IDataItem weirDataItem;
            GetRtcModelAndValueConverter(pidRuleName, out weirDataItem);
            var timeSeries = CreateTimeSeries(true);

            var rule = new PIDRule
            {
                Name = pidRuleName,
                Kp = 0,
                Ki = 0,
                Kd = 0,
                Setting = new Setting { Min = 0, Max = 0, MaxSpeed = 0 },
                TimeSeries = timeSeries,
            };

            controlGroup.Rules.Add(rule);

            // add input to group
            var input = new Input();
            controlGroup.Inputs.Add(input);

            // add input to rule
            rule.Inputs.Add(input);

            rtcModel.GetDataItemByValue(input).LinkTo(weirDataItem);
            
            var output = new Output();
            rule.Outputs.Add(output);
            controlGroup.Outputs.Add(output);
            weirDataItem.LinkTo(rtcModel.GetDataItemByValue(output));

            var proj = new Project("test fews proj");
            proj.RootFolder.Add(rtcModel);

            var projectContext = new ExtendedQueryContext(proj, new FeatureDataTimeSeriesAggregator());

            // call 
            var queryResults = projectContext.GetAll();
            IEnumerable<AggregationResult> aggregationResults = queryResults as IList<AggregationResult> ?? queryResults.ToList();
            foreach (var result in aggregationResults)
            {
                Console.WriteLine(result);
            }
            // checks
            
            var lines = AggregationResult.ToSeperatedValues(aggregationResults);
            var outputFileName = Path.Combine("TestExportFiles", "rtcPidModel.csv");
            FileUtils.CreateDirectoryIfNotExists("TestExportFiles");
            File.WriteAllLines(outputFileName, lines);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetInputTimeSeriesByLocationProjContainsRTCModelHydraulicRule()
        {
            // setup
            string ruleName = "hydrauRule";
            IDataItem weirDataItem;
            var locId = GetRtcModelAndValueConverter(ruleName, out weirDataItem);

            var rule = new HydraulicRule {Name = ruleName};
            rule.Function[-10.0] = -10.0;
            rule.Function[10.0] = 10.0;
            
            var input = new Input();
            rule.Inputs.Add(input);
            rtcModel.ControlGroups[0].Inputs.Add(input);

            rtcModel.GetDataItemByValue(input).LinkTo(weirDataItem);

            var output = new Output();
            rule.Outputs.Add(output);

            weirDataItem.LinkTo(rtcModel.GetDataItemByValue(output));

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
            controlGroup.Rules.Add(rule);

            var proj = new Project("test fews proj");
            proj.RootFolder.Add(rtcModel);

            var projectContext = new ExtendedQueryContext(proj, new FeatureDataTimeSeriesAggregator());

            // call 
            var queryResult = projectContext.GetAllByLocationId(locId).FirstOrDefault();
            // see ControlGroup.GetFeatureDataForFewsAdapter(); TimeRule, PidRule and IntervalRule are supported
            Assert.IsNull(queryResult);
        }

        
        #region privates
        private WaterFlowModel1DLateralSourceData GetWaterFlowModel1DLateralSourceData(string id, bool useSalt)
        {
            DateTime now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            var lateralSource = new LateralSource();
            var waterFlowModel1DLateralSourceData = new WaterFlowModel1DLateralSourceData
            {
                Feature = lateralSource,
                DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries
            };
            lateralSource.Name = id;
            waterFlowModel1DLateralSourceData.Data[t] = 1.0;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(30)] = 1.0;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(60)] = 1.5;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(120)] = 1.0;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(180)] = 0.5;
            waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            if (useSalt)
            {
                waterFlowModel1DLateralSourceData.UseSalt = true;
                waterFlowModel1DLateralSourceData.SaltLateralDischargeType = SaltLateralDischargeType.MassTimeSeries;
                waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(30)] = 5.0;
                waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(60)] = 1.5;
                waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(120)] = 7.0;
                waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(180)] = 4.5;
                waterFlowModel1DLateralSourceData.SaltMassDischargeConstant = 2;
                waterFlowModel1DLateralSourceData.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)] = 3.0;
                waterFlowModel1DLateralSourceData.SaltConcentrationDischargeConstant = 4;
            }
            return waterFlowModel1DLateralSourceData;
        }

        private string GetRtcModelAndValueConverter(string pidRuleName, out IDataItem weirDataItem)
        {
            rtcModel = new RealTimeControlModel();

            const string controlgroupName = "myControlGroup";
            string locId = string.Concat(controlgroupName, "_", pidRuleName);

            controlGroup = new ControlGroup {Name = controlgroupName};
            rtcModel.ControlGroups.Add(controlGroup);

            
            var model = CreateDemoModel();
            var weir = new Weir("weir") {Geometry = new Point(model.Network.Branches[0].Geometry.Coordinates[0]) };
            model.Network.Branches[0].BranchFeatures.Add(weir);

            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(rtcModel);
            hydroModel.Activities.Add(model);

            var itemsForWeir = model.GetChildDataItems(weir).Where(ei => (ei.Role & DataItemRole.Input) == DataItemRole.Input);
            Assert.AreNotEqual(0, itemsForWeir.Count());

            weirDataItem = itemsForWeir.First();

            return locId;
        }
        #endregion
    }
}