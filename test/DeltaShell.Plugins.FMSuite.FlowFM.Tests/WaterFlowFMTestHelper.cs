using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    public class WaterFlowFMTestHelper
    {
        public static WaterFlowFMModel CreateModelWithDemoNetwork(bool addCrossSections = true)
        {
            var network = new HydroNetwork();
            var model = new WaterFlowFMModel { Network = network };

            ConfigureDemoNetwork(network, addCrossSections);
            ConfigureAsDemoModel(model);

            return model;
        }

        private static void ConfigureAsDemoModel(WaterFlowFMModel model)
        {
            var network = model.Network;

            model.Name = "FlowFM (demo network)";

            model.NetworkDiscretization = new Discretization
            {
                Name = WaterFlowFMModel.DiscretizationObjectName,
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            var offsets1 = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)network.Branches[0], offsets1);

            var offsets2 = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)network.Branches[1], offsets2);

            var now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            model.StartTime = t;
            model.StopTime = t.AddMinutes(5);
            model.TimeStep = new TimeSpan(0, 0, 30);
            model.OutputTimeStep = new TimeSpan(0, 0, 30);

            // set boundary conditions

            var boundaryInFlow = new Feature2D { Name = "left", Geometry = new LineString(new[] { new Coordinate(0, 5), new Coordinate(0, 0) }) };
            var boundaryConditionInflow = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                    BoundaryConditionDataType.TimeSeries)
                { Feature = boundaryInFlow };

            boundaryConditionInflow.DataType = BoundaryConditionDataType.TimeSeries;
            boundaryConditionInflow.AddPoint(0);
            boundaryConditionInflow.PointData[0][t] = 0.5;
            boundaryConditionInflow.PointData[0][t] = 1.0;
            boundaryConditionInflow.PointData[0][t.AddSeconds(30)] = 1.0;
            boundaryConditionInflow.PointData[0][t.AddSeconds(60)] = 1.5;
            boundaryConditionInflow.PointData[0][t.AddSeconds(120)] = 1.0;
            boundaryConditionInflow.PointData[0][t.AddSeconds(180)] = 0.5;
            boundaryConditionInflow.PointData[0].Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            var boundaryOutFlow = new Feature2D { Name = "right", Geometry = new LineString(new[] { new Coordinate(0, 5), new Coordinate(0, 0) }) };
            var boundaryConditionOutflow = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                    BoundaryConditionDataType.TimeSeries)
                { Feature = boundaryOutFlow };

            boundaryConditionOutflow.DataType = BoundaryConditionDataType.TimeSeries;
            boundaryConditionOutflow.AddPoint(0);
            boundaryConditionOutflow.PointData[0][t] = 0.1;
            boundaryConditionOutflow.PointData[0][t.AddSeconds(30)] = 0.1;
            boundaryConditionOutflow.PointData[0][t.AddSeconds(60)] = 0.2;
            boundaryConditionOutflow.PointData[0][t.AddSeconds(120)] = 0.3;
            boundaryConditionOutflow.PointData[0][t.AddSeconds(180)] = 0.1;
            boundaryConditionOutflow.PointData[0].Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            model.Boundaries.Add(boundaryInFlow);
            model.Boundaries.Add(boundaryOutFlow);
        }

        public static void ConfigureDemoNetwork(IHydroNetwork network, bool addCrossSections = true)
        {
            // create simplest network
            var crossSectionSectionType = network.CrossSectionSectionTypes.First();

            // add nodes and branches
            INode node1 = new HydroNode { Name = "Node1", Network = network };
            INode node2 = new HydroNode { Name = "Node2", Network = network };
            INode node3 = new HydroNode { Name = "Node3", Network = network };

            // create simplest network
            node1.Geometry = new Point(0.0, 0.0);
            node2.Geometry = new Point(100.0, 0.0);
            node3.Geometry = new Point(100.0, 150.0);

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2, 100.0);
            var branch2 = new Channel("branch2", node2, node3, 150.0);

            branch1.Geometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(50, 0),
                new Coordinate(100, 0),
                new Coordinate(100, 0)
            });

            branch2.Geometry = new LineString(new[]
            {
                new Coordinate(100, 0),
                new Coordinate(100, 50),
                new Coordinate(100, 100),
                new Coordinate(100, 150)
            });

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            if (addCrossSections)
            {

                var crossSection1 = new CrossSectionDefinitionXYZ("crs1")
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(50, 0),
                        new Coordinate(60, 0)
                    })
                };

                var crossSection2 = new CrossSectionDefinitionXYZ("crs2")
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(100, 75),
                        new Coordinate(110, 75)
                    })
                };

                var csFeature1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, crossSection1, 50);
                csFeature1.Name = "cs1";

                crossSection1.Geometry = CreateGeometryForCs1(csFeature1);
                crossSection1.Sections.Add(new CrossSectionSection
                {
                    MinY = crossSection1.Profile.Select(yz => yz.X).Min(),
                    MaxY = crossSection1.Profile.Select(yz => yz.X).Max(),
                    SectionType = crossSectionSectionType
                });

                var csFeature2 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch2, crossSection2, 75);
                csFeature2.Name = "cs2";

                crossSection2.Geometry = CreateGeometryForCs2(csFeature2);
                crossSection2.Sections.Add(new CrossSectionSection
                {
                    MinY = crossSection2.Profile.Select(yz => yz.X).Min(),
                    MaxY = crossSection2.Profile.Select(yz => yz.X).Max(),
                    SectionType = crossSectionSectionType
                });
            }
        }

        public static void ConfigureDemoPipeNetwork(IHydroNetwork network)
        {
            // add nodes and branches
            INode node1 = new Manhole { Name = "Node1", Network = network };
            INode node2 = new Manhole { Name = "Node2", Network = network };
            INode node3 = new Manhole { Name = "Node3", Network = network };
            INode node4 = new Manhole { Name = "Node4", Network = network };

            // create pipe network
            node1.Geometry = new Point(1, 0.5);
            node2.Geometry = new Point(3.0, 1.0);
            node3.Geometry = new Point(1.0, 2.5);
            node4.Geometry = new Point(2.5, 3.5);

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);
            network.Nodes.Add(node4);

            var pipe1 = new Pipe {Name = "pipe1", Network = network, Source = node1, Target = node2, Length = 2.06};
            var pipe2 = new Pipe {Name = "pipe2", Network = network, Source = node2, Target = node3, Length = 2.5};
            var pipe3 = new Pipe {Name = "pipe3", Network = network, Source = node3, Target = node4, Length = 2.24};

            pipe1.Geometry = new LineString(new[]
            {
                new Coordinate(1, 0.5),
                new Coordinate(3, 1)
            });

            pipe2.Geometry = new LineString(new[]
            {
                new Coordinate(3, 1),
                new Coordinate(1, 2.5)
            });

            pipe3.Geometry = new LineString(new[]
            {
                new Coordinate(1, 2.5),
                new Coordinate(2.5, 3.5)
            });

            SewerFactory.AddDefaultPipeToNetwork(pipe1, network);
            SewerFactory.AddDefaultPipeToNetwork(pipe2, network);
            SewerFactory.AddDefaultPipeToNetwork(pipe3, network);
        }

        public static void ConfigureDemoNetworkAtGivenCoordinates(IHydroNetwork network)
        {
            if( network == null)
                network = new HydroNetwork();

            // add nodes and branches
            INode node1 = new HydroNode { Name = "Node1", Network = network };
            INode node3 = new HydroNode { Name = "Node3", Network = network };

            // create simplest network
            node1.Geometry = new Point(-4.0, 20.0);
            node3.Geometry = new Point(35.0, 5.0);

            network.Nodes.Add(node1);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node3, 100.0);

            branch1.Geometry = new LineString(new[]
            {
                new Coordinate(0.0, 22.0),
                new Coordinate(10.0, 10.0),
                new Coordinate(20.0, 0.0),
                new Coordinate(25.0, -5.0)
            });

            network.Branches.Add(branch1);
        }

        private static IGeometry CreateGeometryForCs1(ICrossSection crossSection)
        {
            IList<Coordinate> yzCoordinates = new List<Coordinate>
            {
                new Coordinate(0.0, 3),
                new Coordinate(10.0, 2),
                new Coordinate(20.0, 1),
                new Coordinate(30.0, 0),
                new Coordinate(40.0, 0),
                new Coordinate(50.0, 1),
                new Coordinate(60.0, 2),
                new Coordinate(70.0, 3)
            };

            return CrossSectionHelper.CreateCrossSectionGeometryForXyzCrossSectionFromYZ(crossSection.Branch.Geometry, crossSection.Chainage, yzCoordinates);
        }

        private static IGeometry CreateGeometryForCs2(ICrossSection crossSection)
        {
            IList<Coordinate> yzCoordinates = new List<Coordinate>
            {
                new Coordinate(0.0, 3),
                new Coordinate(10.0, 2),
                new Coordinate(20.0, 1),
                new Coordinate(30.0, 0),
                new Coordinate(40.0, 0),
                new Coordinate(50.0, 1),
                new Coordinate(60.0, 2),
                new Coordinate(70.0, 3)
            };

            return CrossSectionHelper.CreateCrossSectionGeometryForXyzCrossSectionFromYZ(crossSection.Branch.Geometry, crossSection.Chainage, yzCoordinates);
        }

        public static void ConfigureDemoNetwork2Branches(HydroNetwork network)
        {
            // add nodes and branches
            INode node1 = new HydroNode { Name = "Node1", Network = network };
            INode node2 = new HydroNode { Name = "Node2", Network = network };
            INode node3 = new HydroNode { Name = "Node3", Network = network };

            // create simplest network
            node1.Geometry = new Point(-50, 250);
            node2.Geometry = new Point(-50, 150);
            node3.Geometry = new Point(300, 150);

            network.Nodes.Add(node1);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2, 100);
            branch1.Geometry = new LineString(new[]
            {
                new Coordinate(-50, 250.0),
                new Coordinate(-30, 200.0),
                new Coordinate(-50, 150.0),
            });
            network.Branches.Add(branch1);

            var branch2 = new Channel("branch2", node2, node3, 350);
            branch1.Geometry = new LineString(new[]
            {
                new Coordinate(-50, 150.0),
                new Coordinate(300, 150.0),
            });
            network.Branches.Add(branch1);
        }
    }
}