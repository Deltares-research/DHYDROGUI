using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Adapters;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;
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
            string testFile = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFile));

            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFile);

            try
            {
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdapter(localCopyOfTestFile))
                {
                    UnstructuredGrid unstructuredGrid = fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
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
            string testDir = FileUtils.CreateTempDirectory();
            string localCopyOfTestFile = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                gridApi.write_geom_ugrid(localCopyOfTestFile);
            }

            try
            {
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdapter(localCopyOfTestFile))
                {
                    UnstructuredGrid unstructuredGrid = fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1, true);
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