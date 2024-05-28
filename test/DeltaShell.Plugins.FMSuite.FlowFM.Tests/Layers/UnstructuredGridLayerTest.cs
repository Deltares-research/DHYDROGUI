using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
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
            string mapFile = Path.Combine(Path.GetDirectoryName(mduPath), "bendprof_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFile);

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
            string mapFile = Path.Combine(Path.GetDirectoryName(mduPath), "pensioen_map.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(mapFile);

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
    }
}