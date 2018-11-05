using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridUGridDataModel
    {
        public GridWrapper.meshgeomdim Dimensions
        {
            get { return dimensions; }
            set { dimensions = value; }
        }

        public GridWrapper.meshgeom Data
        {
            get { return data; }
            set { data = value; }
        }

        private GridWrapper.meshgeomdim dimensions;
        private GridWrapper.meshgeom data;

        public GridUGridDataModel(UnstructuredGrid grid)
        {
            Dimensions = new GridWrapper.meshgeomdim();
            Data = new GridWrapper.meshgeom();
            SetGridData(grid);
        }

        private void SetGridData(UnstructuredGrid grid)
        {
            dimensions.dim = 2;
            dimensions.layertype = 0;
            dimensions.numlayer = 0;
            dimensions.numnode = grid.Vertices.Count;
            dimensions.numedge = grid.Edges.Count;
            dimensions.numface = grid.Cells.Count;
            dimensions.maxnumfacenodes = grid.Cells.Select(c => c.VertexIndices.Length).Max();
            //MeshgeomMemoryManager.allocate(ref dimensions, ref data);
            var edgesx = new List<double>();
            var edgesy = new List<double>();
            var edgesz = new List<double>();
            var edges = new List<int>();
            foreach (var edge in grid.Edges)
            {
                edges.Add(edge.VertexFromIndex + 1);
                edges.Add(edge.VertexToIndex + 1);
                var edgeCenter = edge.GetEdgeCenter(grid);
                edgesx.Add(edgeCenter.X);
                edgesy.Add(edgeCenter.Y);
                edgesz.Add(edgeCenter.Z);
            }
            var faces = new List<int>();
            var facesx = new List<double>();
            var facesy = new List<double>();
            foreach (var cell in grid.Cells)
            {
                for(var i = 0 ; i < dimensions.maxnumfacenodes; i++)
                {
                    if( i < cell.VertexIndices.Length )
                        faces.Add(cell.VertexIndices[i] + 1);
                    else
                        faces.Add(-999);
                }
                facesx.Add(cell.CenterX);
                facesy.Add(cell.CenterY);
            }
            data.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * dimensions.numedge * 2);
            Marshal.Copy(edges.ToArray(), 0, data.edge_nodes, dimensions.numedge * 2);

            data.edgex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numedge);
            Marshal.Copy(edgesx.ToArray(), 0, data.edgex, dimensions.numedge);

            data.edgey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numedge);
            Marshal.Copy(edgesy.ToArray(), 0, data.edgey, dimensions.numedge);

            data.edgez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numedge);
            Marshal.Copy(edgesz.ToArray(), 0, data.edgez, dimensions.numedge);

            data.face_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * dimensions.numface * dimensions.maxnumfacenodes);
            Marshal.Copy(faces.ToArray(), 0, data.face_nodes, dimensions.numface * dimensions.maxnumfacenodes);

            data.facex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numface);
            Marshal.Copy(facesx.ToArray(), 0, data.facex, dimensions.numface);

            data.facey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numface);
            Marshal.Copy(facesy.ToArray(), 0, data.facey, dimensions.numface);

            data.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numnode);
            Marshal.Copy(grid.Vertices.Select(v => v.CoordinateValue.X).ToArray(), 0, data.nodex, dimensions.numnode);

            data.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numnode);
            Marshal.Copy(grid.Vertices.Select(v => v.CoordinateValue.Y).ToArray(), 0, data.nodey, dimensions.numnode);

            data.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * dimensions.numnode);
            Marshal.Copy(grid.Vertices.Select(v => v.CoordinateValue.Z).ToArray(), 0, data.nodez, dimensions.numnode);

        }
    }

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

