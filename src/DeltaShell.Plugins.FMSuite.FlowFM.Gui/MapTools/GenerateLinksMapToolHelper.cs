using System;
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
                    return Get1D2DEmbeddedLinks(fmModel.Name, fmModel.NetFilePath, fmModel.NetworkDiscretization, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount);
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
            List<Tuple<int,IGeometry>> listIndexesGeometry = GetSelectedPoints(fmModel.NetworkDiscretization, selectedArea, SewerConnectionWaterType.DryWater);
            return false;
        }

        private static bool Get1D2DInhabitantsLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            return false;
        }

        private static bool Get1D2DRoofLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            return false;
        }

        private static bool Get1D2DLateralLinks(WaterFlowFMModel fmModel, IPolygon selectedArea, int startIndex, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            return false;
        }

        private static bool Get1D2DEmbeddedLinks(string modelName, string netFilePath, IDiscretization networkDiscretization, IPolygon selectedArea, int startIndex,
            ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var gGeomApi = new GridGeomApi();
            var ierr = gGeomApi.Get1d2dLinksFromGridAndNetwork(netFilePath, networkDiscretization, ref linksFrom,
                ref linksTo, ref startIndex, ref linksCount, selectedArea);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(
                    "1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.",
                    modelName);
                return true;
            }
            return false;
        }

        #endregion main methods

        #region sub methods

        public static List<Tuple<int, IGeometry>> GetSelectedPoints(IDiscretization networkDiscretization, IPolygon selectedArea, SewerConnectionWaterType waterType)
        {
            var indexesGeometryList = new List<Tuple<int, IGeometry>>();
            var discretisationPoints = networkDiscretization.Locations.Values.ToArray();
            for (int i = 0; i < discretisationPoints.Length; i++)
            {
                var discretisationPoint = discretisationPoints[i];
                if (selectedArea.Intersects(discretisationPoint.Geometry))
                {
                    var pipe = discretisationPoint.Branch as Pipe;
                    if (pipe != null && (pipe.WaterType == SewerConnectionWaterType.Combined || pipe.WaterType == waterType))
                    {
                        indexesGeometryList.Add(new Tuple<int, IGeometry>(i, discretisationPoint.Geometry));
                    }
                }
            }
            return indexesGeometryList;
        }

        #endregion sub methods
    }
}
