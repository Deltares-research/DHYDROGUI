using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using SharpMap.Data.Providers.EGIS.ShapeFileLib;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridFileHelper1D2DLinks 
    {
        /// <summary>
        /// Sets a list of <see cref="ILink1D2D"/> from the provided <paramref name="generatedObjectsForLinks"/> onto the <paramref name="link1D2Ds"/>
        /// </summary>
        /// <param name="generatedObjectsForLinks">All the objects needed to generate 1d2d links</param>
        /// <param name="logHandler">Optional logger to write problems into.</param>
        public void SetLinks(IGeneratedObjectsForLinks generatedObjectsForLinks, ILogHandler logHandler = null)
        {
            Ensure.NotNull(generatedObjectsForLinks, nameof(generatedObjectsForLinks));
            Ensure.NotNull(generatedObjectsForLinks.Links1D2D, nameof(generatedObjectsForLinks.Links1D2D));

            logHandler = logHandler ?? new LogHandler("Setting links correctly in model");

            try
            {
                generatedObjectsForLinks.Links1D2D.Clear();
            }
            catch (NotSupportedException ex)
            {
                logHandler.ReportError($"Cannot clear {nameof(generatedObjectsForLinks.Links1D2D)}, list has no Clear support, error: {ex.Message}");
                logHandler.LogReport();
                return;
            }

            ConcurrentQueue<ILink1D2D> links = new ConcurrentQueue<ILink1D2D>();

            QuadTree treeDiscretizationPoints = GenerateQuadTreeOfDiscretizationPoints(generatedObjectsForLinks.Discretization);
            bool valid1DMeshNodeXyCoordinatesInFile;
            try
            {
                double[] mesh1dNodesX = generatedObjectsForLinks.Mesh1d?.NodesX;
                double[] mesh1dNodesY = generatedObjectsForLinks.Mesh1d?.NodesY;
                valid1DMeshNodeXyCoordinatesInFile = mesh1dNodesX != null
                                                     && mesh1dNodesY != null
                                                     && Array.TrueForAll(mesh1dNodesX, nodeX => !nodeX.Equals(0.0))
                                                     && Array.TrueForAll(mesh1dNodesY, nodeY => !nodeY.Equals(0.0));
            }
            catch (ArgumentNullException ex)
            {
                logHandler.ReportError($"Argument null exception {ex.ParamName} when checking if mesh 1d all have X & Y coordinate values, error: {ex.Message}");
                logHandler.LogReport();
                return;
            }

            Dictionary<string, IBranch> branchesLookup = generatedObjectsForLinks.Discretization?.Network?.Branches?.ToDictionary(b => b.Name);

            UGrid1D2DSearchIndexObject discretizationUGrid1D2DSearchIndexObject = new UGrid1D2DSearchIndexObject()
            {
                BranchesLookup = branchesLookup,
                QuadTree = treeDiscretizationPoints,
                ObjectInModel = generatedObjectsForLinks.Discretization,
                ValidXyInFile = valid1DMeshNodeXyCoordinatesInFile,
                MeshFromFile = generatedObjectsForLinks.Mesh1d
            };

            QuadTree treeCells = GenerateQuadTreeOfUnstructuredGridCells(generatedObjectsForLinks.Grid);
            bool valid2DMeshFaceXyCoordinatesInFile;
            try
            {
                double[] mesh2dFaceX = generatedObjectsForLinks.Mesh2d?.FaceX;
                double[] mesh2dFaceY = generatedObjectsForLinks.Mesh2d?.FaceY;
                valid2DMeshFaceXyCoordinatesInFile = mesh2dFaceX != null
                                                     && mesh2dFaceY != null
                                                     && Array.TrueForAll(mesh2dFaceX, faceX => !faceX.Equals(0.0))
                                                     && Array.TrueForAll(mesh2dFaceY, faceY => !faceY.Equals(0.0));
            }
            catch (ArgumentNullException ex)
            {
                logHandler.ReportError($"Argument null exception {ex.ParamName} when checking if mesh 1d all have X & Y coordinate values, error: {ex.Message}");
                logHandler.LogReport();
                return;
            }

            UGrid1D2DSearchIndexObject cellUGrid1D2DSearchIndexObject = new UGrid1D2DSearchIndexObject()
            {
                QuadTree = treeCells,
                ObjectInModel = generatedObjectsForLinks.Grid,
                ValidXyInFile = valid2DMeshFaceXyCoordinatesInFile,
                MeshFromFile = generatedObjectsForLinks.Mesh2d
            };


            try
            {
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
            }
            catch (AggregateException ex)
            {
                logHandler.ReportError($"Aggregate exception when linking 1d discretization with grid (mesh1d -> mesh2d), error: {ex.Message}");
                logHandler.LogReport();
                return;
            }
            catch (ArgumentNullException ex)
            {
                logHandler.ReportError($"Argument null exception {ex.ParamName} when linking 1d discretization with grid (mesh1d -> mesh2d), error: {ex.Message}");
                logHandler.LogReport();
                return;
            }
            catch (OverflowException ex)
            {
                logHandler.ReportError($"Overflow exception when linking 1d discretization with grid (mesh1d -> mesh2d), error: {ex.Message}");
                logHandler.LogReport();
                return;
            }

            generatedObjectsForLinks.Links1D2D.AddRange(links.OrderBy(l => l.Link1D2DIndex));
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
                int maxLevels = (int)Math.Ceiling(0.4 * Math.Log(discretization.Locations.Values.Count, 2));
                Envelope envelope = discretization.Geometry.EnvelopeInternal;
                if (envelope == null)
                {
                    return null;
                }

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
            if (grid != null && envelope != null)
            {
                // see BuildQuadTree in Layer.cs of Framework which was used as base for this method
                int maxLevels = (int)Math.Ceiling(0.4 * Math.Log(grid.Cells.Count, 2));
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
            RectangleD rectangleD = new RectangleD(x - 25, y - 25, 50, 50); //empirically chosen by Ralph
            IEnumerable<int> calcPointIdxs = discretizationUGrid1D2DSearchIndexObject.QuadTree.GetIndices(ref rectangleD, 0.9f);
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
            foreach (int idx in calcPointIdxs)
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
            string branchId = networkGeometry.BranchIds[mesh1d.BranchIDs[calcPointIdx]];
            double chainage = mesh1d.BranchOffsets[calcPointIdx];

            Point geometry = null;
            if (branchesLookup.TryGetValue(branchId, out IBranch branch))
            {
                LengthIndexedLine lengthIndexedLine = new LengthIndexedLine(branch.Geometry);
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
                int[] verticesOfCellInFile = HydroUGridExtensions.GetBlockFromArray(mesh2d.FaceNodes, mesh2d.MaxNumberOfFaceNodes, cellIdx);

                Coordinate[] coordinatesOfVerticesOfCellInFile = verticesOfCellInFile
                                                                 .Where(verticesIndex => !verticesIndex.Equals((int)UGridFile.DEFAULT_NO_DATA_VALUE)
                                                                                         && !verticesIndex.Equals(int.MinValue)
                                                                                         && !verticesIndex.Equals(fillValueMesh2DFaceNodes)) // use fill value only! -999 is default of deltares / int.MinValue is default to other partner
                                                                 .Select(verticesIndex => new Coordinate(mesh2d.NodesX[verticesIndex], mesh2d.NodesY[verticesIndex])).ToArray();

                Coordinate centroid = GetCentroid(coordinatesOfVerticesOfCellInFile); // converting to GeoApi object is very costly
                x = centroid.X;
                y = centroid.Y;

            }

            if (double.IsNaN(x) || double.IsNaN(y) || !(cellUGrid1D2DSearchIndexObject.ObjectInModel is UnstructuredGrid grid))
            {
                return cellIdx; // use cellIdx from 1d2d link administration of file
            }

            // Find calcPointIdx from provided grid with the coordinates of the face in the file
            faceCoordinateFromFile = new Coordinate(x, y);
            RectangleD rectangleD = new RectangleD(x, y, 50, 50);
            IEnumerable<int> cellIdxs = cellUGrid1D2DSearchIndexObject.QuadTree.GetIndices(ref rectangleD, 0);

            cellIdx = GetCellIndexNearestToCoordinate(faceCoordinateFromFile, cellIdxs, grid, cellIdx);

            return cellIdx;
        }

        private static int GetCellIndexNearestToCoordinate(Coordinate faceCoordinateFromFile, IEnumerable<int> cellIdxs, UnstructuredGrid grid, int cellIdx)
        {
            double distance = double.MaxValue;
            foreach (int idx in cellIdxs)
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

            for (int cellVertexIndex = 0; cellVertexIndex < numberOfVertices; cellVertexIndex++)
            {
                Coordinate coordinate = nodes[cellVertexIndex];
                xSum += coordinate.X;
                ySum += coordinate.Y;
            }
            return new Coordinate(xSum / numberOfVertices, ySum / numberOfVertices);
        }
    }
}