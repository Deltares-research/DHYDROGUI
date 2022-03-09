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
using SharpMap.Rendering;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools.Decorations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Layers
{
    [TestFixture]
    public class UnstructuredGridVertexCoverageLayerTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmptyVertexCoverageLayer()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            // build coverage and show on map
            var coverage = new UnstructuredGridVertexCoverage(grid, true);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var coverageLayer = new UnstructuredGridVertexCoverageLayer {Coverage = coverage};

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
        public void ShowVertexCoverageLayer()
        {
            var r = new Random();

            string ncPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bend1_net.nc");
            UnstructuredGrid grid = NetFileImporter.ImportGrid(ncPath);

            IEnumerable<double> values = Enumerable.Range(0, grid.Vertices.Count).Select(i => (double) r.Next());

            // build coverage and show on map
            var coverage = new UnstructuredGridVertexCoverage(grid, false);
            coverage.SetValues(values);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var coverageLayer = new UnstructuredGridVertexCoverageLayer
            {
                Coverage = coverage,
                RenderTechnology = PrimitivesRenderer.Software
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
        public void ShowVertexCoverageLayerWithLabels()
        {
            var r = new Random();

            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFilePath = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFilePath);

            IEnumerable<double> values = Enumerable.Range(0, grid.Vertices.Count).Select(i => r.NextDouble() * 300.0);

            // build coverage and show on map
            var coverage = new UnstructuredGridVertexCoverage(grid, false);
            coverage.SetValues(values);

            var gridLayer = new UnstructuredGridLayer {Grid = grid};
            var coverageLayer = new UnstructuredGridVertexCoverageLayer
            {
                Coverage = coverage,
                RenderTechnology = PrimitivesRenderer.Software, //todo: software (for build server)
                RenderMode = RenderModeVertex.ColoredNumbers
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