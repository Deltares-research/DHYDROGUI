using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class UGrid1D2DLinksAdapter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UGrid1D2DLinksAdapter));

        public static void Save1D2DLinks(string filePath, IList<WaterFlowFM1D2DLink> links)
        {
            using (var uGrid1D2DLinks = new UGrid1D2DLinks(filePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid1D2DLinks.Create1D2DLinksInFile(links.Count);

                var mesh1DPointIdx = links.Select(l => l.DiscretisationPointIndex).ToArray();
                var mesh2DFaceIdx = links.Select(l => l.FaceIndex).ToArray();
                var linkType = links.Select(l => (int) l.TypeOfLink).ToArray();
                var linkIds = links.Select(l => l.Name).ToArray();
                var linkLongNames = links.Select(l => l.LongName).ToArray();

                uGrid1D2DLinks.Write1D2DLinks(mesh1DPointIdx, mesh2DFaceIdx, linkType, linkIds, linkLongNames);
            }
        }

        public static IEventedList<WaterFlowFM1D2DLink> Load1D2DLinks(string filePath)
        {
            var links = new EventedList<WaterFlowFM1D2DLink>();
            using (var uGrid1D2DLinks = new UGrid1D2DLinks(filePath))
            {
                int[] mesh1DPointIdx;
                int[] mesh2DFaceIdx;
                int[] linkTypes;
                string[] linkIds;
                string[] linkLongNames;

                uGrid1D2DLinks.Read1D2DLinks(out mesh1DPointIdx, out mesh2DFaceIdx, out linkTypes, out linkIds, out linkLongNames);

                for (var iLink = 0; iLink < mesh1DPointIdx.Length; iLink++)
                {
                    var fromPoint = mesh1DPointIdx[iLink];
                    var toCell = mesh2DFaceIdx[iLink];
                    var linkName = linkIds[iLink];
                    var linkLongName = linkLongNames[iLink];
                    var linkType = linkTypes[iLink];
                    var linkId = linkIds[iLink];

                    var link = CreateLink(fromPoint, toCell, linkName, linkLongName, linkType, linkId);

                    links.Add(link);
                }

                return links;
            }
        }

        private static WaterFlowFM1D2DLink CreateLink(int fromPoint, int toCell, string linkName, string linkLongName, int linkType, string linkId)
        {
            var link = new WaterFlowFM1D2DLink(fromPoint, toCell)
            {
                Name = linkName,
                LongName = linkLongName
            };

            if (Enum.IsDefined(typeof(GridApiDataSet.LinkType), linkType))
            {
                link.TypeOfLink = (GridApiDataSet.LinkType) linkType;
            }
            else
            {
                Log.ErrorFormat("Unknown link type ({0}) of link {1} detected. Type has been set to 'mesh1D-mesh2D', no 3.", linkType, linkId);
                link.TypeOfLink = GridApiDataSet.LinkType.Mesh1DMesh2D;
            }
            return link;
        }
    }
}
