using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.GridGeom;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Class containing extensions for converting hydro objects (like HydroNetwork, UnstructuredGrid etc.)
    /// to/from Deltares.UGrid objects
    /// </summary>
    public static class HydroUGridExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(HydroUGridExtensions));

        #region Mesh2d

        /// <summary>
        /// Create a new <see cref="UnstructuredGrid"/> from <see cref="Disposable2DMeshGeometry"/>
        /// </summary>
        /// <param name="meshGeometry">Mesh geometry</param>
        /// <returns>Unstructured grid based on <see cref="meshGeometry"/></returns>
        public static UnstructuredGrid CreateUnstructuredGrid(this Disposable2DMeshGeometry meshGeometry, bool recreateCells)
        {
            var grid = new UnstructuredGrid();
            grid.SetMesh2DGeometry(meshGeometry, recreateCells);
            return grid;
        }

        /// <summary>
        /// Sets the <paramref name="meshGeometry"/> to the 2D <paramref name="grid"/>
        /// </summary>
        /// <param name="grid">Grid to reset</param>
        /// <param name="meshGeometry">Mesh geometry to use</param>
        public static void SetMesh2DGeometry(this UnstructuredGrid grid, Disposable2DMeshGeometry meshGeometry, bool recreateCells)
        {
            if (!grid.IsEmpty)
            {
                grid.Clear();
            }

            grid.Vertices = meshGeometry.CreateVertices();
            grid.Edges = meshGeometry.CreateEdges();
            if (recreateCells)
            {
                using (var api = new RemoteGridGeomApi())
                using (var mesh = new DisposableMeshGeometry(grid))
                {
                    DisposableMeshGeometry resultMesh = api.CreateCells(mesh);
                    grid.Cells = resultMesh.CreateCells();
                }
            }
            else
            {
                grid.Cells = meshGeometry.CreateCells();
            }
        }

        /// <summary>
        /// Creates a <see cref="Disposable2DMeshGeometry"/> based on the <paramref name="grid"/>
        /// </summary>
        /// <param name="grid">Grid to base the geometry on</param>
        /// <returns>The generated <see cref="Disposable2DMeshGeometry"/></returns>
        public static Disposable2DMeshGeometry CreateDisposable2DMeshGeometry(this UnstructuredGrid grid)
        {
            var mesh = new Disposable2DMeshGeometry();

            mesh.SetNodeArrays(grid.Vertices);
            mesh.SetEdgeArrays(grid.Edges);
            mesh.SetCellArrays(grid.Cells);

            return mesh;
        }

        private static IList<Coordinate> CreateVertices(this Disposable2DMeshGeometry mesh)
        {
            return mesh?.NodesX == null || mesh.NodesX.Length == 0
                ? new List<Coordinate>()
                : mesh.NodesX.Select((t, i) => new Coordinate(t, mesh.NodesY[i])).ToList();
        }

        private static void SetNodeArrays(this Disposable2DMeshGeometry mesh, IList<Coordinate> coordinates)
        {
            mesh.NodesX = new double[coordinates.Count];
            mesh.NodesY = new double[coordinates.Count];
            
            for (int index = 0; index < coordinates.Count; ++index)
            {
                var coordinate = coordinates[index];
                mesh.NodesX[index] = coordinate.X;
                mesh.NodesY[index] = coordinate.Y;
            }
        }

        private static IList<Edge> CreateEdges(this Disposable2DMeshGeometry mesh)
        {
            var edgeList = new List<Edge>();
            var numberOfEdges = mesh.EdgeNodes.Length / 2.0;
            for (int blockIndex = 0; blockIndex < numberOfEdges; ++blockIndex)
            {
                int[] blockFromArray = GetBlockFromArray(mesh.EdgeNodes, 2, blockIndex);
                edgeList.Add(new Edge(blockFromArray[0], blockFromArray[1]));
            }
            return edgeList;
        }

        private static void SetEdgeArrays(this Disposable2DMeshGeometry mesh, IList<Edge> gridEdges)
        {
            mesh.EdgeNodes = gridEdges.SelectMany(e => new[] { e.VertexFromIndex, e.VertexToIndex}).ToArray();
        }

        private static IList<Cell> CreateCells(this Disposable2DMeshGeometry mesh)
        {
            var cellList = new List<Cell>();
            if (mesh?.FaceNodes == null || 
                mesh.FaceX == null || 
                mesh.FaceY== null || 
                mesh.MaxNumberOfFaceNodes == 0)
            {
                return cellList;
            }

            var numberOfFaces = mesh.FaceX?.Length;

            for (int blockIndex = 0; blockIndex < numberOfFaces; ++blockIndex)
            {
                int[] blockFromArray = GetBlockFromArray(mesh.FaceNodes, mesh.MaxNumberOfFaceNodes, blockIndex);
                cellList.Add(new Cell(blockFromArray.Where(j => j != -999).ToArray())
                {
                    CenterX = mesh.FaceX[blockIndex],
                    CenterY = mesh.FaceY[blockIndex]
                });
            }
            return cellList;
        }

        private static void SetCellArrays(this Disposable2DMeshGeometry mesh, IList<Cell> gridCells)
        {
            mesh.MaxNumberOfFaceNodes = gridCells.Count > 0 ? gridCells.Max(c => c.VertexIndices.Length) : 0;

            mesh.FaceNodes = Enumerable.Repeat(-999, mesh.MaxNumberOfFaceNodes * gridCells.Count).ToArray();
            mesh.FaceX = new double[gridCells.Count];
            mesh.FaceY = new double[gridCells.Count];

            for (var i = 0; i < gridCells.Count; i++)
            {
                var offset = i * mesh.MaxNumberOfFaceNodes;

                var cell = gridCells[i];
                for (int j = 0; j < cell.VertexIndices.Length; j++)
                {
                    mesh.FaceNodes[offset + j] = cell.VertexIndices[j];
                }

                mesh.FaceX[i] = cell.CenterX;
                mesh.FaceY[i] = cell.CenterY;
            }
        }

        #endregion

        #region Mesh1D

        /// <summary>
        /// Creates a <see cref="IDiscretization"/> based on the <paramref name="network"/> and <paramref name="meshGeometry"/>
        /// </summary>
        /// <param name="meshGeometry">Contains the discretization data</param>
        /// <param name="network">Network that the discretization is based on</param>
        public static IDiscretization CreateDiscretization(this Disposable1DMeshGeometry meshGeometry, IHydroNetwork network)
        {
            var grid = new Discretization();
            grid.SetMesh1DGeometry(meshGeometry, network);
            return grid;
        }


        private static IEnumerable<INetworkLocation> GetNetworkLocations(Disposable1DMeshGeometry meshGeometry, IHydroNetwork network, bool canUseXYForMesh1DNodeCoordinates)
        {
            var numberOfNodes = meshGeometry.NodeIds.Length;
            for (int i = 0; i < numberOfNodes; i++)
            {
                var networkBranch = network.Branches[meshGeometry.BranchIDs[i]];
                var meshGeometryBranchChainage = meshGeometry.BranchOffsets[i];

                double chainage = Math.Abs(networkBranch.Length - meshGeometryBranchChainage) < 0.00001 ? networkBranch.Length : meshGeometryBranchChainage;
                if (chainage < 0)
                {
                    Log.Error($"The chainage of a network location on branch '{networkBranch.Name}' is negative. Location will be skipped.");
                    continue;
                }

                if (chainage > networkBranch.Length)
                {
                    Log.Error($"The chainage ({meshGeometryBranchChainage}) of a network location on branch '{networkBranch.Name}' is beyond the length of the branch ({networkBranch.Length}). Location will be corrected to branch length.");
                    chainage = networkBranch.Length;
                }
                
                yield return new NetworkLocation
                {
                    Branch = networkBranch,
                    Chainage = chainage,
                    Name = meshGeometry.NodeIds[i],
                    LongName = meshGeometry.NodeLongNames[i],
                    Geometry = canUseXYForMesh1DNodeCoordinates 
                                   ? new Point(meshGeometry.NodesX[i], meshGeometry.NodesY[i])
                                   : HydroNetworkHelper.GetStructureGeometry(networkBranch, networkBranch.Length - meshGeometryBranchChainage < 0.000001 ? networkBranch.Length : meshGeometryBranchChainage) 
                };
            }
        }
        /// <summary>
        /// Sets the <paramref name="meshGeometry"/> to the <paramref name="discretization"/>
        /// </summary>
        /// <param name="discretization">Discretization to set</param>
        /// <param name="meshGeometry">Mesh geometry to set</param>
        /// <param name="network">Network that the <paramref name="meshGeometry"/> is based on</param>
        public static void SetMesh1DGeometry(this IDiscretization discretization, Disposable1DMeshGeometry meshGeometry, IHydroNetwork network, bool canUseXYForMesh1DNodeCoordinates = true)
        {
            var logHandler = new LogHandler("the creation of the mesh 1d geometry", typeof(HydroUGridExtensions), 100);
            discretization.Network = network;
            
            IEnumerable<INetworkLocation> networkLocations = GetNetworkLocations(meshGeometry, network, canUseXYForMesh1DNodeCoordinates);

            discretization.UpdateNetworkLocations(networkLocations);

            IList<INetworkSegment> segmentToRemove = null;
            var locationIdLookup = discretization.Locations.AllValues.ToArray().ToIndexDictionary();

            foreach (var segment in discretization.Segments.Values)
            {
                GetLocationIndices(discretization, segment, locationIdLookup, logHandler, out segmentToRemove);
            }

            if (segmentToRemove != null)
            {
                foreach (var segment in segmentToRemove)
                {
                    discretization.Segments.Values.Remove(segment);
                }

            }
            
            logHandler.LogReport();
        }

        /// <summary>
        /// Creates a <see cref="Disposable1DMeshGeometry"/> based on the <paramref name="discretization"/>
        /// </summary>
        /// <param name="discretization">Discretization to base the mesh on</param>
        public static Disposable1DMeshGeometry CreateDisposable1DMeshGeometry(this IDiscretization discretization)
        {
            var logHandler = new LogHandler("the creation of the mesh 1d geometry", typeof(HydroUGridExtensions), 100);
            var locations = discretization.Locations.Values.ToArray();

            var segments = discretization.Segments.Values.ToList();
            var edgeCount = segments.Count;

            var locationIdLookup = locations.ToIndexDictionary();
            var locationIdxBySegment = new Dictionary<INetworkSegment, int[]>();
            IList<INetworkSegment> doNotWriteTheseSegments = null;
            for (int i = 0; i < edgeCount; i++)
            {
                var segment = segments[i];

                var indices = GetLocationIndices(discretization, segment, locationIdLookup, logHandler, out doNotWriteTheseSegments);
                locationIdxBySegment[segment] = indices;
            }

            //update because of missing and thus new points
            locations = discretization.Locations.Values.ToArray();
            var locationCount = locations.Length;
            if (doNotWriteTheseSegments != null)
            {
                foreach (var networkSegment in doNotWriteTheseSegments)
                {
                    if (segments.Contains(networkSegment))
                        segments.Remove(networkSegment);
                }
            }

            edgeCount = segments.Count;

            var mesh = new Disposable1DMeshGeometry
            {
                Name = "mesh1d",
                NodesX = new double[locationCount],
                NodesY = new double[locationCount],
                NodeIds = new string[locationCount],
                NodeLongNames = new string[locationCount],
                BranchIDs = new int[locationCount],
                BranchOffsets = new double[locationCount],

                EdgeBranchIds = new int[edgeCount],
                EdgeNodes = new int[edgeCount *2],
                EdgeCenterPointX = new double[edgeCount],
                EdgeCenterPointY = new double[edgeCount],
                EdgeCenterPointOffset = new double[edgeCount],
            };

            var branchIdLookup = discretization.Network.Branches.ToIndexDictionary();

            for (int i = 0; i < locationCount; i++)
            {
                var location = locations[i];

                mesh.NodesX[i] = location.Geometry?.Coordinate.X.TruncateByDigits() ?? 0;
                mesh.NodesY[i] = location.Geometry?.Coordinate.Y.TruncateByDigits() ?? 0;
                mesh.BranchIDs[i] = branchIdLookup[location.Branch];
                mesh.BranchOffsets[i] = location.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(location.Chainage);
                mesh.NodeIds[i] = location.Name;
                mesh.NodeLongNames[i] = location.LongName ?? "";
            }

            var edgeNodeIndex = 0;
            for (int i = 0; i < edgeCount; i++)
            {
                var segment = segments[i];

                mesh.EdgeBranchIds[i] = branchIdLookup[segment.Branch];
                mesh.EdgeCenterPointOffset[i] = segment.Chainage + segment.Length / 2.0;
                mesh.EdgeCenterPointX[i] = segment.Geometry.Centroid.X.TruncateByDigits();
                mesh.EdgeCenterPointY[i] = segment.Geometry.Centroid.Y.TruncateByDigits();
                
                mesh.EdgeNodes[edgeNodeIndex++] = locationIdxBySegment[segment][0];
                mesh.EdgeNodes[edgeNodeIndex++] = locationIdxBySegment[segment][1];
            }

            logHandler.LogReport();
            return mesh;
        }

        internal static int[] GetLocationIndices(IDiscretization discretization, INetworkSegment segment, IDictionary<INetworkLocation, int> locationIdLookup, ILogHandler logHandler, out IList<INetworkSegment> doNotWriteTheseSegments)
        {
            const double epsilonLocation = 1e-5;
            var branchLocations = discretization.GetLocationsForBranch(segment.Branch);
            doNotWriteTheseSegments = new List<INetworkSegment>();
            int[] indices = new int[]{-1,-1};

            for (int j = 0; j < branchLocations.Count; j++)
            {
                var loc = branchLocations[j];
                if (Math.Abs(loc.Chainage - segment.Chainage) < epsilonLocation)
                {
                    indices[0] = locationIdLookup[loc];
                    continue;
                }

                if (Math.Abs(loc.Chainage - segment.EndChainage) < epsilonLocation)
                {
                    indices[1] = locationIdLookup[loc];
                    break;
                }
            }

            if (indices[0] == -1)
            {
                // no begin point found, search neighboring branches
                var firstLocation = discretization.GetLocationForBranchNode(segment.Branch, BranchNodeType.Begin);
                if (firstLocation != null)
                    indices[0] = locationIdLookup[firstLocation];
                else
                {
                    logHandler.ReportWarning($"Cannot find start edge node of section {segment.SegmentNumber} on branch {segment.Branch.Name} at chainage {segment.Chainage}. Creating one on start node of branch{segment.Branch.Name} (probably because of wrong rounding during load).");
                    indices[0] = -1;
                    doNotWriteTheseSegments.Add(segment);
                }
            }

            if (indices[1] == -1)
            {
                // no end point found, search neighboring branches
                var firstLocation = discretization.GetLocationForBranchNode(segment.Branch, BranchNodeType.End);
                if (firstLocation != null)
                    indices[1] = locationIdLookup[firstLocation];
                else
                {
                    logHandler.ReportWarning($"Cannot find end edge node of section {segment.SegmentNumber} on branch {segment.Branch.Name} at chainage {segment.EndChainage}. Creating one on end node of branch{segment.Branch.Name} (probably because of wrong rounding during load).");
                    indices[1] = -1;
                    doNotWriteTheseSegments.Add(segment);
                }
            }
            return indices;
        }

        #endregion

        #region Network

        /// <summary>
        /// Sets the nodes and branches of the <paramref name="network"/> with the values of the <paramref name="networkGeometry"/>
        /// </summary>
        /// <param name="network">Network to set</param>
        /// <param name="networkGeometry"><see cref="DisposableNetworkGeometry"/> containing the network data</param>
        /// <param name="branchProperties">Additional branch properties</param>
        /// <param name="compartmentProperties">Additional compartment properties</param>
        public static void SetNetworkGeometry(this IHydroNetwork network, DisposableNetworkGeometry networkGeometry, IEnumerable<BranchProperties> branchProperties = null, ICollection<CompartmentProperties> compartmentProperties = null)
        {
            var nodes = CreateNetworkNodes(network, networkGeometry, compartmentProperties);
            var branches = CreateBranches(networkGeometry, nodes, branchProperties);

            network.Nodes.AddRange(nodes.Distinct());
            network.Branches.AddRange(branches);
        }

        /// <summary>
        /// Creates a <see cref="DisposableNetworkGeometry"/> based on the provided <paramref name="network"/>
        /// </summary>
        /// <param name="network">Network containing the data</param>
        /// <returns>A <see cref="DisposableNetworkGeometry"/> containing the network data</returns>
        public static DisposableNetworkGeometry CreateDisposableNetworkGeometry(this IHydroNetwork network)
        {
            var mesh = new DisposableNetworkGeometry
            {
                NetworkName = network.Name
            };

            var nodeIndexLookup = SetNodeDataArrays(mesh, network);
            SetBranchDataArrays(mesh, network, nodeIndexLookup);

            return mesh;
        }

        private static void SetBranchDataArrays(DisposableNetworkGeometry mesh, IHydroNetwork network, IDictionary<object, int> nodeIndexLookup)
        {
            var numberOfBranches = network.Branches.Count;

            mesh.BranchIds = new string[numberOfBranches];
            mesh.BranchLongNames = new string[numberOfBranches];
            mesh.NodesFrom = new int[numberOfBranches];
            mesh.NodesTo = new int[numberOfBranches];
            mesh.BranchGeometryNodesCount = new int[numberOfBranches];
            mesh.BranchLengths = new double[numberOfBranches];
            mesh.BranchOrder = new int[numberOfBranches];
            mesh.BranchTypes = new int[numberOfBranches];

            var geometryXData = new List<double>();
            var geometryYData = new List<double>();

            // add channels, pipes and sewer-connections as branches

            for (int i = 0; i < numberOfBranches; i++)
            {
                var branch = network.Branches[i];
                mesh.BranchIds[i] = branch.Name;
                mesh.BranchLongNames[i] = branch.Description ?? "";
                mesh.BranchGeometryNodesCount[i] = branch.Geometry.Coordinates.Length;
                mesh.BranchLengths[i] = branch.IsLengthCustom ? branch.Length : branch.Length.TruncateByDigits();
                mesh.BranchOrder[i] = branch.OrderNumber;
                mesh.BranchTypes[i] = (int) branch.GetBranchType();

                // lookup node index in nodes array
                if (branch is ISewerConnection connection)
                {
                    mesh.NodesTo[i] = nodeIndexLookup[connection.TargetCompartment ?? (object)connection.Target];
                    mesh.NodesFrom[i] = nodeIndexLookup[connection.SourceCompartment ?? (object)connection.Source]; 
                }
                else
                {
                    mesh.NodesTo[i] = nodeIndexLookup.ContainsKey(branch.Target) ? nodeIndexLookup[branch.Target] : nodeIndexLookup.SingleOrDefault(nlu=>nlu.Key is INameable keyName && keyName.Name.Equals(branch.Target.Name, StringComparison.InvariantCultureIgnoreCase)).Value;
                    mesh.NodesFrom[i] = nodeIndexLookup.ContainsKey(branch.Source) ? nodeIndexLookup[branch.Source] : nodeIndexLookup.SingleOrDefault(nlu => nlu.Key is INameable keyName && keyName.Name.Equals(branch.Source.Name, StringComparison.InvariantCultureIgnoreCase)).Value;  
                }

                var coordinates = branch.Geometry?.Coordinates;

                if (coordinates == null) continue;

                for (int j = 0; j < coordinates.Length; j++)
                {
                    var coordinate = coordinates[j];
                    geometryXData.Add(coordinate.X.TruncateByDigits());
                    geometryYData.Add(coordinate.Y.TruncateByDigits());
                }
            }

            mesh.BranchGeometryX = geometryXData.ToArray();
            mesh.BranchGeometryY = geometryYData.ToArray();
        }

        private static Dictionary<object, int> SetNodeDataArrays(DisposableNetworkGeometry mesh, IHydroNetwork network)
        {
            var compartmentsCount = network.Manholes.SelectMany(m => m.Compartments).Count();
            var hydroNodesCount = network.HydroNodes.Count();

            var nodeCount = compartmentsCount + hydroNodesCount; // compartments and hydro nodes

            mesh.NodesX = new double[nodeCount];
            mesh.NodesY = new double[nodeCount];
            mesh.NodeIds = new string[nodeCount];
            mesh.NodeLongNames = new string[nodeCount];

            // cache node indices
            var nodeIndexLookup = new Dictionary<object, int>();
            var addedNodeIndex = 0;

            for (int i = 0; i < network.Nodes.Count; i++)
            {
                var node = network.Nodes[i];
                if (node is IManhole manhole)
                {
                    for (int j = 0; j < manhole.Compartments.Count; j++)
                    {
                        var compartment = manhole.Compartments[j];

                        var offset = (manhole.Compartments.Count - 1) * 0.5;
                        var compartmentX = manhole.Geometry.Coordinate.X - offset + j;

                        mesh.NodesX[addedNodeIndex] = compartmentX.TruncateByDigits();
                        mesh.NodesY[addedNodeIndex] = manhole.Geometry.Coordinate.Y.TruncateByDigits();
                        mesh.NodeIds[addedNodeIndex] = compartment.Name;
                        mesh.NodeLongNames[addedNodeIndex] = manhole.LongName ?? "";

                        nodeIndexLookup.Add(compartment, addedNodeIndex);
                        addedNodeIndex += 1;
                    }
                    continue;
                }

                mesh.NodesX[addedNodeIndex] = node.Geometry.Coordinate.X.TruncateByDigits();
                mesh.NodesY[addedNodeIndex] = node.Geometry.Coordinate.Y.TruncateByDigits();
                mesh.NodeIds[addedNodeIndex] = node.Name;

                if (node is IHydroNode hydroNode)
                {
                    mesh.NodeLongNames[addedNodeIndex] = hydroNode.LongName ?? "";
                }

                nodeIndexLookup.Add(node, addedNodeIndex);
                addedNodeIndex += 1;
            }

            return nodeIndexLookup;
        }

        private static BranchType GetBranchType(this IBranch branch)
        {
            switch (branch)
            {
                case ISewerConnection s:
                    switch (s.WaterType)
                    {
                        case SewerConnectionWaterType.None:
                            return BranchType.TransportWater;
                        case SewerConnectionWaterType.StormWater:
                            return BranchType.StormWaterFlow;
                        case SewerConnectionWaterType.DryWater:
                            return BranchType.DryWeatherFlow;
                        case SewerConnectionWaterType.Combined:
                            return BranchType.MixedFlow;
                        default:
                            return BranchType.SurfaceWater;
                    }
                case null:
                    throw new ArgumentNullException(nameof(branch));
                default:
                    return BranchType.SurfaceWater;
            }

        }

        private static IBranch[] CreateBranches(DisposableNetworkGeometry networkGeometry, INode[] nodes, IEnumerable<BranchProperties> branchProperties)
        {
            var numberOfBranches = networkGeometry.BranchIds.Length;
            var nodeLookup = nodes
                .SelectMany(n => GetNodeLookupNames(n)
                    .Select(name => new {key = name, node = n}))
                .ToDictionary(n => n.key, m => m.node);

            var propertiesLookup = branchProperties?.ToDictionary(p => p.Name);
            var compartments = nodes.OfType<IManhole>().SelectMany(m => m.Compartments).ToDictionary(c => c.Name);

            var branches = new List<IBranch>();

            var geometryOffset = 0;

            for (int i = 0; i < numberOfBranches; i++)
            {
                var toNodeId = networkGeometry.NodeIds[networkGeometry.NodesTo[i]];
                var fromNodeId = networkGeometry.NodeIds[networkGeometry.NodesFrom[i]];

                // todo: find out what to do with branch type from file.
                // Currently the branch type from the properties is used
                INode source = nodeLookup[fromNodeId];
                INode target = nodeLookup[toNodeId];
                var branch = GetBranch(propertiesLookup, networkGeometry.BranchIds[i], source, target);

                branch.Source = source;
                branch.Target = target;
                branch.Name = networkGeometry.BranchIds[i];
                ((IHydroNetworkFeature)branch).LongName = networkGeometry.BranchLongNames[i];
                if (branch.IsLengthCustom)
                {
                    branch.Length = 0;
                    branch.Length = networkGeometry.BranchLengths[i];
                }

                branch.OrderNumber = networkGeometry.BranchOrder[i];

                if (branch is ISewerConnection sewerConnection)
                {
                    // compartment can be null when coupled to rural network
                    sewerConnection.SourceCompartment = compartments.ContainsKey(fromNodeId) ? compartments[fromNodeId] : null;
                    sewerConnection.TargetCompartment = compartments.ContainsKey(toNodeId) ? compartments[toNodeId] : null;
                }

                if (branch is IPipe pipe)
                {
                    pipe.WaterType = ToPipeWaterType((BranchType)networkGeometry.BranchTypes[i]);
                }
                var nodeCount = networkGeometry.BranchGeometryNodesCount[i];
                var coordinates = new Coordinate[nodeCount];

                for (int j = 0; j < nodeCount; j++)
                {
                    var geometryIndex = j + geometryOffset;
                    coordinates[j] = new Coordinate(networkGeometry.BranchGeometryX[geometryIndex], networkGeometry.BranchGeometryY[geometryIndex]);
                }

                geometryOffset += nodeCount;
                branch.Geometry = new LineString(coordinates);

                branches.Add(branch);
            }

            return branches.ToArray();
        }

        private static SewerConnectionWaterType ToPipeWaterType(BranchType networkGeometryBranchType)
        {
            switch (networkGeometryBranchType)
            {
                case BranchType.DryWeatherFlow:
                    return SewerConnectionWaterType.DryWater;
                case BranchType.StormWaterFlow:
                    return SewerConnectionWaterType.StormWater;
                case BranchType.MixedFlow:
                    return SewerConnectionWaterType.Combined;
            }
            return SewerConnectionWaterType.None;

        }

        private static IEnumerable<string> GetNodeLookupNames(INode node)
        {
            if (node is IManhole manhole)
            {
                foreach (var compartment in manhole.Compartments)
                {
                    yield return compartment.Name;
                }
                yield break;
            }

            yield return node.Name;
        }

        private static IBranch GetBranch(IReadOnlyDictionary<string, BranchProperties> propertiesLookup, string branchName, INode source, INode target)
        {
            if (propertiesLookup == null || !propertiesLookup.ContainsKey(branchName))
            {
                if (source is Manhole || target is Manhole)
                {
                    return new Pipe();
                }

                return new Channel();
            }

            var branchProperties = propertiesLookup[branchName];
            var branchType = branchProperties.BranchType;

            IBranch branch = null;
            
            switch (branchType)
            {
                case BranchFile.BranchType.SewerConnection:
                    branch = new SewerConnection { WaterType = branchProperties.WaterType };
                    break;
                case BranchFile.BranchType.Pipe:
                    branch = new Pipe { WaterType = branchProperties.WaterType, Material = branchProperties.Material };
                    break;
                default:
                    branch = new Channel();
                    break;
            }

            branch.IsLengthCustom = branchProperties.IsCustomLength;

            return branch;
        }

        private static INode[] CreateNetworkNodes(IHydroNetwork network, DisposableNetworkGeometry networkGeometry, ICollection<CompartmentProperties> compartmentProperties = null)
        {
            var propertiesLookup = compartmentProperties?.ToDictionary(p => p.CompartmentId) ??
                                   new Dictionary<string, CompartmentProperties>();

            var manHoleLookup = network.Manholes.ToDictionary(m => m.Name);
            var nodeCount = networkGeometry.NodesX.Length;

            var nodes = new List<INode>();
            var manholesToFix = new List<IManhole>();
            for (int i = 0; i < nodeCount; i++)
            {
                INode node;

                var nodeName = networkGeometry.NodeIds[i];
                var isCompartment = propertiesLookup.ContainsKey(nodeName);

                if (isCompartment)
                {
                    var properties = propertiesLookup[nodeName];
                    var manholeWidth = Math.Sqrt(properties.Area);

                    var compartment = new Compartment(nodeName)
                    {
                        BottomLevel = properties.BedLevel,
                        SurfaceLevel = properties.StreetLevel,
                        FloodableArea = properties.StreetStorageArea,
                        ManholeLength = manholeWidth,
                        ManholeWidth = manholeWidth,
                        Shape = properties.CompartmentShape,
                        CompartmentStorageType = properties.CompartmentStorageType
                    };
                    if (properties.UseTable)
                    {
                        compartment.UseTable = true;
                        compartment.Storage.Arguments[0].InterpolationType = properties.Interpolation;
                        compartment.Storage.Arguments[0].SetValues(properties.Levels);
                        compartment.Storage.Components[0].SetValues(properties.StorageAreas);
                    }

                    if (manHoleLookup.TryGetValue(properties.ManholeId, out var existingManhole))
                    {
                        existingManhole.Compartments.Add(compartment);
                        manholesToFix.Add(existingManhole);
                        continue;
                    }

                    node = new Manhole(properties.ManholeId)
                    {
                        Compartments = new EventedList<ICompartment> { compartment },
                        Geometry = new Point(networkGeometry.NodesX[i], networkGeometry.NodesY[i])
                    };

                    manHoleLookup[node.Name] = (IManhole)node;
                }
                else
                {
                    node = new HydroNode
                    {
                        Name = nodeName == "" ? null : nodeName,
                        Geometry = new Point(networkGeometry.NodesX[i], networkGeometry.NodesY[i])
                    };
                }

                ((IHydroNetworkFeature)node).LongName = networkGeometry.NodeLongNames[i] != ""
                    ? networkGeometry.NodeLongNames[i]
                    : null;
                
                node.Network = network;

                nodes.Add(node);
            }

            // fix manhole geometry
            foreach (var manhole in manholesToFix)
            {
                var firstCompartmentLocation = manhole.Geometry.Coordinate;
                var offset = (manhole.Compartments.Count - 1) / 2.0;
                manhole.Geometry = new Point(firstCompartmentLocation.X + offset, firstCompartmentLocation.Y);
            }

            return nodes.ToArray();
        }

        #endregion

        #region Links

        /// <summary>
        /// Creates a list of <see cref="ILink1D2D"/> from the provided <paramref name="linksGeometry"/>
        /// </summary>
        /// <param name="linksGeometry"><see cref="DisposableLinksGeometry"/> containing the link data</param>
        /// <returns>A list of <see cref="ILink1D2D"/> objects based on the provided <see cref="DisposableLinksGeometry"/></returns>
        public static IList<ILink1D2D> CreateLinks(this DisposableLinksGeometry linksGeometry)
        {
            var link1D2Ds = new List<ILink1D2D>();
            link1D2Ds.SetLinks(linksGeometry);
            return link1D2Ds;
        }

        /// <summary>
        /// Sets a list of <see cref="ILink1D2D"/> from the provided <paramref name="linksGeometry"/> onto the <paramref name="link1D2Ds"/>
        /// </summary>
        /// <param name="link1D2Ds">A list of <see cref="ILink1D2D"/> to add the new links to</param>
        /// <param name="linksGeometry"><see cref="DisposableLinksGeometry"/> containing the link data</param>
        /// <returns>A list of <see cref="ILink1D2D"/> objects based on the provided <see cref="DisposableLinksGeometry"/></returns>
        public static void SetLinks(this IList<ILink1D2D> link1D2Ds, DisposableLinksGeometry linksGeometry)
        {
            link1D2Ds.Clear();

            var numberOfLinks = linksGeometry.LinkId.Length;
            for (int i = 0; i < numberOfLinks; i++)
            {
                link1D2Ds.Add(new Link1D2D(linksGeometry.Mesh1DFrom[i], linksGeometry.Mesh2DTo[i], linksGeometry.LinkId[i])
                {
                    LongName = linksGeometry.LinkLongName[i],
                    TypeOfLink = (LinkStorageType) linksGeometry.LinkType[i]
                });
            }
        }

        /// <summary>
        /// Creates a <see cref="DisposableLinksGeometry"/> based on the set of <paramref name="links1D2D"/>
        /// </summary>
        /// <param name="links1D2D">Set of links to base the <see cref="DisposableLinksGeometry"/> on</param>
        /// <returns>A <see cref="DisposableLinksGeometry"/> based on the set of <paramref name="links1D2D"/></returns>
        public static DisposableLinksGeometry CreateDisposableLinksGeometry(this IList<ILink1D2D> links1D2D)
        {
            var numberOfLinks = links1D2D.Count;

            var linksGeometry = new DisposableLinksGeometry
            {
                LinkId = new string[numberOfLinks],
                LinkLongName = new string[numberOfLinks],
                LinkType = new int[numberOfLinks],
                Mesh1DFrom = new int[numberOfLinks],
                Mesh2DTo = new int[numberOfLinks]
            };

            for (int i = 0; i < numberOfLinks; i++)
            {
                var link1D2D = links1D2D[i];
                linksGeometry.LinkId[i] = link1D2D.Name;
                linksGeometry.LinkLongName[i] = link1D2D.LongName ?? "";
                linksGeometry.LinkType[i] = (int) link1D2D.TypeOfLink;
                linksGeometry.Mesh1DFrom[i] = link1D2D.DiscretisationPointIndex;
                linksGeometry.Mesh2DTo[i] = link1D2D.FaceIndex;
            }

            return linksGeometry;
        }

        #endregion

        private static T[] GetBlockFromArray<T>(T[] fullArray, int blockSize, int blockIndex)
        {
            var objArray = new T[blockSize];
            var sourceIndex = blockIndex * blockSize;
            Array.Copy(fullArray, sourceIndex, objArray, 0, blockSize);
            return objArray;
        }

        
    }
}