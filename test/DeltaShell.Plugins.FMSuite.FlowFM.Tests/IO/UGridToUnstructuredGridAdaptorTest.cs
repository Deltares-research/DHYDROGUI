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
			var testDir = FileUtils.CreateTempDirectory();
            var localCopyOfTestFile = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (var gridApi = GridApiFactory.CreateNew())
            {
                gridApi.write_geom_ugrid(localCopyOfTestFile);
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