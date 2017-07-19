using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Adaptors;
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
            var netFilePath = TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (var gridApi = GridApiFactory.CreateNew())
            {
                GridApiDataSet.DataSetConventions convention;
                var ierr = gridApi.GetConvention(netFilePath, out convention);
                Assert.That(convention, Is.EqualTo(GridApiDataSet.DataSetConventions.CONV_OTHER));
            }
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

            using (var gridApi = GridApiFactory.CreateNew())
            {
                GridApiDataSet.DataSetConventions convention;
                var ierr = gridApi.GetConvention(netFilePath, out convention);
                Assert.That(convention, Is.EqualTo(GridApiDataSet.DataSetConventions.CONV_UGRID));
            }

            // get original grid
            UnstructuredGrid grid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdaptor(netFilePath))
            {
                grid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }
            Assert.NotNull(grid);

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
            using (var uGrid = new UGrid(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.RewriteGridCoordinatesForMeshId(1, grid.Vertices.Select(v => v.X).ToArray(), grid.Vertices.Select(v => v.Y).ToArray());
            }

            // read new grid
            UnstructuredGrid adjustedGrid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdaptor(netFilePath))
            {
                adjustedGrid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }
            Assert.NotNull(adjustedGrid);

            // compare to new value
            var reloadedX = adjustedGrid.Vertices[0].X;
            Assert.AreEqual(newX, reloadedX);
        }
    }
}
