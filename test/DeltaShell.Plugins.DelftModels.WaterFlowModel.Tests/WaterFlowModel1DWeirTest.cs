using System;
using System.Collections.Generic;
using System.IO;
using DelftShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DelftTools.DataObjects.Functions.Filters;
using DelftTools.DataObjects.Helpers;
using DelftTools.DataObjects.HydroNetwork;
using DelftTools.DataObjects.HydroNetwork.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Workflow;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using GeometryFactory=SharpMap.Converters.Geometries.GeometryFactory;

namespace DelftShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DWeirTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaterFlowModel1DWeirTest));

        [Test]
        [Category("Integration")]
        public void ExecuteSimplerWithWeir()
        {
            // create simplest network
            Boundary boundaryInflow;
            Boundary boundaryOutflow;
            HydroNetwork hydroNetwork = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out boundaryInflow, out boundaryOutflow);
            // Add Weir
            var weir = new Weir { Geometry = new Point(5, 0), OffsetY = 150, CrestWidth = 50, CrestLevel = 8 };
            var channel = ((IChannel)hydroNetwork.Branches[0]);
            channel.BranchFeatures.Add(weir);

            var compositeBranchStructure = new CompositeBranchStructure { Network = hydroNetwork, Geometry = new Point(5, 0), Offset = 5 };
            compositeBranchStructure.Structures.Add(weir);

            channel.BranchFeatures.Add(compositeBranchStructure);
            // add discretization
            var networkDiscretization = new Discretization
                                            {
                                                Network = hydroNetwork,
                                                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                            };
            foreach (var ch in hydroNetwork.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, ch, 0, true, 0.5, true, false, -1);
            }

            WaterFlowModel1D flowModel1D = WaterFlowModel1DTestHelper.SetupModelForSimplerNetwork(networkDiscretization, hydroNetwork, boundaryInflow, boundaryOutflow,false);
            WaterFlowModel1DTestHelper.RunSimplerModel(flowModel1D);
            
            if (WaterFlowModel1DTestHelper.WriteTestData)
            {
                TestHelper.WriteXml(@".\..\..\Xml\ExecuteSimplerWithWeir.xml", flowModel1D.ToXml());
            }

            // TODO: migrate test to FitNesse
            //string expected = File.ReadAllText(@".\..\..\Xml\ExecuteSimplerWithWeir.xml");
            //TestHelper.AssertXmlEquals(expected, flowModel1D.ToXml());
            //CompareModelResultsWithStoredResults(flowModel1D, "ExecuteSimplerWithWeir");
        }

        [Test]
        [Category("Integration")]
        public void ExecuteWithWeir()
        {
            //Assert.Fail("Crashes built server?");
            // create simplest network
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2, 100.0);
            var vertices = new List<ICoordinate>
                               {
                                   GeometryFactory.CreateCoordinate(0, 0),
                                   GeometryFactory.CreateCoordinate(100, 0)
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);

            // add boundaries
            var boundaryInflow = new Boundary();
            node1.Boundary = boundaryInflow;

            var boundaryOutflow = new Boundary();
            node2.Boundary = boundaryOutflow;

            // add cross-sections
            var crossSection1 = new CrossSection("crs1", 40.0);

            crossSection1.Definition.DefinitionData[0.0] = new[] { 0.0, 1, 0.001f, 0.001f };
            crossSection1.Definition.DefinitionData[100.0] = new[] { 0.0, 1, 0.001f, 0.001f };
            crossSection1.Definition.DefinitionData[150.0] = new[] { -10.0, 1, 0.001f, 0.001f };
            crossSection1.Definition.DefinitionData[300.0] = new[] { -10.0, 1, 0.001f, 0.001f };
            crossSection1.Definition.DefinitionData[350.0] = new[] { 0.0, 1, 0.001f, 0.001f };
            crossSection1.Definition.DefinitionData[500.0] = new[] { 0.0, 1, 0.001f, 0.001f };

            branch1.BranchFeatures.Add(crossSection1);

            var crossSection2 = new CrossSection("crs2", 60.0);

            crossSection2.Definition.DefinitionData[0.0] = new[] { 0.0, 1, 0.001f, 0.001f };
            crossSection2.Definition.DefinitionData[100.0] = new[] { 0.0, 1, 0.001f, 0.001f };
            crossSection2.Definition.DefinitionData[150.0] = new[] { -10.0, 1, 0.001f, 0.001f };
            crossSection2.Definition.DefinitionData[300.0] = new[] { -10.0, 1, 0.001f, 0.001f };
            crossSection2.Definition.DefinitionData[350.0] = new[] { 0.0, 1, 0.001f, 0.001f };
            crossSection2.Definition.DefinitionData[500.0] = new[] { 0.0, 1, 0.001f, 0.001f };

            branch1.BranchFeatures.Add(crossSection2);

            // add weirs
            var weir = new Weir { OffsetY = 150, CrestWidth = 75, CrestLevel = -3 };
            var compositeStructure = new CompositeBranchStructure { Offset = 50, Structures = { weir } };
            branch1.BranchFeatures.Add(compositeStructure);
            branch1.BranchFeatures.Add(weir); // HACK: <<< bug

            // add discretization
            Discretization networkDiscretization = new Discretization
                                                       {
                                                           Network = network,
                                                           SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                                       };
            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, branch1, 0, true, 5.0, true, false, branch1.Length / 10.0);

            // setup 1d flow model
            var flowModel1D = new WaterFlowModel1D();
            flowModel1D.NetworkDiscretization = networkDiscretization;
            WaterFlowModel1D.TemplateDataZipFile =  WaterFlowModel1DTestHelper.TemplateDir;

            var t = DateTime.Now;

            flowModel1D.StartTime = t;
            flowModel1D.StopTime = t.AddMinutes(5);
            flowModel1D.TimeStep = new TimeSpan(0, 0, 1);
            flowModel1D.OutputTimeStep = new TimeSpan(0, 0, 1);

            // set network
            flowModel1D.Network = network;

            // set initial conditions
            flowModel1D.InitialFlow.SetValues(new double[] { 0.1 });
            flowModel1D.InitialDepth.SetValues(new double[] { 0.1 });

            // set boundary conditions
            var boundaryConditionInflow = new WaterFlowModel1DBoundaryCondition
                                              {
                                                  Boundary = boundaryInflow,
                                                  Type = WaterFlowModel1DBoundaryConditionType.FlowTimeSeries
                                              };
            boundaryConditionInflow[t] = 1;
            boundaryConditionInflow[t.AddSeconds(30)] = 1.0;
            boundaryConditionInflow[t.AddSeconds(60)] = 1.5;
            boundaryConditionInflow[t.AddSeconds(120)] = 1.0;
            boundaryConditionInflow[t.AddSeconds(180)] = 0.5;

            var boundaryConditionOutflow = new WaterFlowModel1DBoundaryCondition
                                               {
                                                   Boundary = boundaryOutflow,
                                                   Type = WaterFlowModel1DBoundaryConditionType.DepthTimeSeries
                                               };
            boundaryConditionOutflow[t] = 0.1;
            boundaryConditionOutflow[t.AddSeconds(30)] = 0.1;
            boundaryConditionOutflow[t.AddSeconds(60)] = 0.2;
            boundaryConditionOutflow[t.AddSeconds(120)] = 0.3;
            boundaryConditionOutflow[t.AddSeconds(180)] = 0.1;

            flowModel1D.ClearBoundaryConditions();
            flowModel1D.AddBoundaryCondition(boundaryConditionInflow);
            flowModel1D.AddBoundaryCondition(boundaryConditionOutflow);

            flowModel1D.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, flowModel1D.Status, "Model should be in initialized state after it is created.");

            t = DateTime.Now;

            var timeStepCount = 0;
            while (flowModel1D.Status != ActivityStatus.Finished)
            {
                flowModel1D.Execute();
                log.Warn(string.Format("timestep: {0}", timeStepCount));
                // get values from model for the last time step
                IList<double> values = flowModel1D.OutputDepth.GetValues<double>(
                    new VariableValueFilter(flowModel1D.OutputDepth.Arguments[0], flowModel1D.CurrentTime)
                    );

                //log.Debug(new List<double>(values).ToArray());

                if (flowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }
                timeStepCount++;
            }
            // expected number of timesteps is 10
            Assert.AreEqual(300, timeStepCount);

            log.DebugFormat("It took {0} sec to run model", (DateTime.Now - t).TotalSeconds);

            Assert.IsTrue(flowModel1D.CurrentTime >= flowModel1D.StopTime);
        }
    }
}