using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Adaptors;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{

    [TestFixture]
    public class GeometryZipExporterTest
    {
        [Test]
        public void TestWriteZValuesToNetFile()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (var gridApi = GridApiFactory.CreateNew())
            {
                Assert.AreEqual(gridApi.GetConvention(netFilePath), GridApiDataSet.DataSetConventions.IONC_CONV_OTHER);
            }
            var grid = NetFileImporter.ImportGrid(netFilePath);

            var currentZValues = grid.Vertices.Select(v => v.Z);
            var newZValues = currentZValues.Select(z => { z = 123.456; return z; }).ToArray();

            NetFile.WriteZValues(netFilePath, newZValues);

            var adjustedGrid = NetFileImporter.ImportGrid(netFilePath);
            var zValues = adjustedGrid.Vertices.Select(v => v.Z);
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        public void TestWriteZValuesToNetFile_UGrid()
        {
            var netFilePath = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            netFilePath = TestHelper.CreateLocalCopySingleFile(netFilePath);

            using (var gridApi = GridApiFactory.CreateNew())
            {
                Assert.AreEqual(gridApi.GetConvention(netFilePath), GridApiDataSet.DataSetConventions.IONC_CONV_UGRID);
            }

            // get original grid
            UnstructuredGrid grid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdaptor(netFilePath))
            {
                grid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }
            Assert.NotNull(grid);

            // generate new z values
            var currentZValues = grid.Vertices.Select(v => v.Z);
            var newZValues = currentZValues.Select(z => { z = 123.456; return z; }).ToArray();
            
            // write new coordinates to netfile
            using (var uGrid = new UGrid(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.WriteZValues(1, newZValues);
            }

            // read new grid
            UnstructuredGrid adjustedGrid;
            using (var uGridAdaptor = new UGridToUnstructuredGridAdaptor(netFilePath))
            {
                adjustedGrid = uGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
            }
            Assert.NotNull(adjustedGrid);

            // compare z values
            var zValues = adjustedGrid.Vertices.Select(v => v.Z);
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

    }
}
