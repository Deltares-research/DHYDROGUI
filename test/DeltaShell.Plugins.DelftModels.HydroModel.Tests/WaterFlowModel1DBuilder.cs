using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DeveloperTools.Builders
{
    public static class WaterFlowModel1DBuilder
    {
        public static WaterFlowModel1D CreateModelWithDemoNetwork(bool addCrossSections = true)
        {
            var network = CreateDemoNetwork(addCrossSections);

            var model = new WaterFlowModel1D("flow model 1d (demo network)")
                {
                    NetworkDiscretization = new Discretization
                        {
                            Name = WaterFlowModel1DDataSet.DiscretizationDataObjectName,
                            Network = network,
                            SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                        }
                };

            var offsets1 = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel) network.Branches[0], offsets1);

            var offsets2 = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)network.Branches[1], offsets2);

            var now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            model.StartTime = t;
            model.StopTime = t.AddMinutes(5);
            model.TimeStep = new TimeSpan(0, 0, 30);
            model.OutputTimeStep = new TimeSpan(0, 0, 30);

            model.SaveStateStartTime = model.StartTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.OutputTimeStep;

            model.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Observations).AggregationOptions = AggregationOptions.Current;
            model.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Observations).AggregationOptions = AggregationOptions.Average;
            model.OutputSettings.StructureOutputTimeStep = model.TimeStep;

            // set network
            model.Network = network;

            // set initial conditions
            model.InitialFlow.DefaultValue = 0.1;// .SetValues(new[] {0.1});
            model.InitialConditions.DefaultValue = 0.1;//.SetValues(new[] {0.1});
            // make sure it is in sync with initial conditions
            model.DefaultInitialWaterLevel = 0.1;

            // set boundary conditions
            var boundaryConditionInflow = model.BoundaryConditions.First(bc => bc.Feature == network.Nodes[0]);
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
            boundaryConditionInflow.Data[t] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(30)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(60)] = 1.5;
            boundaryConditionInflow.Data[t.AddSeconds(120)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(180)] = 0.5;
            boundaryConditionInflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            var boundaryConditionOutflow = model.BoundaryConditions.First(bc => bc.Feature == network.Nodes[2]);
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
            boundaryConditionOutflow.Data[t] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(30)] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(60)] = 0.2;
            boundaryConditionOutflow.Data[t.AddSeconds(120)] = 0.3;
            boundaryConditionOutflow.Data[t.AddSeconds(180)] = 0.1;
            boundaryConditionOutflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            return model;
        }

        public static WaterFlowModel1D CreateModelWithLargeNetwork(int rowcolCount)
        {
            var mainChannel = new CrossSectionSectionType { Name = "Main" };
            var network = CreateLargeNetwork(mainChannel, rowcolCount);

            var model = new WaterFlowModel1D(string.Format("Flow Model 1D ({0} x {0})", rowcolCount))
                {
                    NetworkDiscretization = new Discretization
                        {
                            Name = WaterFlowModel1DDataSet.DiscretizationDataObjectName,
                            Network = network,
                            SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                        }
                };

            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, true, false, 10.0, true, 10.0, true, false, false, -10.0);

            var now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            model.StartTime = t;
            model.StopTime = t.AddMinutes(5);
            model.TimeStep = new TimeSpan(0, 0, 30);
            model.OutputTimeStep = new TimeSpan(0, 0, 30);

            // set network
            model.Network = network;

            //// set initial conditions
            model.InitialFlow.DefaultValue = 0.1;
            model.InitialConditions.DefaultValue = 1.0;
            // make sure it is in sync with initial conditions
            model.DefaultInitialWaterLevel = 1.0;

            // set boundary conditions
            var boundaryConditionInflow = model.BoundaryConditions.First(bc => bc.Feature == network.Nodes[0]);
            boundaryConditionInflow.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.Flow = 2.0;

            var boundaryConditionOutflow = model.BoundaryConditions.First(bc => bc.Feature == network.Nodes[2]);
            boundaryConditionOutflow.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = 1.0;

            return model;
        }

        private static IHydroNetwork CreateLargeNetwork(CrossSectionSectionType crossSectionSectionType, int cellCount)
        {
            IHydroNode[] nodes = new HydroNode[cellCount * cellCount];
            var network = new HydroNetwork();

            for (var y = 0; y < cellCount; y++)
            {
                for (var x = 0; x < cellCount; x++)
                {
                    IHydroNode hydroNode = new HydroNode { Name = string.Format("x={0}, y={1}", x, y), Network = network };
                    hydroNode.Geometry = new Point(0.0 + x * 100.0, 0.0 + y * 100.0);
                    nodes[(cellCount * y) + x] = hydroNode;
                    network.Nodes.Add(hydroNode);
                    if (x > 0)
                    {
                        IHydroNode previous = nodes[(cellCount * y) + x - 1];
                        AddCrossSection(string.Format("{0},{1}->{2},{3}", x - 1, y, x, y), network, previous, hydroNode);
                    }
                    if (y > 0)
                    {
                        IHydroNode previous = nodes[(cellCount * (y - 1)) + x];
                        AddCrossSection(string.Format("{0},{1}->{2},{3}", x, y - 1, x, y), network, previous, hydroNode);
                    }
                }
            }

            var start = new HydroNode { Name = "start", Network = network, Geometry = new Point(-100.0, -100.0) };
            AddCrossSection("start -> 0,0", network, start, nodes[0]);
            network.Nodes.Add(start);

            var end = new HydroNode
                {
                    Name = "end",
                    Network = network,
                    Geometry = new Point(100.0*cellCount, 100.0*cellCount)
                };

            AddCrossSection(string.Format("{0},{0} -> end", cellCount - 1), network, nodes[nodes.Length - 1], end);
            network.Nodes.Add(end);

            return network;
        }

        private static void AddCrossSection(string branchName, HydroNetwork network, IHydroNode previous, IHydroNode hydroNode)
        {
            var branch = new Channel(branchName, previous, hydroNode)
                {
                    Geometry = new LineString(new Coordinate[]
                        {
                            (Coordinate) previous.Geometry.Coordinates[0].Clone(),
                            (Coordinate) hydroNode.Geometry.Coordinates[0].Clone()
                        })
                };

            network.Branches.Add(branch);
            var crossSectionYz = new CrossSectionDefinitionYZ();
            crossSectionYz.SetDefaultYZTableAndUpdateThalWeg();

            var crossSectionBranchFeature = new CrossSection(crossSectionYz) { Chainage = branch.Length / 2 };
            NetworkHelper.AddBranchFeatureToBranch(crossSectionBranchFeature, branch, crossSectionBranchFeature.Chainage);
        }

        private static IHydroNetwork CreateDemoNetwork(bool addCrossSections = true)
        {
            // create simplest network
            var network = new HydroNetwork();

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

            var branch1 = new Channel("branch1", node1, node2);
            var branch2 = new Channel("branch2", node2, node3);

            branch1.Geometry = new LineString(new []
                                                  {
                                                      new Coordinate(0, 0),
                                                      new Coordinate(50, 0),
                                                      new Coordinate(100, 0),
                                                      new Coordinate(100, 0)
                                                  });

            branch2.Geometry = new LineString(new []
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
                        Geometry = new LineString(new []
                            {
                                new Coordinate(50, 0),
                                new Coordinate(60, 0)
                            })
                    };

                var crossSection2 = new CrossSectionDefinitionXYZ("crs2")
                    {
                        Geometry = new LineString(new []
                            {
                                new Coordinate(100, 75),
                                new Coordinate(110, 75)
                            })
                    };


                //branch1.BranchFeatures.Add(crossSection1);
                var csFeature1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, crossSection1, 50);
                csFeature1.Name = "cs1";

                //NetworkHelper.AddBranchFeatureToBranch(branch1, crossSection1, crossSection1.Offset);
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

            return network;
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
    }
}
