using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Functions.Generic;
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
    [TestFixture, Apartment(ApartmentState.STA)]
    public class UnstructuredGridCellVectorCoverageLayerTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmptyCellVectorCoverageLayer()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFile = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFile);

            // build coverage and show on map
            var coverage = new UnstructuredGridCellCoverage(grid, true);
            coverage.Components.Add(new Variable<double>());

            var gridLayer = new UnstructuredGridLayer { Grid = grid };
            var coverageLayer = new UnstructuredGridCellVectorCoverageLayer { Coverage = coverage };

            var map = new Map { Layers = { gridLayer, coverageLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCellVectorCoverageLayer()
        {
            var r = new Random();

            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string mapFile = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFile);

            var valuesX = Enumerable.Range(0, grid.Cells.Count).Select(i => (double)r.Next(20));
            var valuesY = Enumerable.Range(0, grid.Cells.Count).Select(i => (double)r.Next(20));
            
            // build coverage and show on map
            var coverage = new UnstructuredGridCellCoverage(grid, false);
            coverage.Components.Add(new Variable<double>());
            coverage.Components[0].SetValues(valuesX);
            coverage.Components[1].SetValues(valuesY);

            var gridLayer = new UnstructuredGridLayer { Grid = grid };
            var coverageLayer = new UnstructuredGridCellVectorCoverageLayer { Coverage = coverage };

            var map = new Map { Layers = { gridLayer, coverageLayer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            mapControl.GetToolByType<LegendTool>().Visible = true;

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}