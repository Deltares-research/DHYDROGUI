using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.Converters.Geometries;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DCloneTest
    {
        [Test]
        public void CloneWaterFlowModel1D()
        {
            HydroNetwork network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            var startTime = new DateTime(2000, 1, 1);
            
            waterFlowModel1D.StartTime = startTime;
            waterFlowModel1D.StopTime = startTime.AddMinutes(5);
            waterFlowModel1D.TimeStep = new TimeSpan(0, 0, 30);
            waterFlowModel1D.OutputTimeStep = new TimeSpan(0, 0, 30);
            waterFlowModel1D.Name = "kees";
            waterFlowModel1D.OutputOutOfSync = true;

            //action! clone it
            WaterFlowModel1D clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();

            Assert.AreEqual(waterFlowModel1D.StartTime,clonedModel.StartTime);
            Assert.AreEqual(waterFlowModel1D.StopTime, clonedModel.StopTime);
            Assert.AreEqual(waterFlowModel1D.TimeStep, clonedModel.TimeStep);
            Assert.AreEqual(waterFlowModel1D.OutputTimeStep, clonedModel.OutputTimeStep);
            Assert.AreEqual(waterFlowModel1D.Name, clonedModel.Name);
            Assert.AreEqual(waterFlowModel1D.OutputOutOfSync, clonedModel.OutputOutOfSync);
            Assert.AreEqual(waterFlowModel1D.Network.CoordinateSystem, clonedModel.Network.CoordinateSystem);
        }

        [Test]
        public void CloneWaterFlowModel1DTemperatureRelated()
        {
            HydroNetwork network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            var startTime = new DateTime(2000, 1, 1);

            waterFlowModel1D.Name = "kees";
            waterFlowModel1D.UseTemperature = true;

            waterFlowModel1D.TemperatureModelType = TemperatureModelType.Composite;
            waterFlowModel1D.BackgroundTemperature = 59;

            waterFlowModel1D.StartTime = startTime;
            var meteoDataArguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument2) };

            waterFlowModel1D.MeteoData.Clear();
            waterFlowModel1D.MeteoData.Arguments[0].SetValues(meteoDataArguments);

            var valuesAirTemp = new[] {1.0, 51.0};
            var valuesHumidity = new[] { 1.0, 10.0 };
            var valuesCloudinesss = new[] { 10.0, 10.1 };
            waterFlowModel1D.MeteoData.AirTemperature.SetValues(valuesAirTemp);
            waterFlowModel1D.MeteoData.RelativeHumidity.SetValues(valuesHumidity);
            waterFlowModel1D.MeteoData.Cloudiness.SetValues(valuesCloudinesss);

            waterFlowModel1D.SurfaceArea = 10;
            waterFlowModel1D.AtmosphericPressure = 7;
            waterFlowModel1D.DaltonNumber = 8;
            waterFlowModel1D.StantonNumber = 9;
            waterFlowModel1D.HeatCapacityWater = 12;

            //Advanced options.
            waterFlowModel1D.DensityType = DensityType.unesco;
            waterFlowModel1D.Latitude = 23;
            waterFlowModel1D.Longitude = 32;

            //action! clone it
            WaterFlowModel1D clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();

            Assert.AreEqual(waterFlowModel1D.Name, clonedModel.Name);
            Assert.AreEqual(waterFlowModel1D.UseTemperature, clonedModel.UseTemperature);
            Assert.AreEqual(waterFlowModel1D.TemperatureModelType, clonedModel.TemperatureModelType);
            Assert.AreEqual(waterFlowModel1D.BackgroundTemperature, clonedModel.BackgroundTemperature);
            Assert.AreEqual(waterFlowModel1D.MeteoData.AirTemperature.GetValues(), clonedModel.MeteoData.AirTemperature.GetValues());
            Assert.AreEqual(waterFlowModel1D.MeteoData.RelativeHumidity.GetValues(), clonedModel.MeteoData.RelativeHumidity.GetValues());
            Assert.AreEqual(waterFlowModel1D.MeteoData.Cloudiness.GetValues(), clonedModel.MeteoData.Cloudiness.GetValues());
            Assert.AreEqual(waterFlowModel1D.SurfaceArea, clonedModel.SurfaceArea);
            Assert.AreEqual(waterFlowModel1D.AtmosphericPressure, clonedModel.AtmosphericPressure);
            Assert.AreEqual(waterFlowModel1D.DaltonNumber, clonedModel.DaltonNumber);
            Assert.AreEqual(waterFlowModel1D.StantonNumber, clonedModel.StantonNumber);
            Assert.AreEqual(waterFlowModel1D.HeatCapacityWater, clonedModel.HeatCapacityWater);
            Assert.AreEqual(waterFlowModel1D.DensityType, clonedModel.DensityType);
            Assert.AreEqual(waterFlowModel1D.Latitude, clonedModel.Latitude);
            Assert.AreEqual(waterFlowModel1D.Longitude, clonedModel.Longitude);

        }

        [Test]
        public void CloneWaterFlowModel1DNetworkShouldHaveWFM1DAsParent()
        {
            HydroNetwork network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            WaterFlowModel1D clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();
                       
            var clonedNetwork = clonedModel.DataItems.Where(i => i.Value is HydroNetwork).First();
            Assert.AreEqual(clonedModel, clonedNetwork.Owner);
         }

        [Test]
        public void CloneWaterFlowModel1DWithReverseRoughness()
        {
            HydroNetwork network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            waterFlowModel1D.UseReverseRoughness = true;

            var reverseRoughness = waterFlowModel1D.RoughnessSections.OfType<ReverseRoughnessSection>().First();
            reverseRoughness.UseNormalRoughness = false;

            WaterFlowModel1D clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();

            var clonedRoughness = clonedModel.RoughnessSections.First();
            var clonedReverseRoughness = clonedModel.RoughnessSections.OfType<ReverseRoughnessSection>().First();
            
            Assert.AreSame(clonedRoughness, clonedReverseRoughness.NormalSection);
            Assert.AreEqual(reverseRoughness.UseNormalRoughness, clonedReverseRoughness.UseNormalRoughness);
        }

        [Test]
        public void CloneWaterFlowModel1DNetworkShouldNOTHaveReferenceEqualsToOldNetwork()
        {
            HydroNetwork network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            WaterFlowModel1D clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();

            var clonedNetwork = clonedModel.DataItems.Where(i => i.Value is HydroNetwork).First();
            Assert.IsFalse(ReferenceEquals(network,clonedNetwork));
        }

