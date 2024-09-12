using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Class containing extensions for converting hydro objects (like HydroNetwork, UnstructuredGrid etc.)
    /// to/from Deltares.UGrid objects
    /// </summary>
    public static class HydroUGridExtensions
    {   
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
            UGridFileHelperMesh2D.SetMesh2DGeometry(grid, meshGeometry, recreateCells);
            return grid;
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

        private static void SetNodeArrays(this Disposable2DMeshGeometry mesh, IList<Coordinate> coordinates)
        {
            mesh.NodesX = coordinates.Select(c => c.X).ToArray();
            mesh.NodesY = coordinates.Select(c => c.Y).ToArray();
        }

        private static void SetEdgeArrays(this Disposable2DMeshGeometry mesh, IList<Edge> gridEdges)
        {
            mesh.EdgeNodes = gridEdges.SelectMany(e => new[] { e.VertexFromIndex, e.VertexToIndex}).ToArray();
        }

        private static void SetCellArrays(this Disposable2DMeshGeometry mesh, IList<Cell> gridCells)
        {
            mesh.MaxNumberOfFaceNodes = gridCells.Count > 0 ? gridCells.Max(c => c.VertexIndices.Length) : 0;

            mesh.FaceNodes = Enumerable.Repeat((int)UGridFile.DEFAULT_NO_DATA_VALUE, mesh.MaxNumberOfFaceNodes * gridCells.Count).ToArray();//USE Default (set in file)!
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
            UGridFileHelperMesh1D.SetMesh1DGeometry(grid, meshGeometry, network);
            return grid;
        }

        /// <summary>
        /// Creates a <see cref="Disposable1DMeshGeometry"/> based on the <paramref name="discretization"/>
        /// </summary>
        /// <param name="discretization">Discretization to base the mesh on</param>
        /// <param name="logHandler">The log handler to write log messages to. Defaults to <c>null</c>.</param>
        public static Disposable1DMeshGeometry CreateDisposable1DMeshGeometry(this IDiscretization discretization, ILogHandler logHandler = null)
        {
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
                    logHandler?.ReportWarning(
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
                    logHandler?.ReportWarning(
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
                mesh.BranchTypes[i] = (int)branch.GetBranchType();

                // lookup node index in nodes array
                if (branch is ISewerConnection connection)
                {
                    mesh.NodesTo[i] = nodeIndexLookup[connection.TargetCompartment ?? (object)connection.Target];
                    mesh.NodesFrom[i] = nodeIndexLookup[connection.SourceCompartment ?? (object)connection.Source];
                }
                else
                {
                    mesh.NodesTo[i] = nodeIndexLookup.ContainsKey(branch.Target) ? nodeIndexLookup[branch.Target] : nodeIndexLookup.SingleOrDefault(nlu => nlu.Key is INameable keyName && keyName.Name.Equals(branch.Target.Name, StringComparison.InvariantCultureIgnoreCase)).Value;
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
        #endregion

        #region Links
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
                linksGeometry.LinkType[i] = (int)link1D2D.TypeOfLink;
                linksGeometry.Mesh1DFrom[i] = link1D2D.DiscretisationPointIndex;
                linksGeometry.Mesh2DTo[i] = link1D2D.FaceIndex;
            }

            return linksGeometry;
        }
        #endregion

        public static T[] GetBlockFromArray<T>(T[] fullArray, int blockSize, int blockIndex)
        {
            var objArray = new T[blockSize];
            var sourceIndex = blockIndex * blockSize;
            Array.Copy(fullArray, sourceIndex, objArray, 0, blockSize);
            return objArray;
        }
    }
}