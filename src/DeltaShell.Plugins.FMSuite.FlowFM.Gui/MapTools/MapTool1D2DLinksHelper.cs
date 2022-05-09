using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DeltaShell.NGHS.IO.Grid.MeshKernel;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;

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

        public static bool AddNew1D2DLink(WaterFlowFMModel fmModel, LinkGeneratingType linkType, Coordinate startPoint, Coordinate endPoint, double snapTolerance = 0.0)
        {
            var link = GetNewLink(fmModel, startPoint, endPoint, linkType, snapTolerance);
            if (link == null) 
                return false;

            if (linkType != LinkGeneratingType.GullySewer || IsLinkConnectedToAGully(link, fmModel))
            {
                fmModel.Links.Add(link);
            }

            return true;
        }

        private static Link1D2D GetNewLink(WaterFlowFMModel fmModel, Coordinate startCoordinate, Coordinate endCoordinate, LinkGeneratingType linkType, double snapTolerance = SNAP_DISTANCE)
        {
            var startPoint = new Point(startCoordinate);
            var endPoint = new Point(endCoordinate);
            var filter1DMesh = Generate1D2DLinksHelper.GetMesh1DFilter(fmModel.NetworkDiscretization.Locations.Values, linkType, generatedByUser: true);
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
                        TypeOfLink = linkType.GetLinkStorageType(),
                        Geometry = new LineString(new[] { exactStartCoordinate, endPoint.Coordinate }),
                        SnapToleranceUsed = snapTolerance
                    };
                    return link;
                }
            }
            return null;
        }

        private static bool IsLinkConnectedToAGully(ILink1D2D link, WaterFlowFMModel fmModel)
        {
            var cellIndex = link.FaceIndex;
            var grid = fmModel.Grid;
            var gullies = fmModel.Area.Gullies;

            var isLinkConnectedToAGully = gullies.Any(g => Links1D2DHelper.FindCellIndex(g.Geometry as Point, grid) == cellIndex);
            if (!isLinkConnectedToAGully)
            {
                log.ErrorFormat("Link is not connected to a cell with a gully");
            }
            return isLinkConnectedToAGully;
        }
    }
}
