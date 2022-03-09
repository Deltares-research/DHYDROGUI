using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Feature;
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
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class UnstructuredGridLayerTest
    {
        [Test]
        public void ShowGridLayer()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};

            var map = new Map
            {
                Layers = {gridLayer},
                Size = new Size
                {
                    Width = 800,
                    Height = 800
                }
            };
            map.ZoomToExtents();

            var mapControl = new MapControl
            {
                Map = map,
                Dock = DockStyle.Fill
            };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        public void ShowLargerGridLayer()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "pensioen_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};

            var map = new Map
            {
                Layers = {gridLayer},
                Size = new Size
                {
                    Width = 800,
                    Height = 800
                }
            };
            map.ZoomToExtents();

            var mapControl = new MapControl
            {
                Map = map,
                Dock = DockStyle.Fill
            };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Ignore("big")]
        public void ShowHugeGridLayer()
        {
            const int ms = 1000;
            const int ns = 1000;
            const int numVertices = (ms + 1) * (ns + 1);

            var vertices = new List<Coordinate>(numVertices);
            var edgesVertexIndices = new int[ms * ns * 4, 2];

            var r = new Random();

            // generate random grid
            for (var n = 0; n <= ns; n++)
            {
                for (var m = 0; m <= ms; m++)
                {
                    vertices.Add(new Coordinate(m, n, r.Next(20)));
                }
            }

            for (var m = 0; m < ms; m++)
            {
                for (var n = 0; n < ns; n++)
                {
                    int c = (n * ms) + m;
                    int lb = c + n;
                    int rb = lb + 1;
                    int lo = rb + ms;
                    int ro = lo + 1;

                    int e = c * 4;
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

            UnstructuredGrid grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edgesVertexIndices, oneBased: false);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};

            var map = new Map
            {
                Layers = {gridLayer},
                Size = new Size
                {
                    Width = 800,
                    Height = 800
                }
            };
            map.ZoomToExtents();

            var mapControl = new MapControl
            {
                Map = map,
                Dock = DockStyle.Fill
            };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        public void ShowIvoorkustGridLayer()
        {
            string mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var m = new WaterFlowFMModel();
            m.ImportFromMdu(mduPath);

            UnstructuredGrid grid = m.Grid;

            var gridLayer = new UnstructuredGridLayer {Grid = grid};

            var map = new Map
            {
                Layers = {gridLayer},
                Size = new Size
                {
                    Width = 800,
                    Height = 800
                }
            };
            map.ZoomToExtents();

            var mapControl = new MapControl
            {
                Map = map,
                Dock = DockStyle.Fill
            };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        public void CreateUnstructuredGridCellForPointOnGetFeatures()
        {
            var map = new Map {Zoom = 100.0};
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 100, 100);
            var layer = new UnstructuredGridLayer
            {
                Grid = grid,
                Map = map
            };

            List<IFeature> features = layer.GetFeatures(new Point(50, 150)).ToList(); // cell 10
            Assert.AreEqual(1, features.Count);

            var gridCell = (UnstructuredGridFeature) features[0];
            Assert.AreEqual(10, gridCell.Index);
            Assert.AreEqual(grid, gridCell.UnstructuredGrid);
            Assert.AreEqual(grid.Cells[10].ToPolygon(grid), gridCell.Geometry);
        }
    }
}