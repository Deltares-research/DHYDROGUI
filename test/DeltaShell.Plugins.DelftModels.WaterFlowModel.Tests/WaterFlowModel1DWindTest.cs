using System;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DWindTest
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
        public void RunSimpleWithAndWithoutWindModel()
        {
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);
            var model = new WaterFlowModel1D
                            {
                                StartTime = startTime,
                                StopTime = stopTime
                            };
            //a node network with single branch and 2 nodes
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(1000, 0));
            model.Network = network;

            //add a single crossection halfway the branch
            CrossSectionHelper.AddCrossSection(network.Channels.First(), 500, -5);
            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            //generate discretization 
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 200, false,
                                                      0.5, false, false, true, 200);


            Model1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
            qBoundary.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            qBoundary.Flow = 2.0;

            Model1DBoundaryNodeData hBoundary = model.BoundaryConditions[1];
            hBoundary.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            hBoundary.WaterLevel = 3.0;

            model.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

            model.Initialize();
            WaterFlowModel1DTestHelper.RunInitializedModel(model);

            //check influence of wind at random point
            var location = model.OutputWaterLevel.Locations.Values[4];
            var time = model.OutputWaterLevel.Time.Values[10];
            var valueWithoutWind = (double)model.OutputWaterLevel[time, location];

            model.Wind[startTime] = new[] {60.0, 90.0}; //velocity, direction
            model.Wind[stopTime] = new[] { 60.0, 90.0 }; //velocity, direction

            model.Initialize();
            WaterFlowModel1DTestHelper.RunInitializedModel(model);

            var valueWithWind = (double)model.OutputWaterLevel[time, location];

            Assert.AreNotEqual(valueWithoutWind, valueWithWind);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunSimpleWithDifferentWindDirectionModel()
        {
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);
            var model = new WaterFlowModel1D
                            {
                                StartTime = startTime,
                                StopTime = stopTime
                            };
            //a node network with single branch and 2 nodes
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(1000, 0));
            model.Network = network;

            //add a single crossection halfway the branch
            CrossSectionHelper.AddCrossSection(network.Channels.First(), 500, -5);
            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            //generate discretization 
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 200, false,
                                                      0.5, false, false, true, 200);


            Model1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
            qBoundary.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            qBoundary.Flow = 2.0;

            Model1DBoundaryNodeData hBoundary = model.BoundaryConditions[1];
            hBoundary.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            hBoundary.WaterLevel = 3.0;

            model.Wind[startTime] = new[] { 60.0, 90.0 }; //velocity, direction
            model.Wind[stopTime] = new[] { 60.0, 90.0 }; //velocity, direction

            model.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

            model.Initialize();
            WaterFlowModel1DTestHelper.RunInitializedModel(model);

            //check influence of wind at random point
            var location = model.OutputWaterLevel.Locations.Values[4];
            var time = model.OutputWaterLevel.Time.Values[10];
            var valueWindEast = (double)model.OutputWaterLevel[time, location];

            model.Wind[startTime] = new[] { 60.0, 270.0 }; //velocity, direction
            model.Wind[stopTime] = new[] { 60.0, 270.0 }; //velocity, direction

            model.Initialize();
            WaterFlowModel1DTestHelper.RunInitializedModel(model);

            var valueWindWest = (double)model.OutputWaterLevel[time, location];

            Assert.AreNotEqual(valueWindEast, valueWindWest);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunSimpleWithAndWithoutShieldingModel()
        {
            var startTime = new DateTime(2000, 1, 1);
            var stopTime = new DateTime(2000, 1, 10);
            var model = new WaterFlowModel1D
                            {
                                StartTime = startTime,
                                StopTime = stopTime
                            };
            //a node network with single branch and 2 nodes
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(1000, 0));
            model.Network = network;

            //add a single crossection halfway the branch
            CrossSectionHelper.AddCrossSection(network.Channels.First(), 500, -5);
            WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

            //generate discretization 
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 200, false,
                                                      0.5, false, false, true, 200);


            Model1DBoundaryNodeData qBoundary = model.BoundaryConditions[0];
            qBoundary.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            qBoundary.Flow = 2.0;

            Model1DBoundaryNodeData hBoundary = model.BoundaryConditions[1];
            hBoundary.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            hBoundary.WaterLevel = 3.0;

            model.Wind[startTime] = new[] { 60.0, 90.0 }; //velocity, direction
            model.Wind[stopTime] = new[] { 60.0, 90.0 }; //velocity, direction

            model.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

            model.Initialize();
            WaterFlowModel1DTestHelper.RunInitializedModel(model);

            //check influence of wind at random point
            var location = model.OutputWaterLevel.Locations.Values[4];
            var time = model.OutputWaterLevel.Time.Values[10];
            var valueWithoutShielding = (double)model.OutputWaterLevel[time, location];

            foreach (var loc in model.NetworkDiscretization.Locations.Values)
            {
                model.WindShielding[loc] = 0.5;
            }

            model.Initialize();
            WaterFlowModel1DTestHelper.RunInitializedModel(model);

            var valueWithShielding = (double)model.OutputWaterLevel[time, location];

            Assert.AreNotEqual(valueWithoutShielding, valueWithShielding);
        }
    }
}
