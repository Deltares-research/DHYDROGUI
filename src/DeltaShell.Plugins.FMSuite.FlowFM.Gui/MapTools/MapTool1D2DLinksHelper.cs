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

        public static IEnumerable<ILink1D2D> Generate1D2DLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, LinkType linkType)
        {
            using (var disposableMeshGeometry = new DisposableMeshGeometryGridGeom(fmModel.Grid))
            {
                LinkInformation linkInformation;
                switch (linkType)
                {
                    case LinkType.EmbeddedOneToOne:
                        linkInformation = Get1D2DOneToOneEmbeddedLinks(disposableMeshGeometry, fmModel.NetworkDiscretization, selectedArea);
                        break;
                    case LinkType.EmbeddedOneToMany:
                        linkInformation = Get1D2DOneToManyEmbeddedLinks(disposableMeshGeometry, fmModel.NetworkDiscretization, selectedArea);
                        break;
                    case LinkType.Lateral:
                        linkInformation = Get1D2DLateralLinks(disposableMeshGeometry, fmModel.NetworkDiscretization, selectedArea);
                        break;
                    case LinkType.GullySewer:
                        var geometryGullies = fmModel.Area.Gullies.Where(r => r.Geometry.Intersects(selectedArea)).Select(r => r.Geometry);
                        linkInformation = Get1D2DGullyLinks(disposableMeshGeometry, fmModel.NetworkDiscretization, selectedArea, geometryGullies);
                        break;
                    default:
                        log.ErrorFormat("1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Type of link {1} unknown", fmModel.Name, linkType);
                        return Enumerable.Empty<ILink1D2D>();
                }

                return linkInformation != null
                    ? Creates1d2dLinks(linkInformation, fmModel.Grid, fmModel.NetworkDiscretization, linkType)
                    : Enumerable.Empty<ILink1D2D>();
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

        private static IList<Link1D2D> Creates1d2dLinks(LinkInformation linkInformation, UnstructuredGrid grid, IDiscretization networkDiscretization, LinkType linkType)
        {
            var lstNewLinks = new List<Link1D2D>();
            for (int i = 0; i < linkInformation.fromIndices.Length; i++)
            {
                //seems lists are swapt  
                var pointIndex = linkInformation.toIndices[i];
                var cellIndex = linkInformation.fromIndices[i];

                var cell = grid.Cells[cellIndex];
                var node = networkDiscretization.Locations.Values[pointIndex];
                var link = new Link1D2D(pointIndex, cellIndex)
                {
                    Geometry = new LineString(new[] { node.Geometry.Coordinate, cell.Center }),
                    TypeOfLink = linkType
                };
                lstNewLinks.Add(link);
            }
            return lstNewLinks;
        }

        private static LinkInformation Get1D2DGullyLinks(DisposableMeshGeometryGridGeom disposableMeshGeometry, IDiscretization discretization, IPolygon selectedArea, IEnumerable<IGeometry> geometryGullies)
        {
            var filter1DMesh = GetMesh1DFilter(discretization, LinkType.GullySewer, selectedArea);

            var gGeomApi = new GridGeomApi();
            var links = gGeomApi.Get1D2DLinksFromGullies(disposableMeshGeometry, discretization, filter1DMesh, geometryGullies);
            if (gGeomApi.LastErrorCode != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel. Please make sure the grid has been saved and the network is correct.");
            }
            return links;
        }

        private static LinkInformation Get1D2DLateralLinks(DisposableMeshGeometryGridGeom disposableMeshGeometry, IDiscretization discretization, IPolygon selectedArea)
        {
            var filter1DMesh = GetMesh1DFilter(discretization, LinkType.Lateral);

            var gGeomApi = new GridGeomApi();
            var links = gGeomApi.GetLateral1D2DLinks(disposableMeshGeometry, discretization, selectedArea, filter1DMesh);
            if (gGeomApi.LastErrorCode != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel. Please make sure the grid has been saved and the network is correct.");
            }
            return links;
        }

        private static LinkInformation Get1D2DOneToOneEmbeddedLinks(DisposableMeshGeometryGridGeom disposableMeshGeometry, IDiscretization discretization, IPolygon selectedArea)
        {
            var filter1DMesh = GetMesh1DFilter(discretization, LinkType.EmbeddedOneToOne, selectedArea);

            var gGeomApi = new GridGeomApi();
            var links = gGeomApi.GetEmbedded1D2DLinks(disposableMeshGeometry, discretization, selectedArea, filter1DMesh, false);
            if (gGeomApi.LastErrorCode != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel. Please make sure the grid has been saved and the network is correct.");
            }
            return links;
        }

        private static LinkInformation Get1D2DOneToManyEmbeddedLinks(DisposableMeshGeometryGridGeom disposableMeshGeometry, IDiscretization discretization, IPolygon selectedArea)
        {
            var filter1DMesh = GetMesh1DFilter(discretization, LinkType.EmbeddedOneToMany, selectedArea);

            var gGeomApi = new GridGeomApi();
            var links = gGeomApi.GetEmbedded1D2DLinks(disposableMeshGeometry, discretization, selectedArea, filter1DMesh, true);
            if (gGeomApi.LastErrorCode != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel. Please make sure the grid has been saved and the network is correct.");
            }
            return links;
        }

        #endregion main methods

        #region sub methods

        public static List<bool> GetMesh1DFilter(IDiscretization networkDiscretization, LinkType linkType, IPolygon selectedArea = null)
        {
            var filterList = new List<bool>();
            var discretisationPoints = networkDiscretization.Locations.Values.ToArray();

            for (int i = 0; i < discretisationPoints.Length; i++)
            {
                var discretisationPoint = discretisationPoints[i];
                var isAvailableMesh1DPoint = false;
                if (selectedArea == null || selectedArea.Intersects(discretisationPoint.Geometry))
                {
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
                Coordinate exactStartCoordinate = fmModel.NetworkDiscretization.Locations.Values[networkLocationId].Geometry.Coordinate;
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
                        Geometry = new LineString(new[] { exactStartCoordinate, endPoint.Coordinate }),
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
