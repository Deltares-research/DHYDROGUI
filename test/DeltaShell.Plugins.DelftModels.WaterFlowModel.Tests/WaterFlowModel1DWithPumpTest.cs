using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DWithPumpTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void ExecuteSimplerWithPump()
        {
            // create simplest network
            INode inflowNode;
            INode outflowNode;
            var hydroNetwork = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out inflowNode, out outflowNode);
            
            // Add a Pump
            var pump = new Pump {Capacity = 2.0, StartDelivery = 0.0, StopDelivery = 0.0, StartSuction = 0.001, StopSuction = 0.0};
            var channel = hydroNetwork.Channels.First();

            var compositeBranchStructure = new CompositeBranchStructure
                                               {
                                                   Network = hydroNetwork, 
                                                   Geometry = new Point(5, 0), 
                                                   Chainage = 5, 
                                                   //Structures = { pump }
                                               };
            //compositeBranchStructure.Structures.Add(pump);
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, channel, compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            // add discretization
            var networkDiscretization = new Discretization
                                            {
                                                Network = hydroNetwork,
                                                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                            };
            foreach (var ch in hydroNetwork.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, ch, 0, true, 0.5, true, false, false, -1);
            }

            var flowModel1D = WaterFlowModel1DTestHelper.SetupModelForSimplerNetwork(networkDiscretization, hydroNetwork, inflowNode, outflowNode);
            WaterFlowModel1DTestHelper.RunInitializedModel(flowModel1D);

            // TODO: migrate to FitNesse, compare only results at a few locations (with tolerance)
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)] // fails occasionally! find out what is the problem before removing it from work in progress
        public void ExecuteSimplerWithPumpThatHasReductionTable()
        {
            double errorMargin = 1.0;
            // create simplest network (branch with length 100)
            INode inflowNode;
            INode outflowNode;
            var hydroNetwork = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out inflowNode, out outflowNode);

           var pump = new Pump
                           {
                               Capacity = 100.0, 
                               StartDelivery = 0.0, 
                               StopDelivery = 0.0, 
                               StartSuction = 0.001, 
                               StopSuction = 0.0,
                               DirectionIsPositive = false
                           };

            var firstChannel = hydroNetwork.Channels.First();

            // Create and add compositeStructure with our pump
            var compositeBranchStructure = new CompositeBranchStructure
            {
                Network = hydroNetwork,
                Geometry = new Point(5, 0),
                Chainage = 5
            };

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, firstChannel, compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = hydroNetwork,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            foreach (var channel in hydroNetwork.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, true, 0.5, true, false, false, -1);
            }

            var flowModel1D = WaterFlowModel1DTestHelper.SetupModelForSimplerNetwork(networkDiscretization, hydroNetwork, inflowNode, outflowNode);
            var flowResults = GetFlowResultsForFirstFourTimeSteps(flowModel1D, 2).ToArray();

            // Check results without reduction table
            // Joost knows where these values come from and whether these are valid
            Assert.AreEqual(0.1, flowResults[0], errorMargin);
            Assert.AreEqual(269.48135947970559, flowResults[1], errorMargin);
            Assert.AreEqual(1056.8557105830271, flowResults[2], errorMargin);
            Assert.AreEqual(60.1567733295065, flowResults[3], errorMargin);
            
            pump.ReductionTable[2.0] = 0.1;

            flowResults = GetFlowResultsForFirstFourTimeSteps(flowModel1D, 2).ToArray();

            // Check results with reduction table
            Assert.AreEqual(0.1, flowResults[0], 0.01);
            Assert.AreEqual(369.72534571720126, flowResults[1], errorMargin);
            Assert.AreEqual(1151.116201281601, flowResults[2], errorMargin);
            Assert.AreEqual(146.48196577163998, flowResults[3], errorMargin);

            pump.ReductionTable.Clear();

            flowResults = GetFlowResultsForFirstFourTimeSteps(flowModel1D, 2).ToArray();

            // Check results without reduction table
            Assert.AreEqual(0.1, flowResults[0], errorMargin);

            Assert.AreEqual(269.48135947970559, flowResults[1], errorMargin);
            Assert.AreEqual(1056.8557105830271, flowResults[2], errorMargin);
            Assert.AreEqual(60.1567733295065, flowResults[3], errorMargin);
        }

        private static IEnumerable<double> GetFlowResultsForFirstFourTimeSteps(WaterFlowModel1D flowModel1D, int networkLocationIndex)
        {
            if (flowModel1D.Status != ActivityStatus.Initialized)
            {
                flowModel1D.Initialize();
            }

            WaterFlowModel1DTestHelper.RunInitializedModel(flowModel1D);

            var flowCoverage = flowModel1D.OutputFlow;
            var timeSteps = flowCoverage.Time.Values.Take(4).ToArray();
            var networkLocation = flowCoverage.Arguments[1].Values[networkLocationIndex];

            return timeSteps.Select(timeStep => (double) flowCoverage[timeStep, networkLocation]);
        }
    }
}