using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelCoordinateConversionTest
    {
        [Test]
        public void ConvertXYCoordinates()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"harlingen\FilesUsingOldFormat\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            Assert.IsFalse(UGridFileHelper.IsUGridFile(netFilePath));
            var grid = NetFileImporter.ImportGrid(netFilePath);
            
            grid.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);
            var originalX = grid.Vertices[0].X;

            var factory = new OgrCoordinateSystemFactory();
            var mercator = factory.CreateFromEPSG(3857);
            WaterFlowFMModelCoordinateConversion.ConvertGrid(grid, new OgrCoordinateSystemFactory().CreateTransformation(
                grid.CoordinateSystem, mercator));

            var newX = grid.Vertices[0].X;

            Assert.AreNotEqual(originalX, newX);

            NetFile.RewriteGridCoordinates(netFilePath, grid);

            var adjustedGrid = NetFileImporter.ImportGrid(netFilePath);
            var reloadedX = adjustedGrid.Vertices[0].X;

            Assert.AreEqual(newX, reloadedX);
        }

        [Test]
        public void ConvertXYCoordinates_UGrid()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            Assert.IsTrue(UGridFileHelper.IsUGridFile(netFilePath));

            // get original grid
            var grid = new UnstructuredGrid();
            UGridFileHelper.SetUnstructuredGrid(netFilePath, grid);
            Assert.IsFalse(grid.IsEmpty);

            // get original value
            grid.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);
            var originalX = grid.Vertices[0].X;

            // convert coordinate system
            var factory = new OgrCoordinateSystemFactory();
            var mercator = factory.CreateFromEPSG(3857);
            WaterFlowFMModelCoordinateConversion.ConvertGrid(grid, new OgrCoordinateSystemFactory().CreateTransformation(
                grid.CoordinateSystem, mercator));

            // get new value
            var newX = grid.Vertices[0].X;
            Assert.AreNotEqual(originalX, newX);

            // write new coordinates to netfile
            UGridFileHelper.RewriteGridCoordinates(netFilePath, grid);

            // read new grid
            var adjustedGrid = new UnstructuredGrid();
            UGridFileHelper.SetUnstructuredGrid(netFilePath, adjustedGrid);
            Assert.IsFalse(adjustedGrid.IsEmpty);

            // compare to new value
            var reloadedX = adjustedGrid.Vertices[0].X;
            Assert.AreEqual(newX, reloadedX);
        }
    }
}
