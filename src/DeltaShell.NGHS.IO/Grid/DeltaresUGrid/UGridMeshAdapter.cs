using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    public static class UGridMeshAdapter
    {
        #region Mesh2d

        /// <summary>
        /// Create a new <see cref="UnstructuredGrid"/> from <see cref="Disposable2DMeshGeometry"/>
        /// </summary>
        /// <param name="meshGeometry">Mesh geometry</param>
        /// <returns>Unstructured grid based on <see cref="meshGeometry"/></returns>
        public static UnstructuredGrid CreateUnstructuredGrid(this Disposable2DMeshGeometry meshGeometry)
        {
            var grid = new UnstructuredGrid();
            grid.SetMeshGeometry(meshGeometry);
            return grid;
        }

        /// <summary>
        /// Sets the <paramref name="meshGeometry"/> to the 2D <paramref name="grid"/>
        /// </summary>
        /// <param name="grid">Grid to reset</param>
        /// <param name="meshGeometry">Mesh geometry to use</param>
        public static void SetMeshGeometry(this UnstructuredGrid grid, Disposable2DMeshGeometry meshGeometry)
        {
            if (!grid.IsEmpty)
            {
                grid.Clear();
            }

            grid.Vertices = meshGeometry.CreateVertices();
            grid.Edges = meshGeometry.CreateEdges();
            grid.Cells = meshGeometry.CreateCells();
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
                int[] blockFromArray = GetBlockFromArray<int>(mesh.EdgeNodes, 2, blockIndex);
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
                mesh?.FaceX == null || 
                mesh?.FaceY== null || 
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
            
            mesh.FaceNodes = new int[mesh.MaxNumberOfFaceNodes * gridCells.Count];
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
            grid.SetMeshGeometry(meshGeometry, network);
            return grid;
        }

        /// <summary>
        /// Sets the <paramref name="meshGeometry"/> to the <paramref name="discretization"/>
        /// </summary>
        /// <param name="discretization">Discretization to set</param>
        /// <param name="meshGeometry">Mesh geometry to set</param>
        /// <param name="network">Network that the <paramref name="meshGeometry"/> is based on</param>
        public static void SetMeshGeometry(this IDiscretization discretization, Disposable1DMeshGeometry meshGeometry, IHydroNetwork network)
        {
            discretization.Network = network;
            discretization.Name = meshGeometry.Name;

            var numberOfNodes = meshGeometry.NodeIds.Length;

            var networkLocations = new NetworkLocation[numberOfNodes];
            for (int i = 0; i < numberOfNodes; i++)
            {
                networkLocations[i] = new NetworkLocation
                {
                    Branch = network.Branches[meshGeometry.BranchIDs[i]],
                    Chainage = meshGeometry.BranchOffsets[i],
                    Name = meshGeometry.NodeIds[i],
                    LongName = meshGeometry.NodeLongNames[i],
                    Geometry = new Point(meshGeometry.NodesX[i], meshGeometry.NodesY[i])
                };
            }

            discretization.Clear();
            discretization.Locations.SetValues(networkLocations);
        }

        /// <summary>
        /// Creates a <see cref="Disposable1DMeshGeometry"/> based on the <paramref name="discretization"/>
        /// </summary>
        /// <param name="discretization">Discretization to base the mesh on</param>
        public static Disposable1DMeshGeometry CreateDisposable1DMeshGeometry(this IDiscretization discretization)
        {
            var locations = discretization.Locations.Values.ToArray();
            var segments = discretization.Segments.Values.ToArray();

            var locationCount = locations.Length;
            var edgeCount = segments.Length;

            var mesh = new Disposable1DMeshGeometry
            {
                Name = discretization.Name,
                NodesX = new double[locationCount],
                NodesY = new double[locationCount],
                NodeIds = new string[locationCount],
                NodeLongNames = new string[locationCount],
                BranchIDs = new int[locationCount],
                BranchOffsets = new double[locationCount],

                EdgeBranchIds = new int[edgeCount],
                EdgeCenterPointX = new double[edgeCount],
                EdgeCenterPointY = new double[edgeCount],
                EdgeCenterPointOffset = new double[edgeCount],
            };

            var branchIdLookup = discretization.Network.Branches
                .Select((b, i) => new {branch = b, index = i})
                .ToDictionary(t => t.branch, t => t.index);

            for (int i = 0; i < locationCount; i++)
            {
                var location = locations[i];

                mesh.NodesX[i] = location.Geometry?.Coordinate.X ?? 0;
                mesh.NodesY[i] = location.Geometry?.Coordinate.Y ?? 0;
                mesh.BranchIDs[i] = branchIdLookup[location.Branch];
                mesh.BranchOffsets[i] = location.Chainage;
                mesh.NodeIds[i] = location.Name;
                mesh.NodeLongNames[i] = location.LongName;
            }

            for (int i = 0; i < edgeCount; i++)
            {
                var segment = segments[i];

                mesh.EdgeBranchIds[i] = branchIdLookup[segment.Branch];
                mesh.EdgeCenterPointOffset[i] = segment.Length / 2.0;
                mesh.EdgeCenterPointX[i] = segment.Geometry.Centroid.X;
                mesh.EdgeCenterPointY[i] = segment.Geometry.Centroid.Y;
            }

            return mesh;
        }

        #endregion

        #region Network

        /// <summary>
        /// Creates a <see cref="IHydroNetwork"/> from the provided <paramref name="networkGeometry"/>
        /// </summary>
        /// <param name="networkGeometry">Network geometry to use</param>
        /// <returns>A newly created network based on the provided <paramref name="networkGeometry"/></returns>
        public static IHydroNetwork CreateNetwork(this DisposableNetworkGeometry networkGeometry)
        {
            var network = new HydroNetwork();
            network.SetNetworkGeometry(networkGeometry);
            return network;
        }

        public static void SetNetworkGeometry(this IHydroNetwork network, DisposableNetworkGeometry networkGeometry, IEnumerable<BranchFile.BranchProperties> branchProperties = null, ICollection<NodeFile.CompartmentProperties> compartmentProperties = null)
        {
            var nodes = CreateNetworkNodes(network, networkGeometry, compartmentProperties);
            var branches = CreateBranches(networkGeometry, nodes, branchProperties);

            network.Nodes.AddRange(nodes.Distinct());
            network.Branches.AddRange(branches);
        }

        public static DisposableNetworkGeometry CreateDisposable1DMeshGeometry(this HydroNetwork network)
        {
            var compartments = network.Manholes.SelectMany(m => m.Compartments).ToList();
            var hydroNodes = network.HydroNodes.ToList();

            var compartmentInfoLookup = network.Manholes
                .SelectMany(m => m.Compartments
                    .Select((c,i) => new {Compartment = c, Manhole = m, Index = i}))
                .ToDictionary(t => t.Compartment, t => t);

            var numberOfBranches = network.Branches.Count; // channels, pipes and sewer-connections
            var nodeCount = compartments.Count + hydroNodes.Count; // compartments and hydro nodes

            var mesh = new DisposableNetworkGeometry
            {
                NetworkName = network.Name,
                NodesX = new double[nodeCount],
                NodesY = new double[nodeCount],
                NodeIds = new string[nodeCount],
                NodeLongNames = new string[nodeCount],

                BranchIds = new string[numberOfBranches],
                BranchLongNames = new string[numberOfBranches],
                NodesFrom = new int[numberOfBranches],
                NodesTo = new int[numberOfBranches],
                BranchGeometryNodesCount = new int[numberOfBranches],
                BranchLengths = new double[numberOfBranches],
                BranchOrder = new int[numberOfBranches],
                BranchTypes = new int[numberOfBranches]
            };

            // cache node indices
            var nodeIndexLookup = new Dictionary<object, int>(); 

            for (int i = 0; i < compartments.Count; i++)
            {
                var compartment = compartments[i];
                var compartmentInfo = compartmentInfoLookup[compartment];
                
                var manhole = compartmentInfo.Manhole;
                var offset = (manhole.Compartments.Count - 1) * 0.5;

                var compartmentX = manhole.Geometry.Coordinate.X - offset + compartmentInfo.Index;
                
                mesh.NodesX[i] = compartmentX;
                mesh.NodesY[i] = manhole.Geometry.Coordinate.Y;
                mesh.NodeIds[i] = compartment.Name;
                mesh.NodeLongNames[i] = "";

                nodeIndexLookup.Add(compartment, i);
            }

            for (int i = 0; i < hydroNodes.Count; i++)
            {
                var hydroNode = hydroNodes[i];

                mesh.NodesX[i] = hydroNode.Geometry.Coordinate.X;
                mesh.NodesY[i] = hydroNode.Geometry.Coordinate.Y;
                mesh.NodeIds[i] = hydroNode.Name;
                mesh.NodeLongNames[i] = hydroNode.LongName;

                nodeIndexLookup.Add(hydroNode, compartments.Count + i);
            }

            var geometryXData = new List<double>();
            var geometryYData = new List<double>();

            for (int i = 0; i < numberOfBranches; i++)
            {
                var branch = network.Branches[i];
                mesh.BranchIds[i] = branch.Name;
                mesh.BranchLongNames[i] = branch.Description;
                mesh.BranchGeometryNodesCount[i] = branch.Geometry.Coordinates.Length;
                mesh.BranchLengths[i] = branch.Length;
                mesh.BranchOrder[i] = branch.OrderNumber;
                mesh.BranchTypes[i] = (int) branch.GetBranchType();

                // lookup node index in nodes array
                if (branch is ISewerConnection connection)
                {
                    mesh.NodesTo[i] = nodeIndexLookup[connection.TargetCompartment];
                    mesh.NodesFrom[i] = nodeIndexLookup[connection.SourceCompartment];
                }
                else
                {
                    mesh.NodesTo[i] = nodeIndexLookup[branch.Target];
                    mesh.NodesFrom[i] = nodeIndexLookup[branch.Source];
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

            return mesh;
        }

        private static NetworkUGridDataModel.BranchType GetBranchType(this IBranch branch)
        {
            switch (branch)
            {
                case ISewerConnection s:
                    switch (s.WaterType)
                    {
                        case SewerConnectionWaterType.None:
                            return NetworkUGridDataModel.BranchType.TransportWater;
                        case SewerConnectionWaterType.StormWater:
                            return NetworkUGridDataModel.BranchType.StormWaterFlow;
                        case SewerConnectionWaterType.DryWater:
                            return NetworkUGridDataModel.BranchType.DryWeatherFlow;
                        case SewerConnectionWaterType.Combined:
                            return NetworkUGridDataModel.BranchType.MixedFlow;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    return NetworkUGridDataModel.BranchType.SurfaceWater;
                case null:
                    throw new ArgumentNullException(nameof(branch));
            }

        }

        private static IBranch[] CreateBranches(DisposableNetworkGeometry networkGeometry, INode[] nodes, IEnumerable<BranchFile.BranchProperties> branchProperties)
        {
            var numberOfBranches = networkGeometry.BranchIds.Length;
            var nodeLookup = nodes.ToDictionary(n => n.Name, n => n);
            var propertiesLookup = branchProperties?.ToDictionary(p => p.Name);

            var branches = new IBranch[numberOfBranches];

            var geometryOffset = 0;

            for (int i = 0; i < numberOfBranches; i++)
            {
                var type = networkGeometry.BranchTypes[i];
                var branch = GetBranch(type, propertiesLookup, networkGeometry.BranchIds[i]);

                var nodeTo = nodeLookup[networkGeometry.NodeIds[networkGeometry.NodesTo[i]]];
                var nodeFrom = nodeLookup[networkGeometry.NodeIds[networkGeometry.NodesFrom[i]]];

                branch.Source = nodeFrom;
                branch.Target = nodeTo;
                branch.Name = networkGeometry.BranchIds[i];
                ((IHydroNetworkFeature)branch).LongName = networkGeometry.BranchLongNames[i];
                branch.Length = networkGeometry.BranchLengths[i];
                branch.OrderNumber = networkGeometry.BranchOrder[i];

                var nodeCount = networkGeometry.BranchGeometryNodesCount[i];
                var coordinates = new List<Coordinate>();

                for (int j = geometryOffset; j < nodeCount + geometryOffset; j++)
                {
                    coordinates.Add(new Coordinate(networkGeometry.BranchGeometryX[j], networkGeometry.BranchGeometryY[j]));
                }

                branch.Geometry = new LineString(coordinates.ToArray());

                branches[i] = branch;
            }

            return branches;
        }

        private static IBranch GetBranch(int branchTypeNumber, IReadOnlyDictionary<string, BranchFile.BranchProperties> propertiesLookup, string branchName)
        {
            var branchProperties = propertiesLookup?[branchName];
            var branchType = branchProperties?.BranchType ?? BranchFile.BranchType.Channel;

            switch (branchType)
            {
                case BranchFile.BranchType.SewerConnection:
                    return new SewerConnection { WaterType = branchProperties.WaterType };
                case BranchFile.BranchType.Pipe:
                    return new Pipe { WaterType = branchProperties.WaterType, Material = branchProperties.Material };
                default:
                    return new Channel();
            }
        }

        private static INode[] CreateNetworkNodes(IHydroNetwork network, DisposableNetworkGeometry networkGeometry, ICollection<NodeFile.CompartmentProperties> compartmentProperties = null)
        {
            var propertiesLookup = compartmentProperties?.ToDictionary(p => p.CompartmentId) ??
                                   new Dictionary<string, NodeFile.CompartmentProperties>();

            var manHoleLookup = network.Manholes.ToDictionary(m => m.Name);
            var nodeCount = networkGeometry.NodesX.Length;

            var nodes = new INode[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                INode node = null;

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
                        ManholeWidth = manholeWidth
                    };


                    if (manHoleLookup.TryGetValue(properties.ManholeId, out var existingManhole))
                    {
                        existingManhole.Compartments.Add(compartment);
                    }
                    else
                    {
                        node = new Manhole(properties.ManholeId)
                        {
                            Compartments = new EventedList<ICompartment> { compartment }
                        };

                        manHoleLookup[node.Name] = (IManhole)node;
                    }
                }
                else
                {
                    node = new HydroNode
                    {
                        Name = nodeName == "" ? null : nodeName,
                    };
                }

                ((IHydroNetworkFeature)node).LongName = networkGeometry.NodeLongNames[i] != ""
                    ? networkGeometry.NodeLongNames[i]
                    : null;

                node.Geometry = new Point(networkGeometry.NodesX[i], networkGeometry.NodesY[i]);
                node.Network = network;

                nodes[i] = node;
            }

            return nodes;
        }

        #endregion

        #region Links

        public static IList<ILink1D2D> CreateLinks(this DisposableLinksGeometry linksGeometry)
        {
            var link1D2Ds = new List<ILink1D2D>();
            link1D2Ds.SetLinks(linksGeometry);
            return link1D2Ds;
        }

        public static void SetLinks(this IList<ILink1D2D> link1D2Ds, DisposableLinksGeometry linksGeometry)
        {
            link1D2Ds.Clear();

            var numberOfLinks = linksGeometry.LinkId.Length;
            for (int i = 0; i < numberOfLinks; i++)
            {
                link1D2Ds.Add(new Link1D2D(linksGeometry.Mesh1DFrom[i], linksGeometry.Mesh2DTo[i], linksGeometry.LinkId[i])
                {
                    LongName = linksGeometry.LinkLongName[i],
                    TypeOfLink = (LinkType) linksGeometry.LinkType[i]
                });
            }
        }

        public static DisposableLinksGeometry CreateDisposableLinksGeometry(this IList<ILink1D2D> link1D2Ds)
        {
            var numberOfLinks = link1D2Ds.Count;

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
                var link1D2D = link1D2Ds[i];
                linksGeometry.LinkId[i] = link1D2D.Name;
                linksGeometry.LinkLongName[i] = link1D2D.LongName;
                linksGeometry.LinkType[i] = (int) link1D2D.TypeOfLink;
                linksGeometry.Mesh1DFrom[i] = link1D2D.DiscretisationPointIndex;
                linksGeometry.Mesh2DTo[i] = link1D2D.Link1D2DIndex;
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