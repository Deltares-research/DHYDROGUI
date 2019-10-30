using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DWithLateralSourcesTest
    {
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            //WaterFlowModel1D.ServerExecutablePath = @"DelftModelServer.exe";
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunInProcessWithLateralSourceTwice()
        {
            //define a model and network
            //add a source to the network
            //add LSD for this source
            INode inflowNode;
            INode outflowNode;
            HydroNetwork hydroNetwork = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out inflowNode, out outflowNode);

            // Add lateral source to network.
            var source = new LateralSource {Chainage = 1};
            //hydroNetwork.Branches[0].BranchFeatures.Add(source);
            NetworkHelper.AddBranchFeatureToBranch(source, hydroNetwork.Branches[0], source.Chainage);
            
            // get discretization
            Discretization networkDiscretization = GetNetworkDiscretization(hydroNetwork);

            // setup 1d flow model
            var flowModel1D = new WaterFlowModel1D();

            //use a fixed date for comparison with stored results
            var t = new DateTime(2000, 1, 1);

            SetTimeParameters(flowModel1D, t);

            // set network
            flowModel1D.Network = hydroNetwork;

            // set initial conditions
            flowModel1D.InitialFlow.SetValues(new[] {0.1});
            flowModel1D.InitialConditions.SetValues(new[] { 0.1 });                    

            // set boundary conditions
            SetBoundaryConditions(flowModel1D, inflowNode, outflowNode);
            
            flowModel1D.NetworkDiscretization = networkDiscretization;

            // Bind f(t) to source on model
            var timeserie = new TimeSeries();
            timeserie.Components.Add(new Variable<double>());
            timeserie[t] = 0;
            timeserie[t.AddMinutes(5)] = 20;
            flowModel1D.LateralSourceData.First(lsd => lsd.Feature == source).Data = timeserie;

            flowModel1D.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, flowModel1D.Status,
                            "Model should be in initialized state after it is created.");
            
            
            WaterFlowModel1DTestHelper.RunInitializedModel(flowModel1D);
            
            //run it again
            flowModel1D.Initialize();
            WaterFlowModel1DTestHelper.RunInitializedModel(flowModel1D);
        }

        private static void SetBoundaryConditions(WaterFlowModel1D flowModel1D1, INode inflowNode, INode outflowNode)
        {
            var boundaryConditionInflow = flowModel1D1.BoundaryConditions.First(bc => bc.Feature == inflowNode);
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;

            var boundaryConditionOutflow = flowModel1D1.BoundaryConditions.First(bc => bc.Feature == outflowNode);
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;            
        }

        private static void SetTimeParameters(WaterFlowModel1D flowModel1D1, DateTime t)
        {
            flowModel1D1.StartTime = t;
            flowModel1D1.StopTime = t.AddMinutes(5);
            flowModel1D1.TimeStep = new TimeSpan(0, 0, 30);
            flowModel1D1.OutputTimeStep = new TimeSpan(0, 0, 30);
        }

        private static Discretization GetNetworkDiscretization(HydroNetwork hydroNetwork)
        {
            var networkDiscretization = new Discretization
                                            {
                                                Network = hydroNetwork,
                                                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                            };
            foreach (var ch in hydroNetwork.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, ch, 0, true, 0.5, true, false, false, -1);
            }
            return networkDiscretization;
        }
    }
}
