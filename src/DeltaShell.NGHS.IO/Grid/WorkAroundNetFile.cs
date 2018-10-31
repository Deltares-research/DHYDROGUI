using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class WorkAroundNetFile
    {

        /// <summary>
        /// Writes the unstructured grid to a net-file
        /// </summary>
        public static void Initialize(string path, UnstructuredGrid grid)
        {
            NetCdfFile file = null;
            try
            {
                file = NetCdfFile.OpenExisting(path, true);

                if (grid.IsEmpty) return;
                file.ReDefine();
                // Add dimensions
                var netNodeDim = file.AddDimension("nNetNode", grid.Vertices.Count);
                var netLinkDim = file.AddDimension("nNetLink", grid.Edges.Count);
                var netLinkPtsDim = file.AddDimension("nNetLinkPts", 2); // number of points per vertex (link)

                // add variables
                var netNodeXVar = file.AddVariable("NetNode_x", NetCdfDataType.NcDoublePrecision, new[] { netNodeDim });
                var netNodeYVar = file.AddVariable("NetNode_y", NetCdfDataType.NcDoublePrecision, new[] { netNodeDim });

                var netLinkTypeVar = file.AddVariable("NetLinkType", NetCdfDataType.NcInteger, new[] { netLinkDim });
                var netLinkVar = file.AddVariable("NetLink", NetCdfDataType.NcInteger, new[] { netLinkDim, netLinkPtsDim });

                file.EndDefine();

                file.Flush();
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }
    }
}