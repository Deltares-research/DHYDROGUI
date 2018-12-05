using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using log4net;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class UGrid1D2DLinksAdapter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UGrid1D2DLinksAdapter));

        public static void Save1D2DLinks(string filePath, IEnumerable<ILink1D2D> links)
        {
            using (var uGrid1D2DLinks = new UGrid1D2DLinks(filePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                var links1D2D = links.ToList();
                uGrid1D2DLinks.Create1D2DLinksInFile(links1D2D.Count);

                var mesh1DPointIdx = links1D2D.Select(l => l.DiscretisationPointIndex).ToArray();
                var mesh2DFaceIdx = links1D2D.Select(l => l.FaceIndex).ToArray();
                var linkType = links1D2D.Select(l => (int) l.TypeOfLink).ToArray();
                var linkIds = links1D2D.Select(l => l.Name).ToArray();
                var linkLongNames = links1D2D.Select(l => l.LongName).ToArray();

                uGrid1D2DLinks.Write1D2DLinks(mesh1DPointIdx, mesh2DFaceIdx, linkType, linkIds, linkLongNames);
            }
        }

        public static IEnumerable<ILink1D2D> Load1D2DLinks(string filePath)
        {
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

                    yield return link;
                }
            }
        }

        private static ILink1D2D CreateLink(int fromPoint, int toCell, string linkName, string linkLongName, int linkType, string linkId)
        {
            var link = new Link1D2D(fromPoint, toCell)
            {
                Name = linkName,
                LongName = linkLongName
            };

            if (Enum.IsDefined(typeof(LinkType), linkType))
            {
                link.TypeOfLink = (LinkType) linkType;
            }
            else
            {
                Log.ErrorFormat("Unknown link type ({0}) of link {1} detected. Type has been set to 'mesh1D-mesh2D', no 3.", linkType, linkId);
                link.TypeOfLink = LinkType.Embedded;
            }
            return link;
        }
    }
}
