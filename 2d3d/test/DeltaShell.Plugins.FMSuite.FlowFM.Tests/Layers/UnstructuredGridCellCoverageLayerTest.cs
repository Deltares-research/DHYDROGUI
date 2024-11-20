using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools.Decorations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Layers
{
    [TestFixture]
    public class UnstructuredGridCellCoverageLayerTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmptyCellCoverageLayer()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            // build coverage and show on map
            var coverage = new UnstructuredGridCellCoverage(grid, true);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var coverageLayer = new UnstructuredGridCellCoverageLayer {Coverage = coverage};

            var map = new Map
            {
                Layers =
                {
                    gridLayer,
                    coverageLayer
                },
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

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCellCoverageLayer()
        {
            var r = new Random();

            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            IEnumerable<double> values = Enumerable.Range(0, grid.Cells.Count).Select(i => (double) r.Next());

            // build coverage and show on map
            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage.SetValues(values);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var coverageLayer = new UnstructuredGridCellCoverageLayer {Coverage = coverage};

            var map = new Map
            {
                Layers =
                {
                    gridLayer,
                    coverageLayer
                },
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

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSmoothCellCoverageLayer()
        {
            var r = new Random();

            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            IEnumerable<double> values = Enumerable.Range(0, grid.Cells.Count).Select(i => (double) r.Next());

            // build coverage and show on map
            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage.SetValues(values);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var coverageLayer = new UnstructuredGridCellCoverageLayer
            {
                Coverage = coverage,
                OptimizeRendering = true,
                RenderMode = RenderModeCell.Smooth
            };

            var map = new Map
            {
                Layers =
                {
                    gridLayer,
                    coverageLayer
                },
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

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCellPointCoverageLayer()
        {
            var r = new Random();

            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            IEnumerable<double> values = Enumerable.Range(0, grid.Cells.Count).Select(i => (double) r.Next());

            // build coverage and show on map
            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage.SetValues(values);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var coverageLayer = new UnstructuredGridCellCoverageLayer
            {
                Coverage = coverage,
                RenderMode = RenderModeCell.Point
            };

            var map = new Map
            {
                Layers =
                {
                    gridLayer,
                    coverageLayer
                },
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

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}