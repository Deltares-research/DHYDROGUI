using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
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

        public static bool Generate1D2DLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount, LinkType linkType)
        {
            switch (linkType)
            {
                case LinkType.EmbeddedOneToOne:
                    return Get1D2DOneToOneEmbeddedLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                case LinkType.EmbeddedOneToMany:
                    return Get1D2DOneToManyEmbeddedLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                case LinkType.Lateral:
                    return Get1D2DLateralLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                case LinkType.GullySewer:
                    return Get1D2DGullyLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
                default:
                    log.ErrorFormat("1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Type of link {1} unknown", fmModel.Name, linkType);
                    return false;
            }
        }

        public static bool AddNew1D2DLink(WaterFlowFMModel fmModel, LinkType linkType, Coordinate startPoint, Coordinate endPoint, double snapTolerance = 0.0)
        {
            switch (linkType)
            {
                case LinkType.EmbeddedOneToOne:
                    return AddNew1D2DOneToOneEmbeddedLink(fmModel, startPoint, endPoint, snapTolerance);
                case LinkType.EmbeddedOneToMany:
                    return AddNew1D2DEmbeddedLink(fmModel, startPoint, endPoint, snapTolerance);
                case LinkType.Lateral:
                    return AddNew1D2DLateralLink(fmModel, startPoint, endPoint, snapTolerance);
                case LinkType.GullySewer:
                    return AddNew1D2DGullyLink(fmModel, startPoint, endPoint, snapTolerance);
                default:
                    log.ErrorFormat("New 1D2D Link between the grid and the network of WaterFlowFMModel {0} is not added. Type of link {1} unknown", fmModel.Name, linkType);
                    return false;
            }
        }

        #region main methods

        private static bool Get1D2DGullyLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, LinkType.GullySewer);
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

        private static bool Get1D2DLateralLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, LinkType.Lateral);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.GetLateral1D2DLinks(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return false;
            }
            return true;
        }

        private static bool Get1D2DOneToOneEmbeddedLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, LinkType.EmbeddedOneToOne);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.GetEmbedded1D2DLinks(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh, false);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return false;
            }
            return true;
        }

        private static bool Get1D2DOneToManyEmbeddedLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, LinkType.EmbeddedOneToMany);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.GetEmbedded1D2DLinks(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh, true);
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

        public static List<bool> GetMesh1DFilter(IDiscretization networkDiscretization, LinkType linkType)
        {
            var filterList = new List<bool>();
            var discretisationPoints = networkDiscretization.Locations.Values.ToArray();
            for (int i = 0; i < discretisationPoints.Length; i++)
            {
                var discretisationPoint = discretisationPoints[i];
                var isAvailableMesh1DPoint = false;
                switch (linkType)
                {
                    case LinkType.Lateral:
                        isAvailableMesh1DPoint = IsLateralMesh1DPoint(discretisationPoint);
                        break;
                    case LinkType.EmbeddedOneToOne: //go to next case
                    case LinkType.EmbeddedOneToMany:
                        isAvailableMesh1DPoint = IsEmbeddedMesh1DPoint(discretisationPoint);
                        break;
                    case LinkType.GullySewer:
                        isAvailableMesh1DPoint = IsStormWaterMesh1DPoint(discretisationPoint);
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

        #endregion sub methods

        #region add new link

        private static bool AddNew1D2DEmbeddedLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, LinkType.EmbeddedOneToMany, snapTolerance);

            if (link != null)
            {
                fmModel.Links.Add(link);
                return true;
            }
            return false;
        }

        private static bool AddNew1D2DOneToOneEmbeddedLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, LinkType.EmbeddedOneToOne, snapTolerance);

            if (link != null)
            {
                fmModel.Links.Add(link);
                return true;
            }
            return false;
        }

        private static bool AddNew1D2DLateralLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, LinkType.Lateral, snapTolerance);

            if (link != null)
            {
                fmModel.Links.Add(link);
                return true;
            }
            return false;
        }

        private static bool AddNew1D2DGullyLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, double snapTolerance)
        {
            var link = GetNewLink(fmModel, startCoordinate, endCoordinate, LinkType.GullySewer, snapTolerance);

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

        private static Link1D2D GetNewLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, LinkType linkType, double snapTolerance = SNAP_DISTANCE)
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
                    var link = new Link1D2D(networkLocationId, cellId)
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
