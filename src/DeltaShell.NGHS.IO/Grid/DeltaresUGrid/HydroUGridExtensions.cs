using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using SharpMap.Api;
using SharpMap.Api.GridGeom;
using SharpMap.Data.Providers.EGIS.ShapeFileLib;
using Point = NetTopologySuite.Geometries.Point;

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
        /// <param name="recreateCells">If needed we need to recreate the cells because the cell index administration of grid geom (or kernel output) is different.</param>
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
        /// <param name="recreateCells">If needed we need to recreate the cells because the cell index administration of grid geom (or kernel output) is different.</param>
        private static void SetMesh2DGeometry(this UnstructuredGrid grid, Disposable2DMeshGeometry meshGeometry, bool recreateCells)
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
            bool canNodeArraysBeUsedForCoordinateList = mesh?.NodesX == null 
                                                        || mesh.NodesX.Length == 0 
                                                        || mesh.NodesY == null 
                                                        || mesh.NodesY.Length == 0 
                                                        || mesh.NodesX.Length != mesh.NodesY.Length;
            
            return canNodeArraysBeUsedForCoordinateList
                       ? new List<Coordinate>()
                       : mesh.NodesX.Select((t, i) => new Coordinate(t, mesh.NodesY[i])).ToList();
        }

        private static void SetNodeArrays(this Disposable2DMeshGeometry mesh, IList<Coordinate> coordinates)
        {
            mesh.NodesX = coordinates.Select(c => c.X).ToArray();
            mesh.NodesY = coordinates.Select(c => c.Y).ToArray();
        }

        private static IList<Edge> CreateEdges(this Disposable2DMeshGeometry mesh)
        {
            var edgeList = new ConcurrentDictionary<int, Edge>();
            if (mesh.EdgeNodes == null)
            {
                return edgeList.Values.ToList();
            }

            var numberOfEdges = (int) (mesh.EdgeNodes.Length / 2.0);
            Parallel.For(0, numberOfEdges, blockIndex => 
            {
                int[] blockFromArray = GetBlockFromArray(mesh.EdgeNodes, 2, blockIndex);
                edgeList.AddOrUpdate(blockIndex, new Edge(blockFromArray[0], blockFromArray[1]), (i, edge) => edge);
            });
            return edgeList.Values.ToList();
        }

        private static void SetEdgeArrays(this Disposable2DMeshGeometry mesh, IList<Edge> gridEdges)
        {
            mesh.EdgeNodes = gridEdges.SelectMany(e => new[] { e.VertexFromIndex, e.VertexToIndex}).ToArray();
        }

        private static IList<Cell> CreateCells(this Disposable2DMeshGeometry mesh, int fillValueMesh2DFaceNodes = (int)UGridFileHelper.DefaultNoDataValue)
        {
            var cellList = new ConcurrentDictionary<int,Cell>();
            if (mesh?.FaceNodes == null || 
                mesh.FaceX == null || 
                mesh.FaceY== null || 
                mesh.MaxNumberOfFaceNodes == 0)
            {
                return cellList.Values.ToList();
            }

            var numberOfFaces = mesh.FaceX.Length;
            Parallel.For(0, numberOfFaces, blockIndex =>
            {
                int[] blockFromArray = GetBlockFromArray(mesh.FaceNodes, mesh.MaxNumberOfFaceNodes, blockIndex);
                cellList.AddOrUpdate(blockIndex, new Cell(blockFromArray.Where(j => j != fillValueMesh2DFaceNodes).ToArray())
                {
                    CenterX = mesh.FaceX[blockIndex],
                    CenterY = mesh.FaceY[blockIndex]
                }, (i, cell) => cell);
            });
            return cellList.Values.ToList();
        }

        private static void SetCellArrays(this Disposable2DMeshGeometry mesh, IList<Cell> gridCells)
        {
            mesh.MaxNumberOfFaceNodes = gridCells.Count > 0 ? gridCells.Max(c => c.VertexIndices.Length) : 0;

            mesh.FaceNodes = Enumerable.Repeat((int)UGridFileHelper.DefaultNoDataValue, mesh.MaxNumberOfFaceNodes * gridCells.Count).ToArray();//USE Default (set in file)!
            mesh.FaceX = gridCells.Select(c => c.CenterX).ToArray();
            mesh.FaceY = gridCells.Select(c => c.CenterY).ToArray();

            for (var i = 0; i < gridCells.Count; i++)
            {
                var offset = i * mesh.MaxNumberOfFaceNodes;

                var cell = gridCells[i];
                for (int j = 0; j < cell.VertexIndices.Length; j++)
                {
                    mesh.FaceNodes[offset + j] = cell.VertexIndices[j];
                }
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
            var networkLocations = new ConcurrentQueue<INetworkLocation>();
            var networkLocationImportErrors = new ConcurrentQueue<string>();
            const string indexOfVerticeInTheFile = "fileIndex";
            
            Parallel.For(0, numberOfNodes, i =>
            {
                var networkBranch = network.Branches[meshGeometry.BranchIDs[i]];
                var meshGeometryBranchChainage = meshGeometry.BranchOffsets[i];

                double chainage = Math.Abs(networkBranch.Length - meshGeometryBranchChainage) < 0.00001 ? networkBranch.Length : meshGeometryBranchChainage;
                if (chainage < 0)
                {
                    networkLocationImportErrors.Enqueue(string.Format(Resources.HydroUGridExtensions_Negative_chainage_of_network_location, networkBranch.Name));
                    return;
                }

                if (chainage > networkBranch.Length)
                {
                    networkLocationImportErrors.Enqueue(string.Format(Resources.HydroUGridExtensions_Chainage_of_network_location_too_large,
                                                                      meshGeometryBranchChainage, networkBranch.Name, networkBranch.Length));

                    chainage = networkBranch.Length;
                }
                
                var networkLocation = new NetworkLocation()
                {
                    Branch = networkBranch,
                    Chainage = chainage,
                    Name = meshGeometry.NodeIds[i],
                    LongName = meshGeometry.NodeLongNames[i],
                    Geometry = canUseXYForMesh1DNodeCoordinates
                                   ? new Point(meshGeometry.NodesX[i], meshGeometry.NodesY[i])
                                   : HydroNetworkHelper.GetStructureGeometry(networkBranch, networkBranch.Length - meshGeometryBranchChainage < 0.000001 ? networkBranch.Length : meshGeometryBranchChainage)
                };
                networkLocation.Attributes[indexOfVerticeInTheFile] = i;
                networkLocations.Enqueue(networkLocation);
            });

            if (networkLocationImportErrors.Any())
            {
                Log.Error(string.Format(Resources.HydroUGridExtensions_GetNetworkLocations_While_reading_1d_discretization___calculation_point_from_the_netfile_we_encountered_the_following_errors___0__1_, Environment.NewLine, string.Join(Environment.NewLine, networkLocationImportErrors)));
            }
            return networkLocations.Distinct().OrderBy(nl => nl.Attributes[indexOfVerticeInTheFile]);
        }

        /// <summary>
        /// Sets the <paramref name="meshGeometry"/> to the <paramref name="discretization"/>
        /// </summary>
        /// <param name="discretization">Discretization to set</param>
        /// <param name="meshGeometry">Mesh geometry to set</param>
        /// <param name="network">Network that the <paramref name="meshGeometry"/> is based on</param>
        public static void SetMesh1DGeometry(this IDiscretization discretization, Disposable1DMeshGeometry meshGeometry, IHydroNetwork network, bool canUseXYForMesh1DNodeCoordinates = true)
        {
            var logHandler = new LogHandler(Resources.HydroUGridExtensions_Mesh1DGeometryLogHandlerActivityName, typeof(HydroUGridExtensions), 100);
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
            var logHandler = new LogHandler(Resources.HydroUGridExtensions_Mesh1DGeometryLogHandlerActivityName, typeof(HydroUGridExtensions), 100);
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

                mesh.NodesX[i] = location.Geometry?.Coordinate.X ?? 0;
                mesh.NodesY[i] = location.Geometry?.Coordinate.Y ?? 0;
                mesh.BranchIDs[i] = branchIdLookup[location.Branch];
                mesh.BranchOffsets[i] = location.Branch.GetBranchSnappedChainage(location.Chainage);
                mesh.NodeIds[i] = location.Name;
                mesh.NodeLongNames[i] = location.LongName ?? "";
            }

            var edgeNodeIndex = 0;
            for (int i = 0; i < edgeCount; i++)
            {
                var segment = segments[i];

                mesh.EdgeBranchIds[i] = branchIdLookup[segment.Branch];
                mesh.EdgeCenterPointOffset[i] = segment.Chainage + segment.Length / 2.0;
                mesh.EdgeCenterPointX[i] = segment.Geometry.Centroid.X;
                mesh.EdgeCenterPointY[i] = segment.Geometry.Centroid.Y;
                
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
                    logHandler.ReportWarning(
                        string.Format(Resources.HydroUGridExtensions_Cannot_find_start_edge_node_of_section, 
                                      segment.SegmentNumber, segment.Branch.Name, segment.Chainage, segment.Branch.Name));
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
                    logHandler.ReportWarning(
                        string.Format(Resources.HydroUGridExtensions_Cannot_find_end_edge_node_of_section, 
                                      segment.SegmentNumber, segment.Branch.Name, segment.EndChainage, segment.Branch.Name));
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
        /// <param name="forceCustomLengths">Force all branches in the network to have custom lengths and use the lengths that are read from file</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the node or compartment names are not unique.</exception>
        public static void SetNetworkGeometry(this IHydroNetwork network,
                                              DisposableNetworkGeometry networkGeometry,
                                              IEnumerable<BranchProperties> branchProperties,
                                              IEnumerable<CompartmentProperties> compartmentProperties,
                                              bool forceCustomLengths = false)
        {
            Ensure.NotNull(network, nameof(network));
            Ensure.NotNull(networkGeometry, nameof(networkGeometry));
            Ensure.NotNull(branchProperties, nameof(branchProperties));
            Ensure.NotNull(compartmentProperties, nameof(compartmentProperties));

            INode[] nodes = CreateNetworkNodes(network, networkGeometry, compartmentProperties);

            IReadOnlyDictionary<string, INode> nodeLookup = CreateNodeLookup(nodes);
            IReadOnlyDictionary<string, ICompartment> compartmentLookup = CreateCompartmentLookup(nodes);

            IEnumerable<IBranch> branches = CreateBranches(networkGeometry, branchProperties, forceCustomLengths, nodeLookup, compartmentLookup);

            network.Nodes.AddRange(nodes.Distinct());
            network.Branches.AddRange(branches);
        }

        private static IReadOnlyDictionary<string, ICompartment> CreateCompartmentLookup(IEnumerable<INode> nodes)
        {
            return nodes
                   .OfType<IManhole>()
                   .SelectMany(m => m.Compartments)
                   .ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_NetworkCompartmentNamesContext, c => c.Name);
        }

        private static IReadOnlyDictionary<string, INode> CreateNodeLookup(IEnumerable<INode> nodes)
        {
            return nodes
                   .SelectMany(n => GetNodeLookupNames(n)
                                   .Select(name => new
                                   {
                                       key = name,
                                       node = n
                                   })).ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_NetworkNodesContext,
                                                                    n => n.key,
                                                                    n => n.node);
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
                mesh.BranchLengths[i] = branch.Length;
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
                    geometryXData.Add(coordinate.X);
                    geometryYData.Add(coordinate.Y);
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

                        mesh.NodesX[addedNodeIndex] = compartmentX;
                        mesh.NodesY[addedNodeIndex] = manhole.Geometry.Coordinate.Y;
                        mesh.NodeIds[addedNodeIndex] = compartment.Name;
                        mesh.NodeLongNames[addedNodeIndex] = manhole.LongName ?? "";

                        nodeIndexLookup.Add(compartment, addedNodeIndex);
                        addedNodeIndex += 1;
                    }
                    continue;
                }

                mesh.NodesX[addedNodeIndex] = node.Geometry.Coordinate.X;
                mesh.NodesY[addedNodeIndex] = node.Geometry.Coordinate.Y;
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

        private static IEnumerable<IBranch> CreateBranches(DisposableNetworkGeometry networkGeometry, 
                                                           IEnumerable<BranchProperties> branchProperties, 
                                                           bool forceCustomLengths, 
                                                           IReadOnlyDictionary<string, INode> nodeLookup, 
                                                           IReadOnlyDictionary<string, ICompartment> compartmentLookup)
        {
            var propertiesLookup = branchProperties?.ToDictionary(p => p.Name) ?? new Dictionary<string, BranchProperties>();

            var branches = new List<IBranch>();

            var geometryOffset = 0;

            for (int i = 0; i < networkGeometry.BranchIds.Length; i++)
            {
                propertiesLookup.TryGetValue(networkGeometry.BranchIds[i], out var properties);
                branches.Add(CreateBranchByIndex(networkGeometry, i, properties, nodeLookup, compartmentLookup, ref geometryOffset, forceCustomLengths));
            }
            
            return branches.ToArray();
        }

        private static IBranch CreateBranchByIndex(DisposableNetworkGeometry networkGeometry, int branchIndex, BranchProperties branchProperties, IReadOnlyDictionary<string, INode> nodeLookup, IReadOnlyDictionary<string, ICompartment> compartments, ref int geometryOffset, bool forceCustomLengths)
        {
            var toNodeId = networkGeometry.NodeIds[networkGeometry.NodesTo[branchIndex]];
            var fromNodeId = networkGeometry.NodeIds[networkGeometry.NodesFrom[branchIndex]];
            var branchId = networkGeometry.BranchIds[branchIndex];

            INode source = nodeLookup[fromNodeId];
            INode target = nodeLookup[toNodeId];
            var branch = GetBranch(branchProperties, source, target);

            branch.Name = branchId;
            branch.OrderNumber = networkGeometry.BranchOrder[branchIndex];

            var nodeCount = networkGeometry.BranchGeometryNodesCount[branchIndex];
            branch.Geometry = GetLineStringForBranch(networkGeometry, nodeCount, geometryOffset);
            geometryOffset += nodeCount;

            ((IHydroNetworkFeature)branch).LongName = networkGeometry.BranchLongNames[branchIndex];

            if (forceCustomLengths || (branchProperties?.IsCustomLength ?? false))
            {
                branch.Length = 0;
                branch.Length = networkGeometry.BranchLengths[branchIndex];
                branch.IsLengthCustom = true;
            }

            if (branch is IPipe pipe)
            {
                pipe.WaterType = ToPipeWaterType((BranchType)networkGeometry.BranchTypes[branchIndex]);
            }

            if (branch is ISewerConnection sewerConnection)
            {
                // compartment can be null when coupled to rural network
                sewerConnection.SourceCompartment = compartments.ContainsKey(fromNodeId) ? compartments[fromNodeId] : null;
                sewerConnection.TargetCompartment = compartments.ContainsKey(toNodeId) ? compartments[toNodeId] : null;
            }

            return branch;
        }

        private static LineString GetLineStringForBranch(DisposableNetworkGeometry networkGeometry, int nodeCount, int geometryOffset)
        {
            var coordinates = new Coordinate[nodeCount];

            for (int j = 0; j < nodeCount; j++)
            {
                var geometryIndex = j + geometryOffset;
                coordinates[j] = new Coordinate(networkGeometry.BranchGeometryX[geometryIndex], networkGeometry.BranchGeometryY[geometryIndex]);
            }

            return new LineString(coordinates);
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

        private static IBranch GetBranch(BranchProperties branchProperties, INode source, INode target)
        {
            if (branchProperties == null)
            {
                if (source is Manhole || target is Manhole)
                {
                    return new Pipe{Source = source, Target = target};
                }

                return new Channel(source, target);
            }

            var branchType = branchProperties.BranchType;

            IBranch branch;
            switch (branchType)
            {
                case BranchFile.BranchType.SewerConnection:
                    branch = new SewerConnection { Source = source, Target = target, WaterType = branchProperties.WaterType };
                    break;
                case BranchFile.BranchType.Pipe:
                    branch = new Pipe { Source = source, Target = target, WaterType = branchProperties.WaterType, Material = branchProperties.Material };
                    break;
                default:
                    branch = new Channel(source, target);
                    break;
            }

            return branch;
        }

        private static INode[] CreateNetworkNodes(IHydroNetwork network, 
                                                  DisposableNetworkGeometry networkGeometry, 
                                                  IEnumerable<CompartmentProperties> compartmentProperties = null)
        {
            Dictionary<string, CompartmentProperties> compartmentPropertiesLookup = CreateCompartmentPropertiesLookup(compartmentProperties);
            Dictionary<string, IManhole> manHoleLookup = CreateManholeLookup(network);
            
            var nodes = new List<INode>();
            var manholesToFix = new List<IManhole>();

            int nodeCount = networkGeometry.NodesX.Length;
            for (var i = 0; i < nodeCount; i++)
            {
                INode node;

                string nodeName = networkGeometry.NodeIds[i];
                bool isCompartment = compartmentPropertiesLookup.ContainsKey(nodeName);

                if (isCompartment)
                {
                    CompartmentProperties properties = compartmentPropertiesLookup[nodeName];
                    Compartment compartment = CreateCompartment(nodeName, properties);

                    if (CompartmentIsInExistingManhole(manHoleLookup, properties.ManholeId))
                    {
                        IManhole existingManhole = manHoleLookup[properties.ManholeId];

                        existingManhole.Compartments.Add(compartment);
                        manholesToFix.Add(existingManhole);

                        continue;
                    }

                    node = CreateManhole(networkGeometry, i, properties, compartment);
                    manHoleLookup[node.Name] = (IManhole)node;
                }
                else
                {
                    node = CreateHydroNode(networkGeometry, i, nodeName);
                }

                ((IHydroNetworkFeature)node).LongName = GetNodeLongName(networkGeometry, i);

                node.Network = network;

                nodes.Add(node);
            }
            
            foreach (IManhole manhole in manholesToFix)
            {
                FixManholeGeometry(manhole);
            }

            return nodes.ToArray();
        }

        private static Dictionary<string, CompartmentProperties> CreateCompartmentPropertiesLookup(IEnumerable<CompartmentProperties> compartmentProperties)
        {
            Dictionary<string, CompartmentProperties> lookup = compartmentProperties?
                .ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_CompartmentIdContext, p => p.CompartmentId);

            return lookup ?? new Dictionary<string, CompartmentProperties>();
        }
        
        private static Dictionary<string, IManhole> CreateManholeLookup(IHydroNetwork network)
        {
            return network.Manholes.ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_ManholeNamesContext, m => m.Name);
        }
        
        private static Compartment CreateCompartment(string nodeName, CompartmentProperties properties)
        {
            double manholeWidth = Math.Sqrt(properties.Area);
            
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
                AddTablePropertiesToCompartment(compartment, properties);
            }

            return compartment;
        }
        
        private static void AddTablePropertiesToCompartment(ICompartment compartment, CompartmentProperties properties)
        {
            compartment.UseTable = true;
            compartment.Storage.Arguments[0].InterpolationType = properties.Interpolation;
            compartment.Storage.Arguments[0].SetValues(properties.Levels);
            compartment.Storage.Components[0].SetValues(properties.StorageAreas);
        }

        private static bool CompartmentIsInExistingManhole(
            IReadOnlyDictionary<string, IManhole> manHoleLookup, 
            string manholeId)
        {
            return manHoleLookup.ContainsKey(manholeId);
        }
        
        private static Manhole CreateManhole(DisposableNetworkGeometry networkGeometry, 
                                             int nodeIndex, 
                                             CompartmentProperties properties, 
                                             ICompartment compartment)
        {
            return new Manhole(properties.ManholeId)
            {
                Compartments = new EventedList<ICompartment> { compartment },
                Geometry = new Point(networkGeometry.NodesX[nodeIndex], networkGeometry.NodesY[nodeIndex])
            };
        }

        private static HydroNode CreateHydroNode(DisposableNetworkGeometry networkGeometry, int nodeIndex, string nodeName)
        {
            return new HydroNode
            {
                Name = nodeName == "" ? null : nodeName,
                Geometry = new Point(networkGeometry.NodesX[nodeIndex], networkGeometry.NodesY[nodeIndex])
            };
        }

        private static string GetNodeLongName(DisposableNetworkGeometry networkGeometry, int nodeIndex)
        {
            return networkGeometry.NodeLongNames[nodeIndex] != ""
                       ? networkGeometry.NodeLongNames[nodeIndex]
                       : null;
        }

        private static void FixManholeGeometry(IManhole manhole)
        {
            Coordinate firstCompartmentLocation = manhole.Geometry.Coordinate;
            double offset = (manhole.Compartments.Count - 1) / 2.0;
            manhole.Geometry = new Point(firstCompartmentLocation.X + offset, firstCompartmentLocation.Y);
        }

        #endregion

        #region Links

        /// <summary>
        /// Sets a list of <see cref="ILink1D2D"/> from the provided <paramref name="generatedObjectsForLinks"/> onto the <paramref name="link1D2Ds"/>
        /// </summary>
        /// <param name="link1D2Ds">A list of <see cref="ILink1D2D"/> to add the new links to</param>
        /// <param name="generatedObjectsForLinks">All the objects needed to generate 1d2d links</param>
        /// <returns>A list of <see cref="ILink1D2D"/> objects based on the provided <see cref="DisposableLinksGeometry"/></returns>
        public static void SetLinks(this IList<ILink1D2D> link1D2Ds, GeneratedObjectsForLinks generatedObjectsForLinks)
        {
            link1D2Ds.Clear();
            
            var links = new ConcurrentQueue<ILink1D2D>();
            
            QuadTree treeDiscretizationPoints = GenerateQuadTreeOfDiscretizationPoints(generatedObjectsForLinks.Discretization);

            double[] mesh1dNodesX = generatedObjectsForLinks.Mesh1d?.NodesX;
            double[] mesh1dNodesY = generatedObjectsForLinks.Mesh1d?.NodesY;
            bool valid1DMeshNodeXyCoordinatesInFile = mesh1dNodesX != null
                                                      && mesh1dNodesY != null
                                                      && Array.TrueForAll(mesh1dNodesX, nodeX => !nodeX.Equals(0.0))
                                                      && Array.TrueForAll(mesh1dNodesY, nodeY => !nodeY.Equals(0.0)); 

            Dictionary<string, IBranch> branchesLookup = generatedObjectsForLinks.Discretization?.Network?.Branches?.ToDictionary(b => b.Name);

            var discretizationUGrid1D2DSearchIndexObject = new UGrid1D2DSearchIndexObject()
            {
                BranchesLookup = branchesLookup,
                QuadTree = treeDiscretizationPoints,
                ObjectInModel = generatedObjectsForLinks.Discretization,
                ValidXyInFile = valid1DMeshNodeXyCoordinatesInFile,
                MeshFromFile = generatedObjectsForLinks.Mesh1d
            }; 
            
            QuadTree treeCells = GenerateQuadTreeOfUnstructuredGridCells(generatedObjectsForLinks.Grid);

            double[] mesh2dFaceX = generatedObjectsForLinks.Mesh2d?.FaceX;
            double[] mesh2dFaceY = generatedObjectsForLinks.Mesh2d?.FaceY;
            bool valid2DMeshFaceXyCoordinatesInFile = mesh2dFaceX != null 
                                                      && mesh2dFaceY != null 
                                                      && Array.TrueForAll(mesh2dFaceX, faceX => !faceX.Equals(0.0)) 
                                                      && Array.TrueForAll(mesh2dFaceY, faceY => !faceY.Equals(0.0));

            var cellUGrid1D2DSearchIndexObject = new UGrid1D2DSearchIndexObject()
            {
                QuadTree = treeCells,
                ObjectInModel = generatedObjectsForLinks.Grid,
                ValidXyInFile = valid2DMeshFaceXyCoordinatesInFile,
                MeshFromFile = generatedObjectsForLinks.Mesh2d
            };
            

            Parallel.For(0, generatedObjectsForLinks.LinksGeometry.LinkId.Length, indexOfLinkInFile =>
            {
                //From:
                int calcPointIdx = GetCalcPointIdx(generatedObjectsForLinks.LinksGeometry.Mesh1DFrom[indexOfLinkInFile], discretizationUGrid1D2DSearchIndexObject, generatedObjectsForLinks.NetworkGeometry, out Coordinate nodeCoordinateFromFile);

                //To:
                int cellIdx = GetCellIdx(generatedObjectsForLinks.LinksGeometry.Mesh2DTo[indexOfLinkInFile], cellUGrid1D2DSearchIndexObject, generatedObjectsForLinks.FillValueMesh2DFaceNodes, out Coordinate faceCoordinateFromFile);

                links.Enqueue(new Link1D2D(calcPointIdx, cellIdx, generatedObjectsForLinks.LinksGeometry.LinkId[indexOfLinkInFile])
                {
                    Geometry = new LineString(new[] { nodeCoordinateFromFile, faceCoordinateFromFile }), 
                    LongName = generatedObjectsForLinks.LinksGeometry.LinkLongName[indexOfLinkInFile],
                    TypeOfLink = (LinkStorageType)generatedObjectsForLinks.LinksGeometry.LinkType[indexOfLinkInFile],
                    Link1D2DIndex = indexOfLinkInFile
                });
            });
            link1D2Ds.AddRange(links.OrderBy(l => l.Link1D2DIndex));
        }

        private struct UGrid1D2DSearchIndexObject
        {
            public IUnique<long> ObjectInModel { get; set; }
            public QuadTree QuadTree { get; set; }
            public Dictionary<string, IBranch> BranchesLookup { get; set; }
            public bool ValidXyInFile { get; set; }
            public DisposableMeshObject MeshFromFile { get; set; }
        }
        /// <summary>
        /// This will create a QuadTree of the indices of the discretization points
        /// with the depth of the tree determined my the max levels
        /// which is dynamically sized by the amount of discretization points
        /// </summary>
        /// <param name="discretization"><see cref="IDiscretization"/>Contains calculation points for the DOM.</param>
        /// <returns>QuadTree</returns>
        private static QuadTree GenerateQuadTreeOfDiscretizationPoints(IDiscretization discretization)
        {
            QuadTree treeDiscretizationPoints = null;
            if (discretization != null)
            {
                // see BuildQuadTree in Layer.cs of Framework which was used as base for this method
                var maxLevels = (int)Math.Ceiling(0.4 * Math.Log(discretization.Locations.Values.Count, 2));
                Envelope envelope = discretization.Geometry.EnvelopeInternal;
                envelope.ExpandBy(50, 50);
                treeDiscretizationPoints = new QuadTree(ToRectangleD(envelope), maxLevels, true);
                discretization.Locations.Values.AsParallel().ForEach((location, index) => treeDiscretizationPoints.Insert(index, ToRectangleD(location.Geometry.EnvelopeInternal)));
            }

            return treeDiscretizationPoints;
        }

        /// <summary>
        /// This will create a QuadTree of the indices of the UnstructuredGrid cells
        /// with the depth of the tree determined my the max levels
        /// which is dynamically sized by the amount of the UnstructuredGrid cells
        /// </summary>
        /// <param name="grid"><see cref="UnstructuredGrid"/>Contains cells (and indices) for the DOM.</param>
        /// <returns>QuadTree of the indices of the UnstructuredGrid Cells</returns>
        private static QuadTree GenerateQuadTreeOfUnstructuredGridCells(UnstructuredGrid grid)
        {
            Envelope envelope = grid.GetExtents();
            QuadTree treeCells = null;
            if (grid != null)
            {
                // see BuildQuadTree in Layer.cs of Framework which was used as base for this method
                var maxLevels = (int)Math.Ceiling(0.4 * Math.Log(grid.Cells.Count, 2));
                treeCells = new QuadTree(ToRectangleD(envelope), maxLevels, false);
                grid.Cells.AsParallel().ForEach((cell, index) => treeCells.Insert(index, ToRectangleD(cell.ToPolygon(grid).EnvelopeInternal)));
            }

            return treeCells;
        }

        private static int GetCalcPointIdx(int calcPointIdx, UGrid1D2DSearchIndexObject discretizationUGrid1D2DSearchIndexObject, DisposableNetworkGeometry networkGeometry, out Coordinate nodeCoordinateFromFile)
        {
            nodeCoordinateFromFile = new Coordinate(Coordinate.NullOrdinate, Coordinate.NullOrdinate);
            if (!(discretizationUGrid1D2DSearchIndexObject.MeshFromFile is Disposable1DMeshGeometry mesh1d))
            {
                return calcPointIdx;
            }
            double x = double.NaN;
            double y = double.NaN;
            bool validXyInFile = discretizationUGrid1D2DSearchIndexObject.ValidXyInFile
                                     && calcPointIdx < mesh1d.NodesX.Length
                                     && calcPointIdx < mesh1d.NodesY.Length;
            if (validXyInFile)
            {
                x = mesh1d.NodesX[calcPointIdx];
                y = mesh1d.NodesY[calcPointIdx];
            }
            else if (CanGenerateMesh1DCalculationPointGeometry(discretizationUGrid1D2DSearchIndexObject, networkGeometry, mesh1d, calcPointIdx))
            {
                Point geometry = GenerateMesh1DCalculationPointGeometry(discretizationUGrid1D2DSearchIndexObject.BranchesLookup, networkGeometry, mesh1d, calcPointIdx);
                x = geometry.Coordinate.X;
                y = geometry.Coordinate.Y;
            }

            if (double.IsNaN(x) || double.IsNaN(y) || !(discretizationUGrid1D2DSearchIndexObject.ObjectInModel is IDiscretization discretization))
            {
                return calcPointIdx; // use calcPointIdx from 1d2d link administration of file
            }

            // Find calcPointIdx from provided discretization with the coordinates of the file
            nodeCoordinateFromFile = new Coordinate(x, y);
            var rectangleD = new RectangleD(x - 25, y - 25, 50, 50); //empirically chosen by Ralph
            var calcPointIdxs = discretizationUGrid1D2DSearchIndexObject.QuadTree.GetIndices(ref rectangleD, 0.9f);
            calcPointIdx = GetIndexNearestToCoordinate(nodeCoordinateFromFile, calcPointIdxs, discretization, calcPointIdx);

            return calcPointIdx;
        }

        private static bool CanGenerateMesh1DCalculationPointGeometry(UGrid1D2DSearchIndexObject discretizationUGrid1D2DSearchIndexObject, DisposableNetworkGeometry networkGeometry, Disposable1DMeshGeometry mesh1d, int calcPointIdx)
        {
            return networkGeometry != null
                   && mesh1d.BranchIDs != null
                   && mesh1d.BranchOffsets != null
                   && calcPointIdx < mesh1d.BranchIDs.Length
                   && calcPointIdx < mesh1d.BranchOffsets.Length
                   && discretizationUGrid1D2DSearchIndexObject.BranchesLookup != null;
        }

        private static int GetIndexNearestToCoordinate(Coordinate nodeCoordinateFromFile, IEnumerable<int> calcPointIdxs, IDiscretization discretization, int calcPointIdx)
        {
            double distance = double.MaxValue;
            foreach (var idx in calcPointIdxs)
            {
                Coordinate geometryCoordinate = discretization.Locations.Values[idx].Geometry.Coordinate;
                if (geometryCoordinate.Equals2D(nodeCoordinateFromFile))
                {
                    calcPointIdx = idx;
                    break;
                }

                double distanceFileNodeCoordinateToInRangeCalLocation = geometryCoordinate.Distance(nodeCoordinateFromFile);
                if (distanceFileNodeCoordinateToInRangeCalLocation < distance)
                {
                    distance = distanceFileNodeCoordinateToInRangeCalLocation;
                    calcPointIdx = idx;
                }
            }

            return calcPointIdx;
        }

        private static Point GenerateMesh1DCalculationPointGeometry(Dictionary<string, IBranch> branchesLookup, DisposableNetworkGeometry networkGeometry, Disposable1DMeshGeometry mesh1d, int calcPointIdx)
        {
            var branchId = networkGeometry.BranchIds[mesh1d.BranchIDs[calcPointIdx]];
            var chainage = mesh1d.BranchOffsets[calcPointIdx];

            Point geometry = null;
            if (branchesLookup.TryGetValue(branchId, out var branch))
            {
                var lengthIndexedLine = new LengthIndexedLine(branch.Geometry);
                double offset = branch.IsLengthCustom || !double.IsNaN(branch.GeodeticLength)
                                    ? BranchFeature.SnapChainage(branch.Geometry.Length, (branch.Geometry.Length / branch.Length) * chainage)
                                    : chainage;

                // always copy: ExtractPoint will give either a new coordinate or a reference to an existing object
                geometry = new Point(lengthIndexedLine.ExtractPoint(offset).Copy());
            }

            return geometry;
        }

        private static int GetCellIdx(int cellIdx, UGrid1D2DSearchIndexObject cellUGrid1D2DSearchIndexObject, int fillValueMesh2DFaceNodes, out Coordinate faceCoordinateFromFile)
        {
            faceCoordinateFromFile = new Coordinate(Coordinate.NullOrdinate, Coordinate.NullOrdinate); 
            if (!(cellUGrid1D2DSearchIndexObject.MeshFromFile is Disposable2DMeshGeometry mesh2d))
            {
                return cellIdx;
            }

            // Using FaceX & FaceY is under the assumption the grid comes via our kernel team.
            // It is the mass center of the cell and the correct location to find the new cell
            // in the generated unstructured grid.

            /*
                If we cannot trust the source to have the mass center of the cell coordinates in the 2d mesh facex and facey coordinates we can use this but it is very slow
            */
            double x = double.NaN;
            double y = double.NaN;
            bool validXyInFile = cellUGrid1D2DSearchIndexObject.ValidXyInFile
                                     && cellIdx < mesh2d.FaceX.Length
                                     && cellIdx < mesh2d.FaceY.Length;
            if (validXyInFile)
            {
                x = mesh2d.FaceX[cellIdx];
                y = mesh2d.FaceY[cellIdx];
            }
            else if (CanGenerateMesh2DFaceCoordinateFromMesh2DVertices(cellIdx, mesh2d))
            {
                int[] verticesOfCellInFile = GetBlockFromArray(mesh2d.FaceNodes, mesh2d.MaxNumberOfFaceNodes, cellIdx);

                var coordinatesOfVerticesOfCellInFile = verticesOfCellInFile
                                                        .Where(verticesIndex => !verticesIndex.Equals((int)UGridFileHelper.DefaultNoDataValue)
                                                                                && !verticesIndex.Equals(int.MinValue) 
                                                                                && !verticesIndex.Equals(fillValueMesh2DFaceNodes)) // use fill value only! -999 is default of deltares / int.MinValue is default to other partner
                                                        .Select(verticesIndex => new Coordinate(mesh2d.NodesX[verticesIndex], mesh2d.NodesY[verticesIndex])).ToArray();

                var centroid = GetCentroid(coordinatesOfVerticesOfCellInFile);// converting to GeoApi object is very costly
                x = centroid.X; 
                y = centroid.Y;
                
            }

            if (double.IsNaN(x) || double.IsNaN(y) || !(cellUGrid1D2DSearchIndexObject.ObjectInModel is UnstructuredGrid grid))
            {
                return cellIdx; // use cellIdx from 1d2d link administration of file
            }
            
            // Find calcPointIdx from provided grid with the coordinates of the face in the file
            faceCoordinateFromFile = new Coordinate(x, y);
            var rectangleD = new RectangleD(x, y, 50, 50);
            var cellIdxs = cellUGrid1D2DSearchIndexObject.QuadTree.GetIndices(ref rectangleD, 0);

            cellIdx = GetCellIndexNearestToCoordinate(faceCoordinateFromFile, cellIdxs, grid, cellIdx);

            return cellIdx;
        }

        private static int GetCellIndexNearestToCoordinate(Coordinate faceCoordinateFromFile, IEnumerable<int> cellIdxs, UnstructuredGrid grid, int cellIdx)
        {
            double distance = double.MaxValue;
            foreach (var idx in cellIdxs)
            {
                if (grid.Cells[idx].Center.Equals2D(faceCoordinateFromFile))
                {
                    cellIdx = idx;
                    break;
                }

                double distanceFileFaceCoordinateToInRangeCell = grid.Cells[idx].Center.Distance(faceCoordinateFromFile);
                if (distanceFileFaceCoordinateToInRangeCell < distance)
                {
                    distance = distanceFileFaceCoordinateToInRangeCell;
                    cellIdx = idx;
                }
            }

            return cellIdx;
        }

        private static bool CanGenerateMesh2DFaceCoordinateFromMesh2DVertices(int cellIdx, Disposable2DMeshGeometry mesh2d)
        {
            return cellIdx < mesh2d.FaceNodes.Length / mesh2d.MaxNumberOfFaceNodes;
        }

        private static RectangleD ToRectangleD(Envelope envelope)
        {
            return new RectangleD(envelope.MinX, envelope.MinY, envelope.Width, envelope.Height);
        }

        private static Coordinate GetCentroid(Coordinate[] nodes)
        {
            double xSum = 0;
            double ySum = 0;
            int numberOfVertices = nodes.Length;

            for (var cellVertexIndex = 0; cellVertexIndex < numberOfVertices; cellVertexIndex++)
            {
                Coordinate coordinate = nodes[cellVertexIndex];
                xSum += coordinate.X;
                ySum += coordinate.Y;
            }
            return new Coordinate(xSum / numberOfVertices, ySum / numberOfVertices);
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