using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DWithBridgeTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaterFlowModel1DWithBridgeTest));
        
        [SetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }


        [TearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        /// <summary>
        /// Runs a simple model with a bridge. Previously this test crashed the model engine; thus do no remove it.
        /// todo: Add extra result checks.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteSimplerWithBridge()
        {
            // be sure SetBridge is not commented out in WaterFlowModel1D.SetStructure!!!

            // create simplest network
            RunModelWithRoughness(0.2);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecutePillarBridge()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(model);

            ActivityRunner.RunActivity(model);

            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

            var withoutBridge = model.OutputWaterLevel;
            var withoutBridgeValues = ((IEnumerable<double>) withoutBridge.Components[0].Values).ToList();

            var bridge = new Bridge("hehe")
                             {
                                 FlowDirection = FlowDirection.Both,
                                 BridgeType = BridgeType.Pillar,
                                 PillarWidth = 2.0,
                                 ShapeFactor = 0.2
                             };

            SetBridgeToFirstChannel(bridge, model.Network);
            
            ActivityRunner.RunActivity(model);

            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

            var withBridge = model.OutputWaterLevel;

            var withBridgeValues = ((IEnumerable<double>) withBridge.Components[0].Values).ToList();
            Assert.AreNotEqual(withoutBridgeValues, withBridgeValues);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestIfRoughnessHasEffect()
        {
            WaterFlowModel1D model02 = RunModelWithRoughness(0.2);
            IVariableFilter lastTimeStepFilter02 = new VariableValueFilter<DateTime>(model02.OutputVelocity.Time,
                                                                                   model02.OutputVelocity.Time.Values.Last());

            IMultiDimensionalArray valuesFriction02 = model02.OutputVelocity.GetValues(lastTimeStepFilter02);

            WaterFlowModel1D model03 = RunModelWithRoughness(0.3);
            IVariableFilter lastTimeStepFilter03 = new VariableValueFilter<DateTime>(model03.OutputVelocity.Time,
                                                                                   model03.OutputVelocity.Time.Values.Last());

            IMultiDimensionalArray valuesFriction03 = model03.OutputVelocity.GetValues(lastTimeStepFilter03);
            int count = 0;
            for (int i = 0; i < valuesFriction03.Count; i++)
            {
                if (Math.Abs((double)valuesFriction02[i] - (double)valuesFriction03[i]) > 1.0e-6)
                {
                    count++;
                }
            }
            Assert.Greater(count, 0, "Some values should differ.");
        }

        private static WaterFlowModel1D RunModelWithRoughness(double roughness)
        {
            INode inflowNode;
            INode outflowNode;
            var hydroNetwork = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out inflowNode, out outflowNode);
            
            // Add a bridge
            var bridge = new Bridge
                             {
                                 FlowDirection = FlowDirection.Both,
                                 Friction = roughness,
                                 GroundLayerEnabled = false,
                                 FrictionType = BridgeFrictionType.Manning
                             };
            bridge.SetRectangleCrossSection(0.0, 10.0, 5.0);
            SetBridgeToFirstChannel(bridge, hydroNetwork);

            // add discretization
            Discretization networkDiscretization = GetNetworkDiscretization(hydroNetwork);

            var flowModel1D = WaterFlowModel1DTestHelper.SetupModelForSimplerNetwork(networkDiscretization, hydroNetwork, inflowNode, outflowNode);
            WaterFlowModel1DTestHelper.RunInitializedModel(flowModel1D);
            return flowModel1D;
        }

        private static void SetBridgeToFirstChannel(Bridge bridge, IHydroNetwork hydroNetwork)
        {
            var channel = hydroNetwork.Channels.First();

            var compositeBranchStructure = new CompositeBranchStructure 
                                               {
                                                   Network = hydroNetwork, 
                                                   Geometry = new Point(55, 0), 
                                                   Chainage = 55
                                               };
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, channel, compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, bridge);
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