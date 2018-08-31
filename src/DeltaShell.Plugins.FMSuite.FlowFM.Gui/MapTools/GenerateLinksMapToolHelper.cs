using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.Grid;
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
    public static class GenerateLinksMapToolHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GenerateLinksMapToolHelper));

        public static bool Get1D2DLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount, GridApiDataSet.LinkType linkType)
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
                        dictionaryCellIndexLink.Add(index, new Tuple<int,int>(closestPoint.Item1, index));
                    }
                }
            }
            return dictionaryCellIndexLink.Values.ToList();
        }

        #region main methods

        private static bool Get1D2DGullyLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.GullySewer);
            IList<IGeometry> filter2DMesh = new List<IGeometry>();

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFrom2DTo1D(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh, filter2DMesh);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return true;
            }
            return false;
        }

        private static bool Get1D2DInhabitantsLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.InhabitantsSewer);
            IList<IGeometry> filter2DMesh = new List<IGeometry>();

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFrom2DTo1D(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh, filter2DMesh);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return true;
            }
            return false;
        }

        private static bool Get1D2DRoofLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.RoofSewer);

            IList<IGeometry> filter2DMesh = new List<IGeometry>();

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFrom2DTo1D(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh, filter2DMesh);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return true;
            }
            return false;
        }

        private static bool Get1D2DLateralLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filterMesh1D = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.Lateral);
            FilterMesh1DPointOutsideGrid(fmModel.NetworkDiscretization, fmModel.Grid, ref filterMesh1D);

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
            return false;
        }

        private static bool Get1D2DEmbeddedLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.Embedded);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFrom1DTo2D(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return true;
            }
            return false;
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


        private static void FilterMesh1DPointOutsideGrid(IDiscretization networkDiscretization, UnstructuredGrid grid, ref List<bool> filterMesh1D)
        {
            if (!filterMesh1D.Any(b => b)) return;
            if (!grid.Cells.Any()) return;

            var polygons = grid.Cells.Select(c => (IPolygon)c.ToPolygon(grid)).ToArray();
            var multiPolygonOfCells = new MultiPolygon(polygons);
            var locations = networkDiscretization.Locations.Values;
            for (int i = 0; i < locations.Count; i++)
            {
                if (filterMesh1D[i])
                {
                    if(multiPolygonOfCells.Intersects(locations[i].Geometry))
                    {
                        filterMesh1D[i] = false;
                    }
                }
            }
        }
        #endregion sub methods
    }
}
