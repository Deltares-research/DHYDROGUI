using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
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
    public class WaterFlowModel1DWithCulvertTest
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

        /// <summary>
        /// Runs a simple model with a culvert. 
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)] // fails occasionally! find out what is the problem before removing it from work in progress
        public void ExecuteModelWithCulvert()
        {
            //BE SURE ICULVERT is not skipped in WaterFlowModel1D.SetStructures!!!


            // Add a culvert
            ICulvert culvert = new Culvert
                                   {
                                       FlowDirection = FlowDirection.Both,
                                       FrictionType = CulvertFrictionType.Manning,
                                       Friction = 0.2,
                                       Width = 30, 
                                       Height = 10,
                                       CulvertType = CulvertType.Culvert,
                                       SiphonOnLevel = 0.2,
                                       SiphonOffLevel = 0.1,
                                       IsGated = true,
                                       GateInitialOpening = 4.4,
                                       Length = 10,
                                       InletLossCoefficient = 0.1,
                                       InletLevel = 0.3,
                                       OutletLossCoefficient = 0.2,
                                       OutletLevel = 0.4,
                                       BendLossCoefficient = 0.0,
                             };
            //set a geometry
            culvert.GeometryType = CulvertGeometryType.Rectangle;
            culvert.Width = 4;
            culvert.Height = 5;
            culvert.GateOpeningLossCoefficientFunction.Arguments[0].SetValues(new[] { 0.0d, 1.0d });
            culvert.GateOpeningLossCoefficientFunction.SetValues(new[] { 0.0d, 1.0d });

            // create simplest network
            AddCulvertToModelAndRunModel(culvert);
        }

        /// <summary>
        /// Runs a simple model with a culvert. 
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteModelWithSiphon()
        {
            // Add a culvert
            ICulvert culvert = new Culvert
            {
                FlowDirection = FlowDirection.Both,
                FrictionType = CulvertFrictionType.Manning,
                Friction = 0.2,
                Width = 30,
                Height = 10,
                CulvertType = CulvertType.Siphon,
                SiphonOnLevel = 0.2,
                SiphonOffLevel = 0.1,
                IsGated = true,
                GateInitialOpening = 4.4,
                Length = 10,
                InletLossCoefficient = 0.1,
                InletLevel = 0.3,
                OutletLossCoefficient = 0.2,
                OutletLevel = 0.4,
                BendLossCoefficient = 0.0,
            };
            //set a geometry
            culvert.GeometryType = CulvertGeometryType.Rectangle;
            culvert.Width = 4;
            culvert.Height = 5;
            culvert.GateOpeningLossCoefficientFunction.Arguments[0].SetValues(new[] { 0.0d, 1.0d});
            culvert.GateOpeningLossCoefficientFunction.SetValues(new[] { 0.0d, 1.0d});
            
            // create simplest network
            AddCulvertToModelAndRunModel(culvert);
        }

        /// <summary>
        /// Runs a simple model with a culvert. 
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteModelWithInverseSiphon()
        {
            // Add a culvert
            ICulvert culvert = new Culvert
            {
                FlowDirection = FlowDirection.Both,
                FrictionType = CulvertFrictionType.Manning,
                Friction = 0.2,
                Width = 30,
                Height = 10,
                CulvertType = CulvertType.InvertedSiphon,
                SiphonOnLevel = 0.2,
                SiphonOffLevel = 0.1,
                IsGated = true,
                GateInitialOpening = 1.0,
                Length = 10,
                InletLossCoefficient = 0.1,
                InletLevel = 0.3,
                OutletLossCoefficient = 0.2,
                OutletLevel = 0.4,
                BendLossCoefficient = 0.9,
            };
            //set a geometry
            culvert.GeometryType = CulvertGeometryType.Rectangle;
            culvert.Width = 4;
            culvert.Height = 5;

            culvert.GateOpeningLossCoefficientFunction[0.0] = 2.0;
            culvert.GateOpeningLossCoefficientFunction[1.0] = 1.0;


            // create simplest network
            AddCulvertToModelAndRunModel(culvert);
        }

        private void AddCulvertToModelAndRunModel(ICulvert culvert)
        {
            INode inflowNode;
            INode outflowNode;
            var hydroNetwork = WaterFlowModel1DTestHelper.CreateSimplerNetwork(out inflowNode, out outflowNode);

            var channel = hydroNetwork.Channels.First();

            var compositeBranchStructure = new CompositeBranchStructure
                                               {
                                                   Network = hydroNetwork, 
                                                   Geometry = new Point(5, 0), 
                                                   Chainage = 5
                                               };
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, channel, compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, culvert);

            // add discretization
            Discretization networkDiscretization = GetNetworkDiscretization(hydroNetwork);

            var flowModel1D = WaterFlowModel1DTestHelper.SetupModelForSimplerNetwork(networkDiscretization, hydroNetwork, inflowNode, outflowNode);
            WaterFlowModel1DTestHelper.RunInitializedModel(flowModel1D);
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