/*
        [Test]
        public void CloneWaterFlowModel1DWithFlowTimeSeriesBoundaryCondition()
        {
            HydroNetwork network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };

            //get a flow timeseries from the data provider
                    var flowTimeSeries = HydroTimeSeriesFactory.CreateFlowTimeSeries();
            flowTimeSeries[new DateTime(2000, 1, 1)] = 5.0;
            flowTimeSeries[new DateTime(2000, 1, 2)] = 5.0;

            //flowBoundaryCondition.Data = flowTimeSeries;
            var firstBoundaryCondition = waterFlowModel1D.BoundaryConditions[0];

            firstBoundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            firstBoundaryCondition.Data = flowTimeSeries;
            
            //action! clone the model
            var clonedModel = (WaterFlowModel1D) waterFlowModel1D.Clone();

            var clonedFirstBoundaryCondition = clonedModel.BoundaryConditions[0];
            
            Assert.AreEqual(firstBoundaryCondition.DataType, clonedFirstBoundaryCondition.DataType);
            Assert.AreEqual(firstBoundaryCondition.Data.Arguments[0].Values, clonedFirstBoundaryCondition.Data.Arguments[0].Values);
            Assert.AreEqual(firstBoundaryCondition.Data.Components[0].Values, clonedFirstBoundaryCondition.Data.Components[0].Values);
            
        }
*/

        /// <summary>
        ///  Run the FlowModel1DNameDemoNetwork and check if it has actually calculated values.
        /// </summary>
        [Test]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void CloneDemoModelAndRun()
        {
            // use a valid network for the calculation
            using (var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var clonedModel = (WaterFlowModel1D) model.Clone();
                clonedModel.Initialize();

                WaterFlowModel1DTestHelper.RunInitializedModel(clonedModel);
            }
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void AutoCloneDemoModelAndRun()
        {
            // use a valid network for the calculation
            using (var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var clonedModel = TypeUtils.DeepClone(model);
                clonedModel.Initialize();

                var hits = TestReferenceHelper.SearchObjectInObjectGraph(model.Network, clonedModel);
                hits.ForEach(Console.WriteLine);
                Assert.AreEqual(0, hits.Count);

                WaterFlowModel1DTestHelper.RunInitializedModel(clonedModel);
            }
        }

/*
        [Test]
        [Category(TestCategory.Integration)]
        public void CloneModelWithSimpleNetworkAndLinkedBoundaryCondition()
        {
            // create simplest network
            var network = new HydroNetwork();

            IHydroNode node1 = new HydroNode { Name = "boundaryNode", Network = network };

            //get a flow timeseries from the data provider
            var flowTimeSeries = HydroTimeSeriesFactory.CreateFlowTimeSeries();
            flowTimeSeries[new DateTime(2000, 1, 1)] = 5.0;
            var dataItem = new DataItem(flowTimeSeries);
            dataItem.Name = "Amsterdam";

            // add nodes and branches
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network };
            IHydroNode node3 = new HydroNode { Name = "node3", Network = network };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);
            //network.Nodes.Add(node1);

            var branch1 = new Channel("branch1", node1, node2, 100.0);
            var vertices = new List<ICoordinate>
                                {
                                    new Coordinate(0, 0),
                                    new Coordinate(100, 0)
                                };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            var branch2 = new Channel("branch2", node2, node3, 150.0);
            vertices = new List<ICoordinate>
                            {
                                new Coordinate(100, 0),
                                new Coordinate(250, 0)
                            };
            branch2.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            // add discretization
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

            // setup 1d flow model
            var flowModel1D = new WaterFlowModel1D { NetworkDiscretization = networkDiscretization };

            WaterFlowModel1D.TemplateDataZipFile = WaterFlowModel1DTestHelper.TemplateDataDir;
            flowModel1D.RunInSeparateProcess = true;


            // set network
            flowModel1D.Network = network;

            var boundaryCondition = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node1);
            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;

            //action! link the bc to the dataitem
            boundaryCondition.SeriesDataItem.LinkTo(dataItem);

            WaterFlowModel1D clonedModel = (WaterFlowModel1D)flowModel1D.Clone();
            clonedModel.NetworkDiscretization.Locations.Values.Count.Should().Be.EqualTo(
                flowModel1D.NetworkDiscretization.Locations.Values.Count);
            clonedModel.BoundaryConditions.Count.Should().Be.EqualTo(3);
            clonedModel.BoundaryConditions.First(bc => bc.Feature.Name == "boundaryNode").SeriesDataItem.LinkedTo.Name.StartsWith("Amsterdam");
        }
*/

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneSimpleModelWithCrossSectionAndBoundaryConditions()
        {
            // create network
            var network = new HydroNetwork();

            // add nodes and branches
            var node1 = new HydroNode { Name = "node1", Network = network };
            var node2 = new HydroNode { Name = "node2", Network = network };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2, 100.0);

            network.Branches.Add(branch1);

            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };

            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            // add 2 cross-sections
            CrossSectionHelper.AddCrossSection(branch1, 10, -10);

            // note if cross sections are identical (or only 1 cs) run is succesfull
            //AddCrossSection(network, branch1, 90, -15);

            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, branch1, 0, true, 0.5, true, false, false, -1);

            // cell boundaries at cross section
            var startTime = DateTime.Now;

            var flowModel1D = new WaterFlowModel1D
            {
                NetworkDiscretization = networkDiscretization
            };

            flowModel1D.StartTime = startTime;
            flowModel1D.StopTime = startTime.AddMinutes(5);
            flowModel1D.TimeStep = new TimeSpan(0, 0, 30);
            flowModel1D.OutputTimeStep = new TimeSpan(0, 0, 30);
            flowModel1D.Network = network;

            WaterFlowModel1DTestHelper.AddFlowTimeBoundaryCondition(node1, flowModel1D, startTime);
            WaterFlowModel1DTestHelper.AddFlowDepthBoundary(node2, flowModel1D, startTime);


            WaterFlowModel1D clonedModel = (WaterFlowModel1D)flowModel1D.Clone();
                        
            for (int i = 0; i < flowModel1D.BoundaryConditions.FirstOrDefault().Data.Arguments[0].Values.Count; i++)
            {
                flowModel1D.BoundaryConditions.FirstOrDefault().Data.Arguments[0].Values[i].Should().Be.
                    EqualTo(clonedModel.BoundaryConditions.FirstOrDefault().Data.Arguments[0].Values[i]);
            }
            
            flowModel1D.BoundaryConditions.FirstOrDefault().Data.Components[0].Values.ToString()
                .Should().Be.EqualTo(clonedModel.BoundaryConditions.FirstOrDefault().Data.Components[0].Values.ToString());
        }

        [Test]
        public void CloneWaterFlowModel1DCheckParameterSettings()
        {
            HydroNetwork network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            //change one parametersetting value to make it exciting
            waterFlowModel1D.ParameterSettings[1].Value = "42";
            WaterFlowModel1D clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();

            for (int i = 0; i < waterFlowModel1D.ParameterSettings.Count; i++)
            {
                Assert.AreEqual(waterFlowModel1D.ParameterSettings[i].Name,clonedModel.ParameterSettings[i].Name);
                Assert.AreEqual(waterFlowModel1D.ParameterSettings[i].Value, clonedModel.ParameterSettings[i].Value);
            }
        }

        [Test]
        public void CloneWaterFlowModelInitialConditions()
        {
            var network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            var initialFlow = waterFlowModel1D.InitialFlow;
            initialFlow[new NetworkLocation(network.Branches[0], 10.0)] = 10.0;
            Assert.AreEqual(1, initialFlow.Segments.Values.Count);

            var clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();
            var cloneNetwork = clonedModel.Network;
            var clonedInitialFlow = clonedModel.InitialFlow;

            Assert.AreEqual(initialFlow.DefaultValue, clonedInitialFlow.DefaultValue, 1.0e-6);

            Assert.AreNotSame(cloneNetwork, network);
            Assert.AreEqual(10.0, clonedInitialFlow[new NetworkLocation(cloneNetwork.Branches[0], 10.0)]);
            Assert.AreEqual(1, clonedInitialFlow.Segments.Values.Count);
            initialFlow[new NetworkLocation(network.Branches[0], 2.0)] = 2.0;
            Assert.AreEqual(2, initialFlow.Segments.Values.Count);
            Assert.AreEqual(2.0, initialFlow.Locations.Values[0].Chainage, 1.0e-6);
            Assert.AreEqual(10.0, initialFlow.Locations.Values[1].Chainage, 1.0e-6);

            clonedInitialFlow[new NetworkLocation(cloneNetwork.Branches[0], 2.0)] = 2.0;
            Assert.AreEqual(2, clonedInitialFlow.Segments.Values.Count);
            Assert.AreEqual(2.0, clonedInitialFlow.Locations.Values[0].Chainage, 1.0e-6);
            Assert.AreEqual(10.0, clonedInitialFlow.Locations.Values[1].Chainage, 1.0e-6);
        }


        [Test]
        public void CloneWaterFlowModelRoughnessOfQ()
        {
            var network = GetNetwork();
            var waterFlowModel1D = new WaterFlowModel1D { Network = network };
            var roughnessSection = waterFlowModel1D.RoughnessSections.First();
            roughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.DefaultValue = 77.0;
            roughnessSection.RoughnessNetworkCoverage.Locations.Values.Add(new NetworkLocation(network.Branches[0], 10.0));
            roughnessSection.AddQRoughnessFunctionToBranch(network.Branches[0]);

            var clonedModel = (WaterFlowModel1D)waterFlowModel1D.Clone();
            var clonedRoughnessSection = clonedModel.RoughnessSections.First();

            Assert.AreEqual(roughnessSection.Name, clonedRoughnessSection.Name);
            Assert.AreEqual(RoughnessFunction.FunctionOfQ,
                            clonedRoughnessSection.GetRoughnessFunctionType(clonedModel.Network.Branches[0]));
            Assert.AreEqual(
                (double)roughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.DefaultValue,
                (double)clonedRoughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.DefaultValue, 1.0e-6);
            Assert.AreEqual((double)roughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.MinValue,
                            (double)clonedRoughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.MinValue,
                            1.0e-6);
            Assert.AreEqual((double)roughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.MaxValue,
                            (double)clonedRoughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.MaxValue,
                            1.0e-6);
            Assert.AreEqual(
                roughnessSection.RoughnessNetworkCoverage.RoughnessTypeComponent.DefaultValue,
                clonedRoughnessSection.RoughnessNetworkCoverage.RoughnessTypeComponent.DefaultValue);
            Assert.AreEqual(
                (double) clonedRoughnessSection.RoughnessNetworkCoverage.RoughnessValueComponent.DefaultValue,
                clonedRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(
                    new NetworkLocation(clonedModel.Network.Branches[0], 10.0)), 1.0e-6);
        }

        private static HydroNetwork GetNetwork()
        {
            var network = new HydroNetwork();
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel { Source = from, Target = to, Length = 100.0 };
            network.Branches.Add(channel);
            return network;
        }

    }
}
