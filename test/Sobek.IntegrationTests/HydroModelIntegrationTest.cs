using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.IO;
using DelftTools.Utils.UndoRedo;
using DeltaShell.Core.Services;
using DeltaShell.Dimr;
using DeltaShell.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using SharpTestsEx;
using AggregationOptions = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.AggregationOptions;
using Application = System.Windows.Forms.Application;
using ElementSet = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.ElementSet;
using Point = NetTopologySuite.Geometries.Point;
using QuantityType = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.QuantityType;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class HydroModelIntegrationTest : NHibernateIntegrationTestBase
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new WaterQualityModelApplicationPlugin());
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());

            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroModelWithFlow1DDoesNotLoseRoughnessData() // Issue #: SOBEK3-705
        {
            // setup
            var flow1DModel = new WaterFlowModel1D();
            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(flow1DModel);

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            hydroModel.Region.SubRegions.Add(network);
            flow1DModel.GetDataItemByValue(flow1DModel.Network).LinkTo(hydroModel.GetDataItemByValue(network));
            
            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];

            var floodPlainRoughnessSection = flow1DModel.RoughnessSections[1];

            var functionOfH = new Function("functionOfH");
            functionOfH.Arguments.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 6.0, 8.0, 10.0 } });
            functionOfH.Components.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 1.0, 5.0, 9.0 } });

            var functionOfQ = new Function("functionOfQ");
            functionOfQ.Arguments.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 2.0, 6.0, 10.0 } });
            functionOfQ.Components.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 3.0, 5.0, 7.0 } });

            // add functions
            floodPlainRoughnessSection.AddHRoughnessFunctionToBranch(branch1, functionOfH);
            floodPlainRoughnessSection.AddQRoughnessFunctionToBranch(branch2, functionOfQ);
            
            Assert.IsNotNull(floodPlainRoughnessSection.FunctionOfH(branch1));
            Assert.IsNotNull(floodPlainRoughnessSection.FunctionOfQ(branch2));

            // clone hydro model
            var clonedHydroModel = (HydroModel)hydroModel.DeepClone();
            var clonedFlow1DModel = (WaterFlowModel1D)clonedHydroModel.Activities.First(a => a is WaterFlowModel1D);
            var clonedFloodPlainRoughnessSection = clonedFlow1DModel.RoughnessSections[1];

            // compare functionOfH
            var clonedFunctionOfH = clonedFloodPlainRoughnessSection.FunctionOfH(clonedFlow1DModel.Network.Branches[0]);

            var originalArguments = (IList<double>)functionOfH.Arguments[0].Values;
            var clonedArguments = (IList<double>)clonedFunctionOfH.Arguments[0].Values;
            Assert.IsTrue(CompareLists(originalArguments, clonedArguments));

            var originalComponents = (IList<double>)functionOfH.Components[0].Values;
            var clonedComponents = (IList<double>)clonedFunctionOfH.Components[0].Values;
            Assert.IsTrue(CompareLists(originalComponents, clonedComponents));

            // compare functionOfQ
            var clonedFunctionOfQ = clonedFloodPlainRoughnessSection.FunctionOfQ(clonedFlow1DModel.Network.Branches[1]);

            originalArguments = (IList<double>)functionOfQ.Arguments[0].Values;
            clonedArguments = (IList<double>)clonedFunctionOfQ.Arguments[0].Values;
            Assert.IsTrue(CompareLists(originalArguments, clonedArguments));

            originalComponents = (IList<double>)functionOfQ.Components[0].Values;
            clonedComponents = (IList<double>)clonedFunctionOfQ.Components[0].Values;
            Assert.IsTrue(CompareLists(originalComponents, clonedComponents));
        }

        private static bool CompareLists<T>(IList<T> list1, IList<T> list2) where T : IComparable
        {
            if (list1.Count != list2.Count) return false;

            for (var i = 0; i < list1.Count; i++)
            {
                if (list1[i].CompareTo(list2[i]) != 0) return false;
            }
            return true;
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]
        public void RunFlowAloneAndThenFlowAndRRCombinedTools9662()
        {
            RainfallRunoffModel rr;
            WaterFlowModel1D flow;
            var hydroModel = CreateFlowRRModel(out rr, out flow);

            ActivityRunner.RunActivity(flow);
            Assert.AreEqual(ActivityStatus.Failed, flow.Status);

            ActivityRunner.RunActivity(hydroModel);
            Assert.AreEqual(ActivityStatus.Cleaned, flow.Status);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("You cannot run RR and FLow sequentially anymore in dimr")]
        public void RunRRAndFlowSequentially()
        {
            RainfallRunoffModel rr;
            WaterFlowModel1D flow;
            var hydroModel = CreateFlowRRModel(out rr, out flow);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.OfType<SequentialActivity>().First();

            ActivityRunner.RunActivity(hydroModel);
            Assert.AreNotEqual(ActivityStatus.Failed, rr.Status);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunRRAndFlowParallel()
        {
            RainfallRunoffModel rr;
            WaterFlowModel1D flow;
            var hydroModel = CreateFlowRRModel(out rr, out flow);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.OfType<ParallelActivity>().First();

            ActivityRunner.RunActivity(hydroModel);
            Assert.AreNotEqual(ActivityStatus.Failed, rr.Status);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]   // Will be obsolete anyway with introduction of DIMR. 
        public void RunFlowAndRRWithValueExchange()
        {
            RainfallRunoffModel rr;
            WaterFlowModel1D flow;
            var hydroModel = CreateFlowRRModel(out rr, out flow);

            // grab linked coverages
            var flowOutputWaterLevel = flow.OutputWaterLevel;
            var rrWaterLevel = rr.InputWaterLevel;
            var rrOutflows = rr.BoundaryDischarge;
            var flowInflows = flow.Inflows;

            hydroModel.Initialize(); 
            hydroModel.Execute();
            
            Assert.AreEqual(16.9751, (double)flowInflows.Components[0].Values[0], 0.001, "discharge on boundary not as expected");
            Assert.AreEqual(33.9503, (double)flowInflows.Components[0].Values[1], 0.001, "discharge on lateral not as expected");
            
            hydroModel.Execute();

            // asserts
            Assert.AreEqual(3, flowOutputWaterLevel.Time.Values.Count, "flow outgoing data available not as expected");
            Assert.AreEqual(1, rrWaterLevel.Time.Values.Count, "rr incoming data available not as expected");
            Assert.AreEqual(2, rrWaterLevel.Components[0].Values.Count, "rr incoming data slots available not as expected, should be equal to time * (catchments)");

            Assert.AreEqual(3, rrOutflows.Time.Values.Count, "rr outgoing data available not as expected");
            Assert.AreEqual(1, flowInflows.Time.Values.Count, "flow incoming data available not as expected");
            Assert.AreEqual(2, flowInflows.Components[0].Values.Count, "flow incoming data slots available not as expected, should be equal to time * (lat+bound)");
            
            Assert.AreEqual(13.1535, (double)rrWaterLevel.Components[0].Values[0], 0.001, "water level on catchment not as expected");
            
            hydroModel.Cleanup();
        }

        private static HydroModel CreateFlowRRModel(out RainfallRunoffModel rr, out WaterFlowModel1D flow)
        {
            // create network: one branch + lateral
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new[] {new Point(0, 0), new Point(0, 100.0)});
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(network.Branches.First(),
                                                                 CrossSectionDefinitionYZ.CreateDefault(), 20.0);

            ModelTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);
            
            var lateral = new LateralSource {Chainage = 15.0}; //not on grid
            network.Branches.First().BranchFeatures.Add(lateral);
            lateral.Geometry = new Point(0, 15.0);

            // create hydro model
            flow = new WaterFlowModel1D();
            rr = new RainfallRunoffModel();
            var hydroModel = new HydroModel {Activities = {rr, flow}};
            hydroModel.StopTime = hydroModel.StartTime.Add(hydroModel.TimeStep + hydroModel.TimeStep);

            var basin = new DrainageBasin();
            hydroModel.Region.SubRegions.Add(network);
            hydroModel.Region.SubRegions.Add(basin);

            // link regions to models
            rr.GetDataItemByValue(rr.Basin).LinkTo(hydroModel.GetDataItemByValue(basin));
            flow.GetDataItemByValue(flow.Network).LinkTo(hydroModel.GetDataItemByValue(network));

            // generate flow grid
            HydroNetworkHelper.GenerateDiscretization(flow.NetworkDiscretization, true, false, 10, false, 1, true,
                                                      true, true, 50, network.Channels.ToList());

            // set flow waterdepth to 9.0 (level = -1)
            flow.InitialConditions.DefaultValue = 9.0;

            // add catchment to basin
            var catchment = new Catchment
                {
                    IsGeometryDerivedFromAreaSize = true,
                    CatchmentType = CatchmentType.Unpaved,
                    Name = "c1"
                };
            catchment.SetAreaSize(400000);
            basin.Catchments.Add(catchment);

            // add catchment 2 to basin
            var catchment2 = new Catchment
                {
                    IsGeometryDerivedFromAreaSize = true,
                    CatchmentType = CatchmentType.Unpaved,
                    Name = "c2"
                };
            catchment2.SetAreaSize(800000);
            basin.Catchments.Add(catchment2);

            // create links
            var node = network.HydroNodes.First();
            catchment.LinkTo(node);
            catchment2.LinkTo(lateral);

            // set model data to Q(t) (to allow linking)
            flow.BoundaryConditions.First(bc => bc.Feature == node).DataType =
                WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            flow.LateralSourceData.First(lc => lc.Feature == lateral).DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;

            // set meteo for RR
            SetGlobalMeteoDataForTesting(rr);

            return hydroModel;
        }
        private static IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaterFlowModel1DExporter();
            yield return new RainfallRunoffModelExporter();
            yield return new RealTimeControlModelExporter();
        }

        private static void SetGlobalMeteoDataForTesting(RainfallRunoffModel rrModel)
        {
            rrModel.Precipitation.DataDistributionType = MeteoDataDistributionType.Global;
            rrModel.Evaporation.DataDistributionType = MeteoDataDistributionType.Global;

            var days = Math.Ceiling(rrModel.StopTime.Subtract(rrModel.StartTime).TotalDays);

            for (int i = 0; i <= days; i++)
            {
                rrModel.Evaporation.Data[rrModel.StartTime.AddDays(i)] = 0.0;
            }

            var j = 100.0;
            for (var current = rrModel.StartTime; current <= rrModel.StopTime; current += rrModel.TimeStep)
            {
                rrModel.Precipitation.Data[current] = j += 100.0;
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CreateWithFlowAndRainfallRunoff()
        {
            var network = new HydroNetwork();
            var basin = new DrainageBasin();
            var region = new HydroRegion { SubRegions = { network, basin } };

            var flowModel = new WaterFlowModel1D();
            var rainfallRunoffModel = new RainfallRunoffModel();
            var hydroModel = new HydroModel
            {
                Region = region, 
                Activities = { flowModel, rainfallRunoffModel }
            };

            // link sub-regions to sub-models
            flowModel.GetDataItemByValue(flowModel.Network).LinkTo(hydroModel.GetDataItemByValue(network));
            rainfallRunoffModel.GetDataItemByValue(rainfallRunoffModel.Basin).LinkTo(hydroModel.GetDataItemByValue(basin));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DeepCloneHydroModelAndCheckCatchment()
        {
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.SobekModels);
            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            
            // add a catchment
            var c1 = new Catchment {CatchmentType = CatchmentType.Unpaved};
            rr.Basin.Catchments.Add(c1);

            // make a clone
            var clonedHydro = (HydroModel)hydroModel.DeepClone();
            var clonedRR = clonedHydro.Activities.OfType<RainfallRunoffModel>().First();

            // asserts
            Assert.AreNotSame(rr.Basin, clonedRR.Basin);

            // assert using reference helper
            var catchmentReferences = TestReferenceHelper.SearchObjectInObjectGraph(c1, clonedHydro);
            catchmentReferences.ForEach(Console.WriteLine);
            Assert.AreEqual(0, catchmentReferences.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DeepCloneHydroModelWithFlow1dModel()
        {
            // create network
            var node1 = new HydroNode { Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Geometry = new Point(0, 100) };

            var channel = new Channel
            {
                Source = node1,
                Target = node2,
                Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(0, 100) })
            };

            var network = new HydroNetwork { Branches = { channel }, Nodes = { node1, node2 } };

            // create hydro model containing flow model
            var flowModel = new WaterFlowModel1D();

            var hydroModel = new HydroModel { Region = new HydroRegion { SubRegions = { network } }, Activities = { flowModel } };

            // link flow model network data item to hydro model region network data item
            flowModel.GetDataItemByTag(WaterFlowModel1DDataSet.NetworkTag).LinkTo(hydroModel.GetDataItemByValue(network));

            // set some data on flow
            flowModel.BoundaryConditions.First().DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            flowModel.NetworkDiscretization[new NetworkLocation(channel, 0)] = 0.0; // add grid point
            
            // clone hydro model
            var clonedHydromodel = (HydroModel) hydroModel.DeepClone();
            var clonedNetwork = clonedHydromodel.Region.SubRegions.OfType<IHydroNetwork>().First();
            clonedNetwork.Name += " (clone)";
            var clonedFlow = clonedHydromodel.Models.OfType<WaterFlowModel1D>().First();

            // assert flow data is not lost
            clonedFlow.BoundaryConditions.First().DataType.Should("boundary node type").Be.EqualTo(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries);
            clonedFlow.NetworkDiscretization.Locations.Values.Count.Should("one grid point").Be.EqualTo(1);

            clonedFlow.NetworkDiscretization.Locations.Values.First().Network.Should().Be.SameInstanceAs(clonedNetwork);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DeepCloneHydroModelAndCheckHydroLinks()
        {
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.SobekModels);
            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var wf = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            // add a catchment
            var c1 = new Catchment { CatchmentType = CatchmentType.Unpaved };
            rr.Basin.Catchments.Add(c1);
            var startNode = new HydroNode("node1") {Geometry = new Point(0, 0)};
            var endNode = new HydroNode("node2") {Geometry = new Point(1000, 0)};
            var br = new Channel(startNode, endNode, 1000);
            var lat = new LateralSource {Branch = br, Chainage = 500};
            br.BranchFeatures.Add(lat);
            wf.Network.Nodes.AddRange(new[] {startNode, endNode});
            wf.Network.Branches.Add(br);
            hydroModel.Region.AddNewLink(c1, lat);

            // make a clone
            var clonedHydroModel = (HydroModel)hydroModel.DeepClone();

            var links = hydroModel.Region.AllHydroObjects.OfType<Catchment>().First().Links;
            var clonedLinks = clonedHydroModel.Region.AllHydroObjects.OfType<Catchment>().First().Links;

            Assert.AreEqual(links.Count, clonedLinks.Count, "cloned model contains different number of hydro links");
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void RunRRInDWAQ_AC1()
        {
            string path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DWAQ_AC1.lit\14\NETWORK.TP");
            
            var hydroModelImporter = new SobekHydroModelImporter(true, false);
            var hydroModel = (HydroModel) hydroModelImporter.ImportItem(path);

            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();

            // fill missing(?) evap data
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries) rr.Evaporation.Data, rr.StartTime, rr.StopTime,
                                                         new TimeSpan(1, 0, 0));

            //about 4800ms locally
            TestHelper.AssertIsFasterThan(10000, () =>
                {
                    // run rr model
                    ActivityRunner.RunActivity(rr);

                    if (rr.Status == ActivityStatus.Failed)
                    {
                        throw new InvalidOperationException("Execute failed");
                    }
                });
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunDWAQ_AC1TwiceAndExpectSameResultsTools9586()
        {
            string path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DWAQ_AC1.lit\14\NETWORK.TP");

            var hydroModelImporter = new SobekHydroModelImporter(true, false);
            var hydroModel = (HydroModel)hydroModelImporter.ImportItem(path);
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();

            flow.HydFileOutput = false;

            // fill missing(?) evap data
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries)rr.Evaporation.Data, rr.StartTime, rr.StopTime,
                                                         new TimeSpan(1, 0, 0));
            var catchment = rr.Basin.Catchments.First();

            // do a run and get rr output discharge
            ActivityRunner.RunActivity(hydroModel); 
            var rrOutputFirstRun = rr.BoundaryDischarge.GetTimeSeries(catchment).Components[0].Values.OfType<double>().ToList();
            var rrInputWaterLevelFirstRun = rr.InputWaterLevel.GetTimeSeries(catchment).Components[0].Values.OfType<double>().ToList();

            // do a second run and get rr output discharge again
            ActivityRunner.RunActivity(hydroModel);
            
            var rrOutputSecondRun = rr.BoundaryDischarge.GetTimeSeries(catchment).Components[0].Values.OfType<double>().ToList();
            var rrInputWaterLevelSecondRun = rr.InputWaterLevel.GetTimeSeries(catchment).Components[0].Values.OfType<double>().ToList();

            // assert the outputs are the same
            Assert.AreEqual(rrOutputFirstRun, rrOutputSecondRun);
            Assert.AreEqual(rrInputWaterLevelFirstRun, rrInputWaterLevelSecondRun);

            // assert the current water level value
            var rrInputWaterLevelValue = rrInputWaterLevelSecondRun.FirstOrDefault();
            Assert.IsTrue(!rrInputWaterLevelValue.Equals(0.0));
            Assert.IsTrue(!rrInputWaterLevelValue.Equals(RainfallRunoffModelDataSet.UndefinedWaterLevel));
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunRRAndFlowSequentialAndSimultaneousAndExpectDifferentResults()
        {
            string path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DWAQ_AC1.lit\14\NETWORK.TP");

            var hydroModelImporter = new SobekHydroModelImporter(true, false);
            var hydroModel = (HydroModel)hydroModelImporter.ImportItem(path);

            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();

            flow.HydFileOutput = false;

            // fill missing(?) evap data
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries)rr.Evaporation.Data, rr.StartTime, rr.StopTime,
                                                         new TimeSpan(1, 0, 0));

            // run sequential
            hydroModel.CurrentWorkflow = hydroModel.Workflows.First(w => w.Name == "RR + Flow1D");
            ActivityRunner.RunActivity(hydroModel);
            
            // get flow water level at random point
            var sequentialResults = flow.OutputFlow.GetTimeSeries(flow.Network.LateralSources.Last()).Components[0].Values.OfType<double>().ToList();

            // run parallel
            hydroModel.CurrentWorkflow = hydroModel.Workflows.First(w => w.Name == "(RR + Flow1D)");
            ActivityRunner.RunActivity(hydroModel);

            // get flow water level at random point
            var simultaneousResults = flow.OutputFlow.GetTimeSeries(flow.Network.LateralSources.Last()).Components[0].Values.OfType<double>().ToList();

            Assert.AreNotEqual(sequentialResults, simultaneousResults);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ImportRunAndCloneDWAQ_AC1()
        {
            string path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DWAQ_AC1.lit\14\NETWORK.TP");

            // import it
            var hydroModelImporter = new SobekHydroModelImporter(true, false);
            var hydroModel = (HydroModel)hydroModelImporter.ImportItem(path);
            var network = hydroModel.Region.AllRegions.OfType<IHydroNetwork>().First();
            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            flow.HydFileOutput = false;

            // enable some feature coverage in flow
            flow.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Laterals).AggregationOptions = AggregationOptions.Current;

            // fill missing evap data
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries) rr.Evaporation.Data, rr.StartTime, rr.StopTime,
                                                         new TimeSpan(1, 0, 0));

            // run it
            ActivityRunner.RunActivity(hydroModel);
            Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);

            // clone it
            var clonedHydro = (HydroModel)hydroModel.DeepClone();

            // assert input data still exists for each model
            var clonedFlow = clonedHydro.Activities.OfType<WaterFlowModel1D>().First();
            var bnd = clonedFlow.BoundaryConditions.First(f => f.Name == "1 - H(t)");
            Assert.AreEqual(0.02, bnd.Data[new DateTime(2010, 1, 2)]);

            var clonedRR = clonedHydro.Activities.OfType<RainfallRunoffModel>().First();
            Assert.AreEqual(UnpavedEnums.GroundWaterSourceType.FromLinkedNode, ((UnpavedData) clonedRR.ModelData[0]).InitialGroundWaterLevelSource);

            // assert using reference helper
            var networkReferences = TestReferenceHelper.SearchObjectInObjectGraph(network, clonedHydro);
            networkReferences.ForEach(Console.WriteLine);
            Assert.AreEqual(0, networkReferences.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UndoRedoDeleteRTCModel()
        {
            var model = new HydroModel() {Activities = {new WaterFlowModel1D(), new RealTimeControlModel()}};

            using (var undoRedoManager = new UndoRedoManager(model))
            {
                var realTimeControlModel = model.Activities.OfType<RealTimeControlModel>().FirstOrDefault();

                model.Activities.Remove(realTimeControlModel);
                undoRedoManager.Undo();
                undoRedoManager.Redo();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UndoRedoDeleteRTCModelShouldPreserveControlledModels()
        {
            var model = new HydroModel() { Activities = { new WaterFlowModel1D(), new RealTimeControlModel() } };

            using (var undoRedoManager = new UndoRedoManager(model))
            {
                var realTimeControlModel = model.Activities.OfType<RealTimeControlModel>().FirstOrDefault();

                model.Activities.Remove(realTimeControlModel);
                undoRedoManager.Undo();

                Assert.AreEqual(1, model.Activities.OfType<RealTimeControlModel>().First().ControlledModels.Count());
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WindowsForms)]
        public void HydroModelTreeViewNodePresenterIsRegistered()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                gui.Plugins.Add(new ScriptingGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());

                
                gui.Run();

                var mainWindow = (Window)gui.MainWindow;

                Action onShown = delegate
                {
                    var treeView = gui.MainWindow.ProjectExplorer.TreeView;
                    ProjectExplorerGuiPlugin.Instance.InitializeProjectTreeView(); // make sure project explorer is shown

                    var hydroModel = new HydroModel();

                    // add model with child models
                    app.Project.RootFolder.Add(hydroModel);

                    treeView.WaitUntilAllEventsAreProcessed();

                    // asserts
                    treeView.Nodes[0].Nodes[0].Presenter
                        .Should("hydro model node presenter is correctly set").Be.OfType<HydroModelTreeViewNodePresenter>();

                };

                WpfTestHelper.ShowModal(mainWindow, onShown);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.VerySlow)]
        public void ImportTholenInGuiShouldBeFast()
        {
            string path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"THOLEN.lit\30\NETWORK.TP");

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                
                gui.Run();
                
                var mainWindow = (Window)gui.MainWindow;

                Action onShown = () => TestHelper.AssertIsFasterThan(145000, 
                    () =>
                    {
                        var importer = new SobekHydroModelImporter(true) { PathSobek = path };

                        // import in the same way as it is done by gui
                        var importActivity = new FileImportActivity(importer)
                                                 {
                                                     ImportedItemOwner = gui.Application.Project.RootFolder,
                                                     Files = new[] { path }
                                                 };

                        importActivity.OnImportFinished += (activity, model, fileImporter) => gui.Application.Project.RootFolder.Add(model);

                        gui.Application.RunActivityInBackground(importActivity);

                        while (gui.Application.IsActivityRunning())
                        {
                            Application.DoEvents();
                        }
                    });

                WpfTestHelper.ShowModal(mainWindow, onShown);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void WorkDirectoriesAreCreatedInsideProjectFolder_HydroModelFlowRtcWaq()
        {
            //
            // open simple integrated model containing flow, rtc and waq
            // d:\delta-shell\test-data\Plugins\DelftModels\Sobek.IntegrationTests\FlowRtcWaq\FlowRtcWaq.dsproj
            // and execute (run) it
            //
            // inside folder project1.dsproj_data we expect the following 3 folders to exist:
            // - <flowmodel_name>_output
            // - <rtcmodel_name>_output
            //
            //  rename the flow model and save -> a new empty folder must be created:
            // - <flowmodel_new_name>_output
            // 
            // save model at different location -> the working directories are created at
            // new location (but are empty)
            //

            var projectService = new HybridProjectRepository(factory);
            var legacyPath = TestHelper.GetTestFilePath(@"FlowRtcWaq\FlowRtcWaq.dsproj");
            var localPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
            var project = projectService.Open(localPath);
            var localDataDirectory = projectService.ProjectDataDirectory;

            var hydroModel = (HydroModel)project.RootFolder.Models.First();
            var rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();
            var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();

            flowModel.HydFileOutput = false;
            var originalDirectory = Environment.CurrentDirectory;
            hydroModel.CurrentWorkflow = hydroModel.Workflows.FirstOrDefault(w => w.ToString().Equals("(RTC + Flow1D)"));
            ActivityRunner.RunActivity(hydroModel);
            
            var flowModelPath = localDataDirectory + Path.DirectorySeparatorChar + flowModel.Name.Replace(' ', '_') + "_output";
            var rtcModelPath = localDataDirectory + Path.DirectorySeparatorChar + rtcModel.Name.Replace(' ', '_') + "_output";
            Assert.IsTrue(Directory.Exists(flowModelPath));
            Assert.IsTrue(Directory.Exists(rtcModelPath));

            flowModel.Name = "banaantje";
            projectService.Save(project);
            flowModelPath = localDataDirectory + Path.DirectorySeparatorChar + flowModel.Name.Replace(' ', '_') + "_output";
            Assert.IsTrue(Directory.Exists(flowModelPath));
            Assert.IsTrue(FileUtils.IsDirectoryEmpty(flowModelPath));
            Assert.IsTrue(Directory.Exists(rtcModelPath));

            const string newLocalPath = "SavedAsProject.dsproj";
            projectService.SaveProjectAs(project, newLocalPath);
            localDataDirectory = projectService.ProjectDataDirectory;

            flowModelPath = localDataDirectory + Path.DirectorySeparatorChar + flowModel.Name.Replace(' ', '_') + "_output";
            rtcModelPath = localDataDirectory + Path.DirectorySeparatorChar + rtcModel.Name.Replace(' ', '_') + "_output";
            Assert.IsTrue(Directory.Exists(flowModelPath));
            Assert.IsTrue(Directory.Exists(rtcModelPath));
            Assert.IsTrue(FileUtils.IsDirectoryEmpty(flowModelPath));
            Assert.IsTrue(FileUtils.IsDirectoryEmpty(rtcModelPath));

            projectService.Close(project);
            FileUtils.DeleteIfExists(localPath);
            FileUtils.DeleteIfExists(newLocalPath);

            Environment.CurrentDirectory = originalDirectory;
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void WorkDirectoriesAreCreatedInsideProjectFolder_HydroModelRR()
        {
            //
            // see the comments at test WorkDirectoriesAreCreatedInsideProjectFolder_HydroModelFlowRtcWaq
            // 
            // This test does the same, but now for an RR-model that is created in code
            // (we have no choice since we don't yet offer backwards compatibility for RR)
            // 

            var projectService = new HybridProjectRepository(factory);
            var path = "rr.dsproj";
            var project = projectService.Create(path);
            var dataPath = projectService.ProjectDataDirectory;

            //var report = new RainfallRunoffModelValidator().Validate(rainfallRunoffModel);
            var hydroModel = new HydroModel()
                {
                    Activities = { new RainfallRunoffModel() }
                };
            var rainfallRunoffModel = hydroModel.Activities.OfType<RainfallRunoffModel>().FirstOrDefault();
            Assert.NotNull(rainfallRunoffModel);
            var catchment = Catchment.CreateDefault();
            rainfallRunoffModel.Basin.Catchments.Add(catchment);
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(rainfallRunoffModel.Precipitation.Data, rainfallRunoffModel.StartTime, rainfallRunoffModel.StopTime,
                                         new TimeSpan(0, 1, 0, 0));
            generator.GenerateTimeSeries(rainfallRunoffModel.Evaporation.Data, rainfallRunoffModel.StartTime, rainfallRunoffModel.StopTime,
                                         new TimeSpan(0, 1, 0, 0));
            
            project.RootFolder.Models = new[] {hydroModel};

            var originalDirectory = Environment.CurrentDirectory;

            projectService.Save(project);
            ActivityRunner.RunActivity(hydroModel);
            var dimrModel = rainfallRunoffModel as IDimrModel;
            Assert.NotNull(dimrModel);
            var rrModelPath = dataPath + Path.DirectorySeparatorChar + hydroModel.Name.Replace(' ', '_') + "_output" + Path.DirectorySeparatorChar + dimrModel.DirectoryName;
            Assert.IsTrue(Directory.Exists(rrModelPath));
            Assert.IsFalse(FileUtils.IsDirectoryEmpty(rrModelPath));

            rainfallRunoffModel.Name = "banaantje";
            projectService.Save(project);
            Assert.IsTrue(Directory.Exists(rrModelPath));
            Assert.IsFalse(FileUtils.IsDirectoryEmpty(rrModelPath));

            const string newLocalPath = "SavedAsRRProject.dsproj";
            projectService.SaveProjectAs(project, newLocalPath);
            dataPath = projectService.ProjectDataDirectory;

            rrModelPath = dataPath + Path.DirectorySeparatorChar + hydroModel.Name.Replace(' ', '_') + "_output" + Path.DirectorySeparatorChar + dimrModel.DirectoryName;
            Assert.IsFalse(Directory.Exists(rrModelPath));

            projectService.Close(project);
            FileUtils.DeleteIfExists(path);

            Environment.CurrentDirectory = originalDirectory;
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ImportSaveLoadRunNoExceptionHydroModelRRTools22551()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());
                gui.Plugins.Add(new RainfallRunoffGuiPlugin());

                gui.Run();

                var mainWindow = (Window) gui.MainWindow;

                Action onShown = delegate
                {
                    string path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"Test_400\400_000.lit\12\NETWORK.TP");

                    var projectService = new HybridProjectRepository(factory);
                    var projectName = @"rr400.dsproj";

                    var project = projectService.CreateNewProjectInTemporaryFolder();
                    project.Name = projectName;

                    var importer = new SobekHydroModelImporter(true, false, false);

                    var compositeModel = (ICompositeActivity) importer.ImportItem(path);
                    var hydroModel = (HydroModel) compositeModel.ParentModel();

                    project.RootFolder.Models = new[] {hydroModel};

                    var originalDirectory = Environment.CurrentDirectory;

                    var tempProjectPath = Path.GetTempPath();
                    var fullProjectPath = Path.Combine(tempProjectPath, projectName);
                    projectService.SaveProjectAs(project, fullProjectPath);
                    projectService.Close(project);

                    project = projectService.Open(fullProjectPath);

                    hydroModel = (HydroModel) project.RootFolder.Models.First();

                    bool exceptionOccured = false;
                    try
                    {
                        ActivityRunner.RunActivity(hydroModel);
                    }
                    catch (Exception e)
                    {
                        exceptionOccured = true;
                    }
                    Assert.False(exceptionOccured, "An exception occured");

                    Environment.CurrentDirectory = originalDirectory;
                };

                WpfTestHelper.ShowModal(mainWindow, onShown);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.VerySlow)]
        public void RunWaterQualityModelParsingDataShouldBeFastTools9130()
        {
            string path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DWAQ_AC1.lit\47\NETWORK.TP");
            var hydroModelImporter = new SobekHydroModelImporter(false, false);
            var hydroModel = (HydroModel)hydroModelImporter.ImportItem(path);
            ModelTestHelper.ReplaceStoreForOutputCoverages(hydroModel);

            TestHelper.AssertIsFasterThan(80000, () => ActivityRunner.RunActivity(hydroModel));
        }

        #region Flow1D_RTC_ModelMerge

        [Test]
        [Category(TestCategory.Integration)]
        public void TestMergingTwoIntegratedModelsRelinksDataItemsAsExpected()
        {
            // Setup Models
            var sourceModel = CreateSimpleFlowRtcHydroModel();
            var destinationModel = CreateSimpleFlowRtcHydroModel();

            // Setup RTC on source model
            var source_wfm1d = sourceModel.Activities.OfType<WaterFlowModel1D>().First();
            var source_rtc = sourceModel.Activities.OfType<RealTimeControlModel>().First();

            var observationPoint = source_wfm1d.Network.BranchFeatures.OfType<IObservationPoint>().First();
            var observationPointDataItems = source_wfm1d.GetChildDataItems(observationPoint);
            var sourceFlow1dModelOutputDataItem = observationPointDataItems.First(opdi => opdi.Role == DataItemRole.Output);

            var weir = source_wfm1d.Network.BranchFeatures.OfType<IWeir>().First();
            var weirDataItems = source_wfm1d.GetChildDataItems(weir);
            var sourceFlow1dModelInputDataItem = weirDataItems.First(opdi => opdi.Role == (DataItemRole.Input | DataItemRole.Output));
            
            var sourceRtcModelControlGroupDataItem = source_rtc.GetDataItemByValue(source_rtc.ControlGroups.First());
            var sourceRtcModelInputDataItem = sourceRtcModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            var sourceRtcModelOutputDataItem = sourceRtcModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);

            // Link DataItems
            sourceRtcModelInputDataItem.LinkTo(sourceFlow1dModelOutputDataItem);
            sourceFlow1dModelInputDataItem.LinkTo(sourceRtcModelOutputDataItem);
            
            // Pre-merge
            var validationreport = destinationModel.ValidateMerge(sourceModel);
            Assert.AreEqual(0, validationreport.AllErrors.Count(), "ModelMerge validation has failed");

            var destination_wfm1d = destinationModel.Activities.OfType<WaterFlowModel1D>().First();
            var destination_rtc = destinationModel.Activities.OfType<RealTimeControlModel>().First();

            Assert.AreEqual(1, destination_wfm1d.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0));
            Assert.AreEqual(0, destination_rtc.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0));

            // Merge
            destinationModel.Merge(sourceModel, null);
            
            // Check Results
            CheckResultsAfterMerge(destination_wfm1d, destination_rtc);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestMergingTwoIntegratedModelsRelinksDataItemsAsExpected_SameBranchFeaturePropertyUsedForRTCInputAndOutput() // Issue#: SOBEK3-561
        {
            // Setup Models
            var sourceModel = CreateSimpleFlowRtcHydroModel();
            var destinationModel = CreateSimpleFlowRtcHydroModel();

            // Setup RTC on source model
            var source_wfm1d = sourceModel.Activities.OfType<WaterFlowModel1D>().First();
            var source_rtc = sourceModel.Activities.OfType<RealTimeControlModel>().First();

            var weir = source_wfm1d.Network.BranchFeatures.OfType<IWeir>().First();
            var weirDataItems = source_wfm1d.GetChildDataItems(weir);
            var sourceFlow1dModelOutputDataItem = weirDataItems.First(opdi => opdi.Role == DataItemRole.Output);
            var sourceFlow1dModelInputDataItem = weirDataItems.First(opdi => opdi.Role == (DataItemRole.Input | DataItemRole.Output));

            var sourceRtcModelControlGroupDataItem = source_rtc.GetDataItemByValue(source_rtc.ControlGroups.First());
            var sourceRtcModelInputDataItem = sourceRtcModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            var sourceRtcModelOutputDataItem = sourceRtcModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);
            
            // Link DataItems - Note: the same BranchFeature is used for both Input and Output
            sourceRtcModelInputDataItem.LinkTo(sourceFlow1dModelOutputDataItem);
            sourceFlow1dModelInputDataItem.LinkTo(sourceRtcModelOutputDataItem);

            // Pre-merge
            var validationreport = destinationModel.ValidateMerge(sourceModel);
            Assert.AreEqual(0, validationreport.AllErrors.Count(), "ModelMerge validation has failed");

            var destination_wfm1d = destinationModel.Activities.OfType<WaterFlowModel1D>().First();
            var destination_rtc = destinationModel.Activities.OfType<RealTimeControlModel>().First();

            Assert.AreEqual(1, destination_wfm1d.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0));
            Assert.AreEqual(0, destination_rtc.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0));

            // Merge
            destinationModel.Merge(sourceModel, null);

            // Check Results
            CheckResultsAfterMerge(destination_wfm1d, destination_rtc);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestMergingTwoIntegratedModelsRelinksDataItemsAsExpected_AfterRenamingOfControlGroupAndFeatures() // Issue#: SOBEK3-597
        {
            // Setup Models
            var sourceModel = CreateSimpleFlowRtcHydroModel();
            var destinationModel = CreateSimpleFlowRtcHydroModel();

            // Setup RTC on source model
            var source_wfm1d = sourceModel.Activities.OfType<WaterFlowModel1D>().First();
            var source_rtc = sourceModel.Activities.OfType<RealTimeControlModel>().First();

            var weir = source_wfm1d.Network.BranchFeatures.OfType<IWeir>().First();
            var weirDataItems = source_wfm1d.GetChildDataItems(weir);
            var sourceFlow1dModelOutputDataItem = weirDataItems.First(opdi => opdi.Role == DataItemRole.Output);
            var sourceFlow1dModelInputDataItem = weirDataItems.First(opdi => opdi.Role == (DataItemRole.Input | DataItemRole.Output));

            var sourceRtcModelControlGroupDataItem = source_rtc.GetDataItemByValue(source_rtc.ControlGroups.First());
            var sourceRtcModelInputDataItem = sourceRtcModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            var sourceRtcModelOutputDataItem = sourceRtcModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);

            // Link DataItems
            sourceRtcModelInputDataItem.LinkTo(sourceFlow1dModelOutputDataItem);
            sourceFlow1dModelInputDataItem.LinkTo(sourceRtcModelOutputDataItem);

            // Rename feature
            weir.Name = "Weir1_Renamed";
            source_rtc.ControlGroups.First().Name = "ControlGroup1_Renamed";
            
            // Pre-merge
            var validationreport = destinationModel.ValidateMerge(sourceModel);
            Assert.AreEqual(0, validationreport.AllErrors.Count(), "ModelMerge validation has failed");

            var destination_wfm1d = destinationModel.Activities.OfType<WaterFlowModel1D>().First();
            var destination_rtc = destinationModel.Activities.OfType<RealTimeControlModel>().First();

            Assert.AreEqual(1, destination_wfm1d.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0));
            Assert.AreEqual(0, destination_rtc.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0));

            // Merge
            destinationModel.Merge(sourceModel, null);

            // Check Results
            CheckResultsAfterMerge(destination_wfm1d, destination_rtc);
        }

        private static HydroModel CreateSimpleFlowRtcHydroModel()
        {
            var flow1dModel = new WaterFlowModel1D();
            var rtcModel = new RealTimeControlModel();
            var integratedModel = new HydroModel();
            integratedModel.Activities.Add(flow1dModel);
            integratedModel.Activities.Add(rtcModel);

            flow1dModel.Network.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); // Amersfoort / RD New

            var hydroNode1 = new HydroNode() { Geometry = new Point(0.0, 0.0) };
            flow1dModel.Network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Geometry = new Point(100.0, 0.0) };
            flow1dModel.Network.Nodes.Add(hydroNode2);
            var branch = new Channel("Branch1", hydroNode1, hydroNode2, 100.0);
            flow1dModel.Network.Branches.Add(branch);

            var observationPoint = new ObservationPoint()
            {
                Name = "ObservationPoint1", 
                Geometry = new Point(15.0, 0.0), 
                Chainage = 15.0
            };
            branch.BranchFeatures.Add(observationPoint);
            
            var weir = new Weir("Weir1")
            {
                Geometry = new Point(65.0, 0.0),
                Chainage = 65.0
            };
            branch.BranchFeatures.Add(weir);

            var controlGroup = new ControlGroup() { Name = "ControlGroup1" };
            rtcModel.ControlGroups.Add(controlGroup);
            controlGroup.Inputs.Add(new Input());
            controlGroup.Outputs.Add(new Output());

            return integratedModel;
        }

        private static void CheckResultsAfterMerge(WaterFlowModel1D destination_wfm1d, RealTimeControlModel destination_rtc)
        {
            Assert.AreEqual(3, destination_wfm1d.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0),
                "Number of additional linked dataitems in desitination WFM1D model is not as expected");
            Assert.AreEqual(2, destination_rtc.AllDataItems.Count(di => di.LinkedTo != null || di.LinkedBy.Count > 0),
                "Number of additional linked dataitems in desitination RTC model is not as expected");

            var destinationModelNetworkDataItem = destination_wfm1d.GetDataItemByValue(destination_wfm1d.Network);
            var destinationFlow1dModelInputDataItem = destinationModelNetworkDataItem.Children.First(di => di.Role == (DataItemRole.Input | DataItemRole.Output));
            var destinationFlow1dModelOutputDataItem = destinationModelNetworkDataItem.Children.First(di => di.Role == DataItemRole.Output);

            var destinationModelControlGroupDataItem = destination_rtc.GetDataItemByValue(destination_rtc.ControlGroups.Last());
            var destinationRtcModelInputDataItem = destinationModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            var destinationRtcModelOutputDataItem = destinationModelControlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);

            Assert.IsTrue(destinationRtcModelInputDataItem.LinkedTo == destinationFlow1dModelOutputDataItem,
                "Dataitem links in desitination WFM1D model are not as expected");
            Assert.IsTrue(destinationRtcModelOutputDataItem.LinkedBy.Contains(destinationFlow1dModelInputDataItem),
                "Dataitems links in desitination RTC model are not as expected");
        }

        #endregion

        #region RTC FLOW (RR) models

        //[Test]
        //[Category(TestCategory.Integration)]
        //[Category(TestCategory.Slow)]
        //public void RtcModelCheckPIDRuleSetpointValue()
        //{
        //    var projectService = new HybridProjectRepository(factory);
        //    // dsproj was created by creating a developer model in the application
        //    // (developer -> add demo mode -> integrated model with flow, RR and RTC),
        //    // and opening and saving that model using version 35228 (to make sure it
        //    // is from a released version)
        //    var legacyPath = TestHelper.GetTestFilePath(@"FlowRtcRR\DeveloperTestModel_SOBEK3.5.5.35228.dsproj");
        //    var localPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
        //    var project = projectService.Open(localPath);

        //    var hydroModel = project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
        //    Assert.NotNull(hydroModel, "Something is wrong in the dsproj - missing hydromodel.");
        //    var rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
        //    Assert.NotNull(rtcModel, "Something is wrong in the dsproj - missing realtimecontrolmodel.");

        //    var ctg = rtcModel.ControlGroups.FirstOrDefault();
        //    Assert.NotNull(ctg, "error in model - control group not found");
        //    var pid = ctg.Rules.OfType<PIDRule>().FirstOrDefault();
        //    Assert.NotNull(pid, "error in model - pid rule not found");

        //    pid.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
        //    const double constantValue = 0.444d;
        //    pid.ConstantValue = constantValue;

        //    ActivityRunner.RunActivity(hydroModel);
        //    Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);

        //    // assert timeseries file exists
        //    // assert timeseries file does not exist
        //    // assert constant value is present in RtcToolsConfig.xml

        //    var localDataDirectory = projectService.ProjectDataDirectory;
        //    var timeSeriesFile = Path.Combine(localDataDirectory, rtcModel.Name.Replace(' ', '_') + "_output", "timeseries_import.xml");
        //    Assert.IsFalse(File.Exists(timeSeriesFile));
        //    var rtcToolsConfigFile = Path.Combine(localDataDirectory, rtcModel.Name.Replace(' ', '_') + "_output", "rtcToolsConfig.xml");
        //    Assert.IsTrue(File.Exists(rtcToolsConfigFile));
        //    var xmldoc = new XmlDocument();
        //    xmldoc.Load(rtcToolsConfigFile);
        //    var elements = xmldoc.GetElementsByTagName("setpointValue");
        //    Assert.AreEqual(1, elements.Count);
        //    Assert.AreEqual(constantValue.ToString(CultureInfo.InvariantCulture), elements[0].InnerXml);

        //    projectService.Close(project);
        //    FileUtils.DeleteIfExists(localPath);
        //}

        //[Test]
        //[Category(TestCategory.Integration)]
        //[Category(TestCategory.Slow)]
        //public void RtcModelCheckPIDRuleSetpointSeries()
        //{
        //    var projectService = new HybridProjectRepository(factory);
        //    // dsproj was created by creating a developer model in the application
        //    // (developer -> add demo mode -> integrated model with flow, RR and RTC),
        //    // and opening and saving that model using version 35228 (to make sure it
        //    // is from a released version)
        //    var legacyPath = TestHelper.GetTestFilePath(@"FlowRtcRR\DeveloperTestModel_SOBEK3.5.5.35228.dsproj");
        //    var localPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
        //    var project = projectService.Open(localPath);

        //    var hydroModel = project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
        //    Assert.NotNull(hydroModel, "Something is wrong in the dsproj - missing hydromodel.");
        //    var rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
        //    Assert.NotNull(rtcModel, "Something is wrong in the dsproj - missing realtimecontrolmodel.");

        //    var ctg = rtcModel.ControlGroups.FirstOrDefault();
        //    Assert.NotNull(ctg, "error in model - control group not found");
        //    var pid = ctg.Rules.OfType<PIDRule>().FirstOrDefault();
        //    Assert.NotNull(pid, "error in model - pid rule not found");

        //    pid.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;

        //    var timeSeries = new TimeSeries();
        //    var t = new DateTime(2010, 1, 1);
        //    timeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
        //    timeSeries.Components.Add(new Variable<double>("value", new Unit("-", "-")));
        //    timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
        //    timeSeries[t] = 0.444d;
        //    timeSeries[t.AddDays(6)] = 0.444d;
        //    timeSeries.Name = "someTime";

        //    pid.TimeSeries = timeSeries;

        //    ActivityRunner.RunActivity(hydroModel);
        //    Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);
            
        //    // assert timeseries file exists

        //    var localDataDirectory = projectService.ProjectDataDirectory;
        //    var timeSeriesFile = Path.Combine(localDataDirectory, rtcModel.Name.Replace(' ', '_') + "_output", "timeseries_import.xml");
        //    Assert.IsTrue(File.Exists(timeSeriesFile));

        //    projectService.Close(project);
        //    FileUtils.DeleteIfExists(localPath);
        //}

        #endregion

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void CanRunFlowRtcRRModel() // https://issuetracker.deltares.nl/browse/SOBEK3-611
        {
            var projectService = new HybridProjectRepository(factory);
            // dsproj was created by opening and saving the project from the issue 
            // using version 35228 (to make sure it is from a released version)
            var legacyPath = TestHelper.GetTestFilePath(@"FlowRtcRR\test-rr-flow-rtc_SOBEK3.5.5.35228.dsproj");
            var localPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
            var project = projectService.Open(localPath);

            var hydroModel = project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
            Assert.NotNull(hydroModel, "Something is wrong in the dsproj - missing hydromodel.");

            try
            {
                ActivityRunner.RunActivity(hydroModel);
                ActivityRunner.RunActivity(hydroModel); // run it twice in a row
                Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);
            }
            catch (Exception e)
            {
                Assert.Fail("Exception when running model: " + e);
            }
            finally
            {
                projectService.Close(project);
                FileUtils.DeleteIfExists(localPath);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Given1D2DHydroModelWhenUseMorSedValueSetToTrueThenMapFormatEqualToNetCdfMapFormatType()
        {
            var hydroModel = GetHydroModelWithFmSubModel(true);

            var fmSubModel = (WaterFlowFMModel)hydroModel.Activities.FirstOrDefault();
            if (fmSubModel == null) throw new Exception("Integrated model was not of type WaterFlowFMModel");

            Assert.That(fmSubModel.ModelDefinition.UseMorphologySediment, Is.EqualTo(false));
            Assert.That(fmSubModel.ModelDefinition.MapFormat, Is.EqualTo(MapFormatType.NetCdf));

            fmSubModel.ModelDefinition.UseMorphologySediment = true;
            Assert.That(fmSubModel.ModelDefinition.UseMorphologySediment, Is.EqualTo(true));
            Assert.That(fmSubModel.ModelDefinition.MapFormat, Is.EqualTo(MapFormatType.NetCdf));
        }

        [Test]
        public void GivenNon1D2DHydroModelWhenUseMorSedValueSetToTrueThenMapFormatEqualToUGridMapFormatType()
        {
            var hydroModel = GetHydroModelWithFmSubModel(false);

            var fmSubModel = (WaterFlowFMModel)hydroModel.Activities.FirstOrDefault();
            if (fmSubModel == null) throw new Exception("Integrated model was not of type WaterFlowFMModel");

            Assert.That(fmSubModel.ModelDefinition.UseMorphologySediment, Is.EqualTo(false));
            Assert.That(fmSubModel.ModelDefinition.MapFormat, Is.EqualTo(MapFormatType.NetCdf));

            fmSubModel.ModelDefinition.UseMorphologySediment = true;
            Assert.That(fmSubModel.ModelDefinition.UseMorphologySediment, Is.EqualTo(true));
            Assert.That(fmSubModel.ModelDefinition.MapFormat, Is.EqualTo(MapFormatType.Ugrid));
        }

        private static HydroModel GetHydroModelWithFmSubModel(bool isPartOf1D2DModel)
        {
            var fmModel = new WaterFlowFMModel
            {
                ModelDefinition =
                {
                    MapFormat = MapFormatType.NetCdf
                }
            };
            var isPartOf1D2DModelGuiProperty = fmModel.ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel);
            isPartOf1D2DModelGuiProperty.Value = isPartOf1D2DModel;

            var hydroModel = new HydroModel()
            {
                Activities = {fmModel}
            };
            return hydroModel;
        }
    }
}