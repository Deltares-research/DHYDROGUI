using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Adapters;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Adaptors
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
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdapter(localCopyOfTestFile))
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
            var testDir = FileUtils.CreateTempDirectory();
            var localCopyOfTestFile = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (var gridApi = GridApiFactory.CreateNew())
            {
                gridApi.write_geom_ugrid(localCopyOfTestFile);
            }
            
            try
            {
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdapter(localCopyOfTestFile))
                {
                    var unstructuredGrid = fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1, true);
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