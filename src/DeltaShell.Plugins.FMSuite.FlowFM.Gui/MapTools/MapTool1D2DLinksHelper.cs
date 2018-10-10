using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    /// <summary>
    /// Class to help the maptool.
    /// Determinates the requestst and translated it to GridGeom calls.
    /// Fills up the missing functionality (WIP) in the GridGeom.
    /// Process can be slow ...
    /// </summary>
    public static class MapTool1D2DLinksHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MapTool1D2DLinksHelper));
        private const double SNAP_DISTANCE = 10.0; //

        public static bool Generate1D2DLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount, GridApiDataSet.LinkType linkType)
        {
            switch (linkType)
            {
                case GridApiDataSet.LinkType.Embedded:
                    return Get1D2DEmbeddedLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                case GridApiDataSet.LinkType.Lateral:
                    return Get1D2DLateralLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                case GridApiDataSet.LinkType.RoofSewer:
                    return Get1D2DRoofLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                case GridApiDataSet.LinkType.InhabitantsSewer:
                    return Get1D2DInhabitantsLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                case GridApiDataSet.LinkType.GullySewer:
                    return Get1D2DGullyLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                default:
                    log.ErrorFormat("1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Type of link {1} unknown", fmModel.Name, linkType);
                    return false;
            }
        }

        public static bool AddNew1D2DLink(WaterFlowFMModel fmModel, GridApiDataSet.LinkType linkType, Coordinate startPoint, Coordinate endPoint, double snapTolerance = 0.0)
        {
            switch (linkType)
            {
                case GridApiDataSet.LinkType.Embedded:
                    return AddNew1D2DEmbeddedLink(fmModel, startPoint, endPoint, snapTolerance);
                case GridApiDataSet.LinkType.Lateral:
                    return AddNew1D2DLateralLink(fmModel, startPoint, endPoint, snapTolerance);
                case GridApiDataSet.LinkType.RoofSewer:
                    return AddNew1D2DRoofLink(fmModel, startPoint, endPoint, snapTolerance);
                case GridApiDataSet.LinkType.InhabitantsSewer:
                    return AddNew1D2DInhabitantsLink(fmModel, startPoint, endPoint, snapTolerance);
                case GridApiDataSet.LinkType.GullySewer:
                    return AddNew1D2DGullyLink(fmModel, startPoint, endPoint, snapTolerance);
                default:
                    log.ErrorFormat("New 1D2D Link between the grid and the network of WaterFlowFMModel {0} is not added. Type of link {1} unknown", fmModel.Name, linkType);
                    return false;
            }
        }

        /// <summary>
        /// Temporary method to get from/to indexes for creating lateral1d2d links. Can be deleted when GridGeom function for Lateral/River links is ready
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="gridPoints1D">The grid points1 d.</param>
        /// <param name="selectionArea">The selection area.</param>
        /// <returns></returns>
        public static IList<Tuple<int,int>> TemporaryMethodGetFromToIndexesFor1D2DLinks(UnstructuredGrid grid, IList<INetworkLocation> gridPoints1D, IPolygon selectionArea, IList<bool> filterMesh1D)
        {
            var edgesInSelectedArea = grid.Edges.Where(e => selectionArea.Intersects(new Point(e.GetEdgeCenter(grid)))).ToList();
            var filteredGridPoints1D = new List<Tuple<INetworkLocation, int>>();
            for(int i = 0; i < gridPoints1D.Count; i++)
            {
                if (filterMesh1D[i])
                {
                    filteredGridPoints1D.Add(new Tuple<INetworkLocation, int>(gridPoints1D[i],i));
                }
            }

            var dictionaryCellIndexLink = new Dictionary<int,Tuple<int,int>>();
            foreach (var edge in edgesInSelectedArea)
            {
                var edgeCenter = edge.GetEdgeCenter(grid);
                var closestPoint = filteredGridPoints1D.Select(p => new Tuple<int, INetworkLocation, double>(p.Item2, p.Item1, p.Item1.Geometry.Coordinate.Distance(edgeCenter)))
                        .OrderBy(t => t.Item3).FirstOrDefault();

                if (closestPoint == null)
                {
                    continue;
                }

                //subselection of all cells
                var cells = grid.VertexToCellIndices[edge.VertexFromIndex]
                    .Concat(grid.VertexToCellIndices[edge.VertexToIndex])
                    .Select(i => grid.Cells[i]).Distinct().ToList();

                var cellsOnEdge = cells.Where(c =>
                    c.VertexIndices.Contains(edge.VertexFromIndex) &&
                    c.VertexIndices.Contains(edge.VertexToIndex));

                if (cellsOnEdge.Count() == 1)
                {
                    var edgeCell = cellsOnEdge.First();
                    var index = grid.Cells.IndexOf(edgeCell);
                    if (!dictionaryCellIndexLink.ContainsKey(index))
                    {
                        //dictionaryCellIndexLink.Add(index, new Tuple<int,int>(closestPoint.Item1, index));
                        dictionaryCellIndexLink.Add(index, new Tuple<int, int>(index + 1, closestPoint.Item1 + 1)); //gridgeom swapt from to (2d-1d instead of 1d-2d, so we do the same in this temp method), index 1 based
                    }
                }
            }
            return dictionaryCellIndexLink.Values.ToList();
        }

        #region main methods

        private static bool Get1D2DGullyLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.GullySewer);
            var geometryGullies = fmModel.Area.Gullies.Where(r => r.Geometry.Intersects(selectedArea)).Select(r => r.Geometry);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFromGullies(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, filter1DMesh, geometryGullies);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return false;
            }
            return true;
        }

        private static bool Get1D2DInhabitantsLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.InhabitantsSewer);
            return Get1D2DRoofLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount, filter1DMesh);
        }

        private static bool Get1D2DRoofLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.RoofSewer);
            return Get1D2DRoofLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount, filter1DMesh);
        }

        private static bool Get1D2DRoofLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount, List<bool> filter1DMesh)
        {
            var geometryRoofs = fmModel.Area.RoofAreas.Where(r => r.Geometry.Intersects(selectedArea)).Select(r => r.Geometry);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFromRoofs(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, filter1DMesh, geometryRoofs);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return false;
            }
            return true;
        }

        private static bool Get1D2DLateralLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filterMesh1D = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.Lateral);
            FilterMesh1DPointInGrid(fmModel.NetworkDiscretization, fmModel.Grid, ref filterMesh1D, true);

            var gGeomApi = new GridGeomApi();
            //Not ready yet
            //            var ierr = gGeomApi.Get1D2DLinksFrom2DBoundaryCellsTo1D(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filterMesh1D);
            //            if (ierr != GridApiDataSet.GridConstants.NOERR)
            //            {
            //                log.ErrorFormat(
            //                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
            //                    fmModel.Name);
            //                return true;
            //            }
            var grid = fmModel.Grid;
            var discretization = fmModel.NetworkDiscretization;
            var tuplesFromToIndexes = TemporaryMethodGetFromToIndexesFor1D2DLinks(grid, discretization.Locations.Values, selectedArea, filterMesh1D);
            linksFrom = tuplesFromToIndexes.Select(t => t.Item1).ToList();
            linksTo = tuplesFromToIndexes.Select(t => t.Item2).ToList();
            linksCount = tuplesFromToIndexes.Count;
            return true;
        }

        private static bool Get1D2DEmbeddedLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.Embedded);
            FilterMesh1DPointInGrid(fmModel.NetworkDiscretization, fmModel.Grid, ref filter1DMesh);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFrom1DTo2D(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return false;
            }
            return true;
        }

        #endregion main methods

        #region sub methods

        public static List<bool> GetMesh1DFilter(IDiscretization networkDiscretization, GridApiDataSet.LinkType linkType)
        {
            var filterList = new List<bool>();
            var discretisationPoints = networkDiscretization.Locations.Values.ToArray();
            for (int i = 0; i < discretisationPoints.Length; i++)
            {
                var discretisationPoint = discretisationPoints[i];
                var isAvailableMesh1DPoint = false;
                switch (linkType)
                {
                    case GridApiDataSet.LinkType.Lateral:
                        isAvailableMesh1DPoint = IsLateralMesh1DPoint(discretisationPoint);
                        break;
                    case GridApiDataSet.LinkType.Embedded:
                        isAvailableMesh1DPoint = IsEmbeddedMesh1DPoint(discretisationPoint);
                        break;
                    case GridApiDataSet.LinkType.RoofSewer:
                        isAvailableMesh1DPoint = IsRoofSewerMesh1DPoint(discretisationPoint);
                        break;
                    case GridApiDataSet.LinkType.GullySewer:
                        isAvailableMesh1DPoint = IsStormWaterMesh1DPoint(discretisationPoint);
                        break;
                    case GridApiDataSet.LinkType.InhabitantsSewer:
                        isAvailableMesh1DPoint = IsDryWaterMesh1DPoint(discretisationPoint);
                        break;
                }
                filterList.Add(isAvailableMesh1DPoint);
            }
            return filterList;
        }

        private static bool IsLateralMesh1DPoint(INetworkLocation discretisationPoint)
        {
            var sewerConnection = discretisationPoint.Branch as SewerConnection;
            return sewerConnection == null;
        }

        private static bool IsEmbeddedMesh1DPoint(INetworkLocation discretisationPoint)
        {
            var sewerConnection = discretisationPoint.Branch as SewerConnection;
            return sewerConnection == null || sewerConnection.WaterType == SewerConnectionWaterType.Combined || sewerConnection.WaterType == SewerConnectionWaterType.StormWater;
        }

        private static bool IsRoofSewerMesh1DPoint(INetworkLocation discretisationPoint)
        {
            var sewerConnection = discretisationPoint.Branch as SewerConnection;
            return sewerConnection != null && (sewerConnection.WaterType == SewerConnectionWaterType.Combined || sewerConnection.WaterType == SewerConnectionWaterType.StormWater);
        }

        private static bool IsStormWaterMesh1DPoint(INetworkLocation discretisationPoint)
        {
            var sewerConnection = discretisationPoint.Branch as SewerConnection;
            return sewerConnection != null && (sewerConnection.WaterType == SewerConnectionWaterType.Combined || sewerConnection.WaterType == SewerConnectionWaterType.StormWater);
        }

        private static bool IsDryWaterMesh1DPoint(INetworkLocation discretisationPoint)
        {
            var sewerConnection = discretisationPoint.Branch as SewerConnection;
            return sewerConnection != null && (sewerConnection.WaterType == SewerConnectionWaterType.Combined || sewerConnection.WaterType == SewerConnectionWaterType.DryWater);
        }


        private static void FilterMesh1DPointInGrid(IDiscretization networkDiscretization, UnstructuredGrid grid, ref List<bool> filterMesh1D, bool bOutsideGrid = false)
        {
            if (!filterMesh1D.Any(b => b)) return;
            if (!grid.Cells.Any()) return;

            var extent = grid.GetExtents();
            var locations = networkDiscretization.Locations.Values;
            for (int i = 0; i < locations.Count; i++)
            {
                if (filterMesh1D[i])
                {
                    var coordinate = locations[i].Geometry.Coordinate;
                    var geometry = locations[i].Geometry;
                    var nearestIndex = grid.IndexOfNearestCell(coordinate);
                    if (extent.Intersects(coordinate) == bOutsideGrid && (grid.Cells[nearestIndex].ToPolygon(grid)).Intersects(geometry) == bOutsideGrid)
                    {
                        filterMesh1D[i] = false;
                    }
                }
            }
        }
        #endregion sub methods

        #region add new link

        private static bool AddNew1D2DEmbeddedLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, GridApiDataSet.LinkType.Embedded, snapTolerance);

            if (link != null)
            {
                fmModel.Links.Add(link);
                return true;
            }
            return false;
        }

        private static bool AddNew1D2DLateralLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, GridApiDataSet.LinkType.Lateral, snapTolerance);

            if (link != null)
            {
                fmModel.Links.Add(link);
                return true;
            }
            return false;
        }

        private static bool AddNew1D2DRoofLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, GridApiDataSet.LinkType.RoofSewer, snapTolerance);

            if (link != null)
            {
                if (!IsLinkConnectedToARoof(link.Geometry.Coordinates.LastOrDefault(), fmModel.Area.RoofAreas))
                {
                    log.ErrorFormat("Link is not connected to a roof");
                }
                else
                { 
                    fmModel.Links.Add(link);
                    return true;
                }

            }
            return false;
        }

        private static bool AddNew1D2DInhabitantsLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, GridApiDataSet.LinkType.InhabitantsSewer, snapTolerance);

            if (link != null)
            {
                if (!IsLinkConnectedToARoof(link.Geometry.Coordinates.LastOrDefault(), fmModel.Area.RoofAreas))
                {
                    log.ErrorFormat("Link is not connected to a roof");
                }
                else
                {
                    fmModel.Links.Add(link);
                    return true;
                }

            }
            return false;
        }

        private static bool AddNew1D2DGullyLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, GridApiDataSet.LinkType.GullySewer, snapTolerance);

            if (link != null)
            {
                if (!IsLinkConnectedToAGully(link.FaceIndex, fmModel.Grid, fmModel.Area.Gullies))
                {
                    log.ErrorFormat("Link is not connected to a cell with a gully");
                }
                else
                {
                    fmModel.Links.Add(link);
                    return true;
                }

            }
            return false;
        }

        private static WaterFlowFM1D2DLink GetNewLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, GridApiDataSet.LinkType linkType, double snapTolerance = SNAP_DISTANCE)
        {
            var startPoint = new Point(startCoordinate);
            var endPoint = new Point(endCoordinate);
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, linkType);
            var networkLocationId = Links1D2DHelper.FindCalculationPointIndex(startPoint, fmModel.NetworkDiscretization, snapTolerance, filter1DMesh);

            if (networkLocationId == Links1D2DHelper.MISSING_INDEX)
            {
                log.WarnFormat("No discretization point for type {0} has been found for the start point of the link. We will the reverse direction.", linkType);
                startPoint = new Point(endCoordinate);
                endPoint = new Point(startCoordinate);
                networkLocationId = Links1D2DHelper.FindCalculationPointIndex(startPoint, fmModel.NetworkDiscretization, snapTolerance, filter1DMesh);
            }

            if (networkLocationId == Links1D2DHelper.MISSING_INDEX)
            {
                log.ErrorFormat("No discretization point for type {0} has been found. Maybe the type of channel or pipe is not suitable for this link.", linkType);
            }
            else
            {
                var cellId = Links1D2DHelper.FindCellIndex(endPoint, fmModel.Grid);
                if (cellId == Links1D2DHelper.MISSING_INDEX)
                {
                    log.ErrorFormat("No grid cell has been found for the link.");
                }
                else
                {
                    var link = new WaterFlowFM1D2DLink(networkLocationId, cellId)
                    {
                        TypeOfLink = linkType,
                        Geometry = new LineString(new[] { startPoint.Coordinate, endPoint.Coordinate }),
                        SnapToleranceUsed = snapTolerance
                    };
                    return link;
                }
            }
            return null;
        }

        private static bool IsLinkConnectedToARoof(Coordinate lastCoordinate, IEnumerable<RoofArea> roofAreas)
        {
            var point = new Point(lastCoordinate);
            return roofAreas.Any(r => r.Geometry.Intersects(point));
        }

        private static bool IsLinkConnectedToAGully(int cellIndex, UnstructuredGrid grid, IEnumerable<Gully> gullies)
        {
            return gullies.Any(g => Links1D2DHelper.FindCellIndex(g.Geometry as Point, grid) == cellIndex);
        }

        #endregion add new link
    }
}
