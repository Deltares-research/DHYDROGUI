using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Adapters;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;
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
            string netFilePath = TestHelper.GetTestFilePath(@"harlingen\FilesUsingOldFormat\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                GridApiDataSet.DataSetConventions convention;
                gridApi.GetConvention(netFilePath, out convention);
                Assert.That(convention, Is.EqualTo(GridApiDataSet.DataSetConventions.CONV_OTHER));
            }

            UnstructuredGrid grid = NetFileImporter.ImportGrid(netFilePath);

            grid.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);
            double originalX = grid.Vertices[0].X;

            var factory = new OgrCoordinateSystemFactory();
            ICoordinateSystem mercator = factory.CreateFromEPSG(3857);
            WaterFlowFMModelCoordinateConversion.ConvertGrid(grid, new OgrCoordinateSystemFactory().CreateTransformation(
                                                                 grid.CoordinateSystem, mercator));

            double newX = grid.Vertices[0].X;

            Assert.AreNotEqual(originalX, newX);

            NetFile.RewriteGridCoordinates(netFilePath, grid);

            UnstructuredGrid adjustedGrid = NetFileImporter.ImportGrid(netFilePath);
            double reloadedX = adjustedGrid.Vertices[0].X;

            Assert.AreEqual(newX, reloadedX);
        }

        [Test]
        public void ConvertXYCoordinates_UGrid()
        {
            string netFilePath = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                GridApiDataSet.DataSetConventions convention;
                gridApi.GetConvention(netFilePath, out convention);
                Assert.That(convention, Is.EqualTo(GridApiDataSet.DataSetConventions.CONV_UGRID));
            }

            // get original grid
            UnstructuredGrid grid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(netFilePath))
            {
                grid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }

            Assert.NotNull(grid);

            // get original value
            grid.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);
            double originalX = grid.Vertices[0].X;

            // convert coordinate system
            var factory = new OgrCoordinateSystemFactory();
            ICoordinateSystem mercator = factory.CreateFromEPSG(3857);
            WaterFlowFMModelCoordinateConversion.ConvertGrid(grid, new OgrCoordinateSystemFactory().CreateTransformation(
                                                                 grid.CoordinateSystem, mercator));

            // get new value
            double newX = grid.Vertices[0].X;
            Assert.AreNotEqual(originalX, newX);

            // write new coordinates to netfile
            using (var uGrid = new UGrid(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.RewriteGridCoordinatesForMeshId(1, grid.Vertices.Select(v => v.X).ToArray(), grid.Vertices.Select(v => v.Y).ToArray());
            }

            // read new grid
            UnstructuredGrid adjustedGrid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(netFilePath))
            {
                adjustedGrid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }

            Assert.NotNull(adjustedGrid);

            // compare to new value
            double reloadedX = adjustedGrid.Vertices[0].X;
            Assert.AreEqual(newX, reloadedX);
        }
    }
}