using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.NetCdf;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class UnstructuredGridNetCdfFileWriter
    {
        /// <summary>
        /// Writes a RGFGrid NetCDF file using the unstructuredGrid
        /// </summary>
        /// <param name="path">Path of the NetCDF file</param>
        /// <param name="unstructuredGrid">UnstructuredGrid to write</param>
        public static void WriteRgfGridNetCdfFile(string path, UnstructuredGrid unstructuredGrid)
        {
            if (unstructuredGrid.IsEmpty)
            {
                throw new Exception("Cannot write empty grid to net file");
            }
            NetCdfFile file = null;
            try
            {
                file = NetCdfFile.CreateNew(path);

                // Add dimensions
                var netNodeDim = file.AddDimension("nNetNode", unstructuredGrid.Vertices.Count);
                var netLinkDim = file.AddDimension("nNetLink", unstructuredGrid.Edges.Count);
                var netLinkPtsDim = file.AddDimension("nNetLinkPts", 2); // number of points per vertex (link)

                // add variables
                var netNodeXVar = file.AddVariable("NetNode_x", NetCdfDataType.NcDoublePrecision, new[] {netNodeDim});
                var netNodeYVar = file.AddVariable("NetNode_y", NetCdfDataType.NcDoublePrecision, new[] {netNodeDim});

                var netLinkTypeVar = file.AddVariable("NetLinkType", NetCdfDataType.NcInteger, new[] {netLinkDim});
                var netLinkVar = file.AddVariable("NetLink", NetCdfDataType.NcInteger, new[] {netLinkDim, netLinkPtsDim});

                // set attributes
                if (unstructuredGrid.CoordinateSystem != null && unstructuredGrid.CoordinateSystem.IsGeographic)
                {
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("axis", "theta"));
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("long_name", "longitude of vertex"));
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("units", "degrees_east"));
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("standard_name", "longitude"));

                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("axis", "phi"));
                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("long_name", "latitude of vertex"));
                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("units", "degrees_north"));
                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("standard_name", "latitude"));
                }
                else
                {
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("axis", "X"));
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("long_name", "x-coordinate in Cartesian system"));
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("units", "metre"));
                    file.AddAttribute(netNodeXVar, new NetCdfAttribute("standard_name", "projection_x_coordinate"));

                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("axis", "Y"));
                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("long_name", "y-coordinate in Cartesian system"));
                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("units", "metre"));
                    file.AddAttribute(netNodeYVar, new NetCdfAttribute("standard_name", "projection_y_coordinate"));
                }

                if (unstructuredGrid.CoordinateSystem != null)
                {
                    var crsVar = file.AddVariable("crs", NetCdfDataType.NcInteger, new NetCdfDimension[0]);
                    file.AddAttribute(crsVar, new NetCdfAttribute("spatial_ref", unstructuredGrid.CoordinateSystem.WKT));
                    file.AddAttribute(crsVar, new NetCdfAttribute("EPSG", (int)unstructuredGrid.CoordinateSystem.AuthorityCode));

                    file.AddGlobalAttribute(new NetCdfAttribute("Spherical", unstructuredGrid.CoordinateSystem.IsGeographic ? 1 : 0));
                }

                file.EndDefine();

                // write vertices data
                file.Write(netNodeXVar, unstructuredGrid.Vertices.Select(c => c.X).ToArray());
                file.Write(netNodeYVar, unstructuredGrid.Vertices.Select(c => c.Y).ToArray());

                // write edges data
                file.Write(netLinkTypeVar, Enumerable.Repeat(2, unstructuredGrid.Edges.Count).ToArray());
                file.Write(netLinkVar, CreateLinkData(unstructuredGrid.Edges));
                
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

        private static Array CreateLinkData(IList<Edge> edges)
        {
            var array = new int[edges.Count,2];
            var edgeCount = 0;
            edges.ForEach(e =>
                {
                    // +1 because rgfGrid does not use zero based vertex indices
                    array[edgeCount, 0] = e.VertexFromIndex + 1;
                    array[edgeCount, 1] = e.VertexToIndex + 1; 
                    edgeCount++;
                });
            return array;
        }
    }
}