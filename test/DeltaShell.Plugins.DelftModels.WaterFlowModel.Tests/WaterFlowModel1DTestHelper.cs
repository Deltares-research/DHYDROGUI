using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    public static class WaterFlowModel1DTestHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (WaterFlowModel1DTestHelper));

        public static HydroNetwork CreateSimplerNetwork(out INode inflowNode, out INode outflowNode)
        {
            // HydroNetwork has default 1 CrossSectionType
            var network = new HydroNetwork();

            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(100, 0);

            // add nodes and branches
            var node1 = new HydroNode {Name = "node1", Network = network, Geometry = new Point(startCoordinate)};
            var node2 = new HydroNode {Name = "node2", Network = network, Geometry = new Point(endCoordinate)};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var channel = new Channel("branch1", node1, node2, 100.0);
            var vertices = new List<Coordinate>
                               {
                                   startCoordinate,
                                   endCoordinate
                               };
            channel.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());


            network.Branches.Add(channel);

            // add boundary sources

            inflowNode = node1;

            outflowNode = node2;

            // add cross-sections
            AddDefaultCrossSection(channel, "crs1", 40);
            return network;
        }

        public static void AddDefaultCrossSection(IChannel channel, string name, double offset)
        {
            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(100.0, 0.0),
                                        new Coordinate(150.0, -10.0),
                                        new Coordinate(300.0, -10.0),
                                        new Coordinate(350.0, 0.0),
                                        new Coordinate(500.0, 0.0)
                                    };
            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(channel, offset, yzCoordinates, name);
        }

        public static void RunInitializedModel(WaterFlowModel1D flowModel1D)
        {
            if (flowModel1D.Status != ActivityStatus.Initialized)
            {
                throw new InvalidOperationException("Cannot run model that is not initialized");
            }
            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(flowModel1D);

            DateTime t = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            var stepTimes = new List<DateTime>();
            int timeStepCount = 0;
            while (flowModel1D.Status != ActivityStatus.Done)
            {
                stepTimes.Add(flowModel1D.CurrentTime);
                flowModel1D.Execute();
                
                
                if (flowModel1D.Status == ActivityStatus.Failed)
                {
                    flowModel1D.Cleanup();
                    Assert.Fail("Model run has failed: " + flowModel1D.LastRunLog);
                }
                timeStepCount++;
            }

            flowModel1D.Finish();

            flowModel1D.Cleanup();
            foreach (var stepTime in stepTimes.Skip(1))
            {
                // get values from model for the time steps
                IList<double> values = flowModel1D.OutputDepth.GetValues<double>(
                    new VariableValueFilter<DateTime>(flowModel1D.OutputDepth.Arguments[0], stepTime));
                Assert.Greater(values.Count, 0);
                log.Debug(new List<double>(values).ToArray());
            }

            // expected number of timesteps is at least 10
            Assert.IsTrue(10 <= timeStepCount);
            log.DebugFormat("It took {0} sec to run model", (DateTime.Now - t).TotalSeconds);
            Assert.IsTrue(flowModel1D.CurrentTime >= flowModel1D.StopTime);
        }


        public static WaterFlowModel1D SetupModelForSimplerNetwork(IDiscretization networkDiscretization,
                                                                   HydroNetwork hydroNetwork, INode inflowNode,
                                                                   INode outflowNode)
        {
            // setup 1d flow model
            var flowModel1D = new WaterFlowModel1D();

            //use a fixed date for comparison with stored results
            var t = new DateTime(2000, 1, 1);

            flowModel1D.StartTime = t;
            flowModel1D.StopTime = t.AddMinutes(5);
            flowModel1D.TimeStep = new TimeSpan(0, 0, 30);
            flowModel1D.OutputTimeStep = new TimeSpan(0, 0, 30);

            // set network
            flowModel1D.Network = hydroNetwork;

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 0.1;
            flowModel1D.InitialConditions.DefaultValue = 0.1;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == inflowNode);
            boundaryConditionInflow.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            boundaryConditionInflow.Data[t] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(30)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(60)] = 1.5;
            boundaryConditionInflow.Data[t.AddSeconds(120)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(180)] = 0.5;
            boundaryConditionInflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == outflowNode);
            boundaryConditionOutflow.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            boundaryConditionOutflow.Data[t] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(30)] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(60)] = 0.2;
            boundaryConditionOutflow.Data[t.AddSeconds(120)] = 0.3;
            boundaryConditionOutflow.Data[t.AddSeconds(180)] = 0.1;
            boundaryConditionOutflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            flowModel1D.NetworkDiscretization = networkDiscretization;
            flowModel1D.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
            flowModel1D.OutputSettings.BranchVelocity = AggregationOptions.Current;

            flowModel1D.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, flowModel1D.Status,
                            "Model should be in initialized state after it is created.");

            return flowModel1D;
        }


        public static void AddFlowDepthBoundary(INode node, WaterFlowModel1D flowModel1D, DateTime t)
        {
            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node);

            boundaryConditionOutflow.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            boundaryConditionOutflow.Data[t] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(30)] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(60)] = 0.2;
            boundaryConditionOutflow.Data[t.AddSeconds(120)] = 0.3;
            boundaryConditionOutflow.Data[t.AddSeconds(180)] = 0.1;
            boundaryConditionOutflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
        }

        public static void AddFlowTimeBoundaryCondition(INode node, WaterFlowModel1D flowModel1D, DateTime t)
        {
            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node);
            
            boundaryConditionInflow.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            boundaryConditionInflow.Data[t] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(30)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(60)] = 1.5;
            boundaryConditionInflow.Data[t.AddSeconds(120)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(180)] = 0.5;

            //add default extrapolation for test.
            boundaryConditionInflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
        }

        /// <summary>
        /// Creates a default discretization for the network
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        public static Discretization GetNetworkDiscretization(IHydroNetwork network)
        {
            var networkDiscretization = new Discretization
                                            {
                                                Network = network,
                                                SegmentGenerationMethod =
                                                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                            };

            foreach (IChannel branch in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, branch, 0, false, 0.5, false, false, true,
                                                          branch.Length / 10.0);
            }
            return networkDiscretization;
        }
    }
}