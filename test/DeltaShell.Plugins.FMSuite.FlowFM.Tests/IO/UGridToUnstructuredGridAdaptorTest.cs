using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Adaptors;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class UGridToUnstructuredGridAdaptorTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
		public void GivenNcGridFileWhenGetUgridForFMThenNoException()
        {
            var testFile = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFile));

            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFile);

            try
            {
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdaptor(localCopyOfTestFile))
                {
                    var unstructuredGrid = fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
                    Assert.That(unstructuredGrid, Is.Not.Null);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
		}

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestGetUnstructuredGridFromUGridMeshId_WithExportedNetGeomFile()
        {
            /*
               Note:
                    writing net_geom currently produces an 'invalid' UGrid file since the start_index attribute is not written to the following variables:
                    mesh2d_edge_faces, mesh2d_edge_nodes, mesh2d_face_nodes
                
                    the api call ionc_write_geom_ugrid is not actually being used yet in DeltaShell... only in tests
             */

            var testDir = FileUtils.CreateTempDirectory();
            var localCopyOfTestFile = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (var gridApi = GridApiFactory.CreateNew())
            {
                gridApi.ionc_write_geom_ugrid(localCopyOfTestFile);
            }
            
            try
            {
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdaptor(localCopyOfTestFile))
                {
                    var unstructuredGrid = fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
                    Assert.That(unstructuredGrid, Is.Not.Null);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
        }
    }
}