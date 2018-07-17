using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
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
            var filter1DMesh = GetMesh1DFilter(fmModel.NetworkDiscretization, GridApiDataSet.LinkType.Lateral);

            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1D2DLinksFrom2DBoundaryCellsTo1D(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea, filter1DMesh);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    fmModel.Name);
                return true;
            }
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

        #endregion sub methods
    }
}
