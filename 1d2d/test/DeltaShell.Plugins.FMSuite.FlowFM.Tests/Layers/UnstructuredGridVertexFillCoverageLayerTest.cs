﻿using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools.Decorations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Layers
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class UnstructuredGridVertexFillCoverageLayerTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmptyVertexFillCoverageLayer()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFile = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFile);

            // build coverage and show on map
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false);

            var gridLayer = new UnstructuredGridLayer { Grid = grid };
            var coverageLayer = new UnstructuredGridVertexCoverageLayer { Coverage = bathymetry, RenderMode = RenderModeVertex.FillSmooth };

            var map = new Map { Layers = { gridLayer, coverageLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowVertexFillCoverageLayer()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFile = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFile);

            var values = grid.Vertices.Select(v => v.Z);

            // build coverage and show on map
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false);
            bathymetry.SetValues(values);

            var gridLayer = new UnstructuredGridLayer { Grid = grid };
            var coverageLayer = new UnstructuredGridVertexCoverageLayer
                {
                    Coverage = bathymetry,
                    RenderMode = RenderModeVertex.FillSmooth,
                };

            var map = new Map { Layers = {gridLayer, coverageLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowVertexFillCoverageLayerManualGrid()
        {
            var coord1 = new Coordinate(0, 0, 1);
            var coord2 = new Coordinate(0, 10.5, 2);
            var coord3 = new Coordinate(10.5, 10, 3);
            var coord4 = new Coordinate(10, 0, 4);
            var vertices = new[] {coord1, coord2, coord3, coord4};

            var edges = new int[,] { {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3} };

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);

            var values = grid.Vertices.Select(v => v.Z);

            // build coverage and show on map
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false);
            bathymetry.SetValues(values);

            var gridLayer = new UnstructuredGridLayer { Grid = grid };
            var coverageLayer = new UnstructuredGridVertexCoverageLayer
            {
                Coverage = bathymetry,
                RenderMode = RenderModeVertex.FillSmooth,
            };
            //gridLayer, 
            var map = new Map { Layers = { coverageLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowVertexFillCoverageLayerPensioen()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\pensioen\pensioen.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFile = Path.Combine(Path.GetDirectoryName(mduPath), "pensioen_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFile);

            var values = grid.Vertices.Select(v => v.Z);

            // build coverage and show on map
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false);
            bathymetry.SetValues(values);

            var gridLayer = new UnstructuredGridLayer { Grid = grid };
            var coverageLayer = new UnstructuredGridVertexCoverageLayer
            {
                Coverage = bathymetry,
                RenderMode = RenderModeVertex.FillSmooth,
            };
            //gridLayer, 
            var map = new Map { Layers = { coverageLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void VerifySmoothFilledBathymetryIsFast()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            var grid = new UnstructuredGrid();
            using (var ugridFile = new UGridFile(netFilePath))
                ugridFile.SetUnstructuredGrid(grid);
            
            Assert.IsFalse(grid.IsEmpty);

            var values = grid.Vertices.Select(v => v.Z);

            // build coverage and show on map
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false);
            bathymetry.SetValues(values);

            var coverageLayer = new UnstructuredGridVertexCoverageLayer
            {
                Coverage = bathymetry,
                RenderMode = RenderModeVertex.FillSmooth,
            };
            var map = new Map { Layers = { coverageLayer }, Size = new Size { Width = 1000, Height = 1000 } };
            map.ZoomToExtents();
            map.Render(); //warmup
            coverageLayer.RenderRequired = true;

            TestHelper.AssertIsFasterThan(500, () => map.Render()); //50ms on my pc (TS)
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void VerifyPointFilledBathymetryIsFast()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            var grid = new UnstructuredGrid();
            using (var ugridFile = new UGridFile(netFilePath))
                ugridFile.SetUnstructuredGrid(grid);

            Assert.IsFalse(grid.IsEmpty);
            
            var values = grid.Vertices.Select(v => v.Z);

            // build coverage and show on map
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false);
            bathymetry.SetValues(values);

            var coverageLayer = new UnstructuredGridVertexCoverageLayer
            {
                Coverage = bathymetry,
                RenderTechnology = PrimitivesRenderer.Software,
                RenderMode = RenderModeVertex.Point,
            };
            var map = new Map { Layers = { coverageLayer }, Size = new Size { Width = 1000, Height = 1000 } };
            map.ZoomToExtents();
            map.Render(); //warmup
            coverageLayer.RenderRequired = true;

            TestHelper.AssertIsFasterThan(250, () => map.Render());
        }
    }
}