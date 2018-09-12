using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils
{
    public static class WaterFlowModel1DModelMergeTestHelper
    {
        public static WaterFlowModel1D SetupWFM1D(double from, double to, string name = "Flow1D")
        {
            var network = new HydroNetwork();
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            network.CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992); // RD new

            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(new Coordinate(from, 0)) };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(new Coordinate(to, 0)) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            
            var channel = new Channel(node1, node2);
            var vertices = new List<Coordinate>
            {
                new Coordinate(from, 0),
                new Coordinate(to, 0)
            };
            channel.Geometry = GeometryFactory.Default.CreateLineString(vertices.ToArray());
            var crossSection = CrossSectionDefinitionZW.CreateDefault();
            crossSection.AddSection(network.CrossSectionSectionTypes.FirstOrDefault(), 100.0);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSection, (to - from)/2);

            network.Branches.Add(channel);

            var waterFlowModel1D = new WaterFlowModel1D() { Network = network, Name = name };
            waterFlowModel1D.BoundaryConditions.ForEach(bc =>
            {
                bc.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                bc.Flow = 42;
            });
            return waterFlowModel1D;
        }

        public static IDiscretization SetupUniformNetworkDiscretization(INetwork network, int numberOfPointsPerBranch)
        {
            var networkDiscretization = new Discretization(){ Network = network };
            foreach (var branch in network.Branches)
            {
                var spacing = branch.Length / (numberOfPointsPerBranch - 1);
                for (var i = 0; i < numberOfPointsPerBranch; i++)
                {
                    networkDiscretization.Locations.Values.Add(new NetworkLocation(branch, i * spacing));
                }
            }
            return networkDiscretization;
        }

        public static void SetupCoverageLocations(INetworkCoverage coverage, int numberOfLocationsPerBranch)
        {
            foreach (var branch in coverage.Network.Branches)
            {
                var spacing = branch.Length / numberOfLocationsPerBranch;
                for (var i = 0; i < numberOfLocationsPerBranch; i++)
                {
                    coverage.Arguments[0].Values.Add(new NetworkLocation(branch, i*spacing));
                }
            }
        }

        public static void SetupCoverageLocationsInSpecificBranchOrder(INetworkCoverage coverage, int numberOfLocationsPerBranch, List<int> branchOrder)
        {
            var isAutoSorted = coverage.Arguments[0].IsAutoSorted;

            // disable auto sorting if neccessary
            if (isAutoSorted) coverage.Arguments[0].IsAutoSorted = false;
            foreach (var branchIndex in branchOrder)
            {
                var branch = coverage.Network.Branches[branchIndex];
                var spacing = branch.Length / numberOfLocationsPerBranch;
                for (var i = 0; i < numberOfLocationsPerBranch; i++)
                {
                    coverage.Arguments[0].Values.Add(new NetworkLocation(branch, i * spacing));
                }
            }
            // reset auto sorting to original value
            if (isAutoSorted) coverage.Arguments[0].IsAutoSorted = true;
        }
    }
}