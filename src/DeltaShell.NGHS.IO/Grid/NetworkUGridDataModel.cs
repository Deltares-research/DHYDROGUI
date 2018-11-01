using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public class NetworkUGridDataModel
    {
        public string Name;
        public int NetworkId;

        public int NumberOfNodes;
        public int NumberOfBranches;
        public int NumberOfGeometryPoints;

        public double[] NodesX = new double[0];
        public double[] NodesY = new double[0];
        public string[] NodesNames = new string[0];
        public string[] NodesDescriptions = new string[0];
        public int[] SourceNodeIds = new int[0];
        public int[] TargedNodesIds = new int[0];
        public double[] BranchLengths = new double[0];
        public int[] NumberOfGeometryPointsPerBranch = new int[0];
        public int[] BranchOrderNumbers = new int[0];
        public string[] BranchNames = new string[0];
        public string[] BranchDescriptions = new string[0];
        public double[] GeopointsX = new double[0];
        public double[] GeopointsY = new double[0];
        public ICoordinateSystem CoordinateSystem;

        public NetworkUGridDataModel(IHydroNetwork network)
        {
            SetNetworkData(network);
        }

        public NetworkUGridDataModel(string networkName, ICoordinateSystem coordinateSystem, double[] nodesX, double[] nodesY, string[] nodesNames, string[] nodesDescriptions, int[] sourceNodes, int[] targetNodes, double[] branchLengths, int[] branchGeometryPoints, string[] branchNames, string[] branchDescriptions, double[] geometryPointsX, double[] geometryPointsY, int[] branchOrderNumbers)
        {
            Name = networkName;
            CoordinateSystem = coordinateSystem;

            NodesX = nodesX;
            NodesY = nodesY;
            NodesNames = nodesNames;
            NodesDescriptions = nodesDescriptions;
            SourceNodeIds = sourceNodes;
            TargedNodesIds = targetNodes;

            BranchLengths = branchLengths;
            NumberOfGeometryPointsPerBranch = branchGeometryPoints;
            BranchNames = branchNames;
            BranchDescriptions = branchDescriptions;
            BranchOrderNumbers = branchOrderNumbers;

            GeopointsX = geometryPointsX;
            GeopointsY = geometryPointsY;

            NumberOfNodes = nodesNames.Length;
            NumberOfBranches = branchNames.Length;
            NumberOfGeometryPoints = branchGeometryPoints.Sum();
        }

        private void SetNetworkData(IHydroNetwork network)
        {
            if (network == null) return;
            Name = network.Name ?? string.Empty;

            var compartmentCoordinateDictionary = new Dictionary<string, Coordinate>();

            if (network.Nodes != null)
            {
                var compartments = new List<Compartment>();
                network.Manholes.ForEach(m =>
                {
                    compartments.AddRange(m.Compartments);
                });
                var compartmentCount = compartments.Count;
                
                var nonManholeNetworkNodes = network.Nodes.Where(n => !(n is IManhole)).ToList();

                // The compartment coordinates are adjusted slightly for writing to UGRID
                var compartmentsX = new List<double>();
                var compartmentsY = new List<double>();
                network.Manholes.ForEach(m =>
                {
                    var numOfCompartments = m.Compartments.Count;
                    var offset = (numOfCompartments - 1) * 0.5;
                    for (var i = 0; i < numOfCompartments; i++)
                    {
                        var compartmentX = m.Geometry.Coordinate.X - offset + i;
                        var compartmentY = m.Geometry.Coordinate.Y;
                        compartmentsX.Add(compartmentX);
                        compartmentsY.Add(compartmentY);
                        compartmentCoordinateDictionary.Add(m.Compartments[i].Name, new Coordinate(compartmentX, compartmentY));
                    }
                });

                NumberOfNodes = nonManholeNetworkNodes.Count + compartmentCount;
                NodesX = nonManholeNetworkNodes.Select(n => n.Geometry.Coordinates[0].X).Concat(compartmentsX).ToArray();
                NodesY = nonManholeNetworkNodes.Select(n => n.Geometry.Coordinates[0].Y).Concat(compartmentsY).ToArray();
                NodesNames = nonManholeNetworkNodes.Select(n => n.Name).Concat(compartments.Select(c => c.Name)).ToArray();
                NodesDescriptions = nonManholeNetworkNodes.Select(n => n.Description).Concat(compartments.Select(c => string.Empty)).ToArray();
            }

            if (network.Branches != null)
            {
                NumberOfBranches = network.Branches.Count;
                var sourceNames = network.Branches.Select(b =>
                {
                    var sewerConnection = b as SewerConnection;
                    return sewerConnection != null ? sewerConnection.SourceCompartmentName : b.Source.Name;
                }).ToArray();
                SourceNodeIds = sourceNames.Select(n => NodesNames.ToList().IndexOf(n)).ToArray();

                var targetNames = network.Branches.Select(b =>
                {
                    var sewerConnection = b as SewerConnection;
                    return sewerConnection != null ? sewerConnection.TargetCompartmentName : b.Target.Name;
                }).ToArray();
                TargedNodesIds = targetNames.Select(n => NodesNames.ToList().IndexOf(n)).ToArray();

                BranchLengths = network.Branches.Select(b => b.Length).ToArray();

                NumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                NumberOfGeometryPointsPerBranch = network.Branches.Select(b => b.Geometry?.Coordinates?.Length ?? 0).ToArray();

                BranchNames = network.Branches.Select(b => b.Name).ToArray();
                BranchDescriptions = network.Branches.Select(b => b.Description).ToArray();
                BranchOrderNumbers = network.Branches.Select(b => b.OrderNumber).ToArray();
                
                var nonSewerConnections = network.Branches.Where(b => !(b is SewerConnection)).ToArray();

                // Determine the end points of the sewer connections,
                // because the compartment coordinates are adjusted slightly
                var sourceAndTargetCompartments = new List<string>();
                network.SewerConnections.ForEach(sc =>
                {
                    sourceAndTargetCompartments.Add(sc.SourceCompartment.Name);
                    sourceAndTargetCompartments.Add(sc.TargetCompartment.Name);
                });

                var compartmentXCoordinates = sourceAndTargetCompartments.Select(name => compartmentCoordinateDictionary[name].X);
                var compartmentYCoordinates = sourceAndTargetCompartments.Select(name => compartmentCoordinateDictionary[name].Y);

                GeopointsX = nonSewerConnections.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X)).Concat(compartmentXCoordinates).ToArray();
                GeopointsY = nonSewerConnections.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y)).Concat(compartmentYCoordinates).ToArray();
            }
        }
    }
}

