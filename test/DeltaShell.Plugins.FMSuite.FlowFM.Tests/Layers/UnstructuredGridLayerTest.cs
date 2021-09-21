using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpMapTestUtils;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Layers
{
    // TODO : move to SharpMap tests
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class UnstructuredGridLayerTest
    {
        [Test]
        public void ShowGridLayer()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var grid = MapFileImporter.Import(mduPath, "bendprof_map.nc");

            var gridLayer = new UnstructuredGridLayer { Grid = grid };

            var map = new Map { Layers = { gridLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        public void ShowLargerGridLayer()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var grid = MapFileImporter.Import(mduPath, "pensioen_map.nc");

            var gridLayer = new UnstructuredGridLayer { Grid = grid };

            var map = new Map { Layers = { gridLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Ignore("big")]
        [Category("ToCheck")]
        public void ShowHugeGridLayer()
        {
            const int ms = 1000;
            const int ns = 1000;
            const int numVertices = (ms + 1)*(ns + 1);
            
            var vertices = new List<Coordinate>(numVertices);
            var edgesVertexIndices = new int[ms * ns * 4, 2];

            var r = new Random();

            // generate random grid
            for (int n = 0; n <= ns; n++) 
            {
                for (int m = 0; m <= ms; m++)
                {
                    vertices.Add(new Coordinate(m, n, r.Next(20)));
                }
            }

            for (int m = 0; m < ms; m++)
            {
                for (int n = 0; n < ns; n++)
                {
                    var c = n * ms + m;
                    var lb = c + n;
                    var rb = lb + 1;
                    var lo = rb + ms;
                    var ro = lo + 1;

                    var e = c*4;
                    edgesVertexIndices[e, 0] = lb;
                    edgesVertexIndices[e, 1] = rb;
                    edgesVertexIndices[e + 1, 0] = rb;
                    edgesVertexIndices[e + 1, 1] = ro;
                    edgesVertexIndices[e + 2, 0] = ro;
                    edgesVertexIndices[e + 2, 1] = lo;
                    edgesVertexIndices[e + 3, 0] = lo;
                    edgesVertexIndices[e + 3, 1] = lb;
                }
            }

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edgesVertexIndices, oneBased:false);
                        
            var gridLayer = new UnstructuredGridLayer { Grid = grid };

            var map = new Map { Layers = { gridLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        public void ShowIvoorkustGridLayer()
        {
            var mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var m = new WaterFlowFMModel(mduPath);
            var grid = m.Grid;

            var gridLayer = new UnstructuredGridLayer {Grid = grid};

            var map = new Map { Layers = { gridLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Ignore("too big, local data")]
        [Category(TestCategory.Performance)]
        [Category("ToCheck")]
        public void ProfileElbeGridLayer()
        {
            var grid = NetFileImporter.ImportGrid(@"D:\schre_tn\Desktop\Elbe\elbe_big_net.nc");
            var gridLayer = new UnstructuredGridLayer {Grid = grid};

            var map = new Map {Layers = {gridLayer}, Size = new Size {Width = 800, Height = 800}};
            map.ZoomToExtents();
            map.Render();

            TestHelper.AssertIsFasterThan(10000, () =>
                {
                    for (var i = 0; i < 5; i++)
                    {
                        gridLayer.RenderRequired = true;
                        map.Render();
                    }
                });
        }

        [Test]
        [Ignore("too big, local data")]
        [Category(TestCategory.Performance)]
        [Category("ToCheck")]
        public void ProfileElbeGridLayerExtensive()
        {
            // to make sure OpenGL etc have been loaded into the process already (takes up quite some mbs)
            RenderBendProfGridOnce();

            var grid = NetFileImporter.ImportGrid(@"D:\schre_tn\Desktop\Elbe\elbe_big_net.nc");

            TestHelper.AssertIsFasterThan(40000, () =>
                {
                    Map map = null;
                    UnstructuredGridLayer gridLayer = null;
                    const int numTimes = 10;
                    var stopwatch = new Stopwatch();

                    var elapsed = new long[numTimes];
                    var mem = new long[numTimes];
                    for (var i = 0; i < numTimes; i++)
                    {
                        var before = GC.GetTotalMemory(true);
                        stopwatch.Restart();

                        if (i == 0)
                        {
                            gridLayer = new UnstructuredGridLayer { Grid = grid };
                            map = new Map { Layers = { gridLayer }, Size = new Size { Width = 800, Height = 800 } };
                            map.ZoomToExtents();
                            map.Zoom *= 2; //zoom out
                        }
                        else
                        {
                            map.Zoom /= 1.5; // zoom in
                            gridLayer.RenderRequired = true;
                        }
                        map.Render();

                        stopwatch.Stop();
                        elapsed[i] = stopwatch.ElapsedMilliseconds;
                        mem[i] = GC.GetTotalMemory(true) - before;
                    }

                    // log results:
                    var line0 = ""; var line1 = ""; var line2 = "";
                    for (int i = 0; i < numTimes; i++)
                    {
                        line0 = string.Format("{0} {1,8}", line0, i);
                        line1 = string.Format("{0} {1,6}ms", line1, elapsed[i]);
                        line2 = string.Format("{0} {1,6}kb", line2, mem[i]/1024);
                    }

                    Console.WriteLine(line0);
                    Console.WriteLine(line1);
                    Console.WriteLine(line2);
                });
        }

        [Test]
        public void CreateUnstructuredGridCellForPointOnGetFeatures()
        {
            var map = new Map{ Zoom = 100.0 };
            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 100, 100);
            var layer = new UnstructuredGridLayer {Grid = grid, Map = map};

            var features = layer.GetFeatures(new Point(50, 150)).ToList(); // cell 10
            Assert.AreEqual(1, features.Count);

            var gridCell = (UnstructuredGridFeature)features[0];
            Assert.AreEqual(10, gridCell.Index);
            Assert.AreEqual(grid, gridCell.UnstructuredGrid);
            Assert.AreEqual(grid.Cells[10].ToPolygon(grid), gridCell.Geometry);
        }

        private static void RenderBendProfGridOnce()
        {
            var ncPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bend1_net.nc");
            var grid = NetFileImporter.ImportGrid(ncPath);
            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var map = new Map {Layers = {gridLayer}, Size = new Size {Width = 800, Height = 800}};
            map.ZoomToExtents();
            map.Render();
        }
    }
}