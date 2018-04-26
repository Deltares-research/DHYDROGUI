using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class UGrid1D2DLinksAdapter
    {
        public static void Save1D2DLinks(string filePath, IList<WaterFlowFM1D2DLink> links)
        {
            var uGrid1D2DLinks = new UGrid1D2DLinks(filePath, GridApiDataSet.NetcdfOpenMode.nf90_write);
            uGrid1D2DLinks.Create1D2DLinksInFile(links.Count);

            var mesh1DPointIdx = links.Select(l => l.DiscretisationPointIndex).ToArray();
            var mesh2DFaceIdx = links.Select(l => l.FaceIndex).ToArray();
            var linkType = links.Select(l => (int)l.TypeOfLink).ToArray();
            var linkIds = links.Select(l => l.Name).ToArray();
            var linkLongNames = links.Select(l => l.LongName).ToArray();

            uGrid1D2DLinks.Write1D2DLinks(mesh1DPointIdx, mesh2DFaceIdx, linkType, linkIds, linkLongNames);
        }

        public static IEventedList<WaterFlowFM1D2DLink> Load1D2DLinks(string filePath)
        {
            var links = new EventedList<WaterFlowFM1D2DLink>();
            var uGrid1D2DLinks = new UGrid1D2DLinks(filePath);

            int[] mesh1DPointIdx;
            int[] mesh2DFaceIdx;
            int[] linkType;
            string[] linkIds;
            string[] linkLongNames;

            uGrid1D2DLinks.Read1D2DLinks(out mesh1DPointIdx, out mesh2DFaceIdx, out linkType, out linkIds, out linkLongNames);

            for (var iLink = 0; iLink < mesh1DPointIdx.Length; iLink++)
            {
                var link = new WaterFlowFM1D2DLink(mesh1DPointIdx[iLink], mesh2DFaceIdx[iLink]);
                link.Name = linkIds[iLink];
                link.LongName = linkLongNames[iLink];

                links.Add(link);
            }

            return links;
        }
    }
}
