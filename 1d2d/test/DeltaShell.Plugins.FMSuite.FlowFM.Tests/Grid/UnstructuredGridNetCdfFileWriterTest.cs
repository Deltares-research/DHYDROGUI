using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Grid
{
    [TestFixture]
    public class UnstructuredGridNetCdfFileWriterTest
    {
        [Test, Category(TestCategory.DataAccess)]
        public void WriteRgfGridNetCdfFile()
        {
            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var grid = new UnstructuredGrid
                {
                    CoordinateSystem = coordinateSystemFactory.CreateFromEPSG(3857) // WGS 84 / Pseudo-Mercator
                };
            
            #region Create simple regular grid 
            
            // 3  4  5
            // +--+--+
            // |  |  |
            // +--+--+
            // 0  1  2

            grid.Vertices.Add(new Coordinate(0, 0)); // index = 0
            grid.Vertices.Add(new Coordinate(1, 0)); // index = 1
            grid.Vertices.Add(new Coordinate(2, 0)); // index = 2
            grid.Vertices.Add(new Coordinate(0, 1)); // index = 3
            grid.Vertices.Add(new Coordinate(1, 1)); // index = 4
            grid.Vertices.Add(new Coordinate(2, 1)); // index = 5

            grid.Edges.Add(new Edge(0, 1));
            grid.Edges.Add(new Edge(1, 2));

            grid.Edges.Add(new Edge(3, 4));
            grid.Edges.Add(new Edge(4, 5));

            grid.Edges.Add(new Edge(0, 3));
            grid.Edges.Add(new Edge(1, 4));
            grid.Edges.Add(new Edge(2, 5));

            #endregion

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netFile.nc") ;
            
            NetFile.Write(path, grid);

            NetCdfFile netFile = null;

            try
            {
                netFile = NetCdfFile.OpenExisting(path);

                Assert.AreEqual(3, netFile.GetAllDimensions().Count()); // nNetNode, nNetLink, nNetLinkPts
                Assert.AreEqual(6, netFile.GetVariables().Count()); // NetNode_x, NetNode_y, NetLinkType, NetLink, crs

                Assert.AreEqual(grid.Vertices.Count, netFile.GetDimensionLength(netFile.GetDimension("nNetNode")));
                Assert.AreEqual(grid.Edges.Count, netFile.GetDimensionLength(netFile.GetDimension("nNetLink")));
                Assert.AreEqual(2, netFile.GetDimensionLength(netFile.GetDimension("nNetLinkPts")));

                var netNodeX = netFile.GetVariableByName("NetNode_x");
                var netNodeY = netFile.GetVariableByName("NetNode_y");
                var netLinkType = netFile.GetVariableByName("NetLinkType");
                var netLink = netFile.GetVariableByName("NetLink");
                var crs = netFile.GetVariableByName("crs");

                Assert.NotNull(netNodeX);
                Assert.NotNull(netNodeY);
                Assert.NotNull(netLinkType);
                Assert.NotNull(netLink);
                Assert.NotNull(crs);

                Assert.AreEqual("3857", netFile.GetAttributeValue(crs, "EPSG"));
                Assert.AreEqual(grid.CoordinateSystem.WKT, netFile.GetAttributeValue(crs, "spatial_ref"));

                Assert.AreEqual(grid.Vertices.Select(v => v.X), netFile.Read(netNodeX));
                Assert.AreEqual(grid.Vertices.Select(v => v.Y), netFile.Read(netNodeY));

                var expectedLinkArray = new int[grid.Edges.Count, 2]; // 2 => from, to
                
                var edgeCount = 0;
                grid.Edges.ForEach(e =>
                    {
                        // +1 because rgfGrid does not use zero based vertex indices
                        expectedLinkArray[edgeCount, 0] = e.VertexFromIndex + 1;
                        expectedLinkArray[edgeCount, 1] = e.VertexToIndex + 1;
                        edgeCount++;
                    });

                Assert.AreEqual(expectedLinkArray, netFile.Read(netLink));
            }
            finally
            {
                if (netFile != null)
                {
                    netFile.Close();
                }
            }
        }
    }
}