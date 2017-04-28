using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class Delft3DDepthFileExporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteDepth()
        {
            var grid = new CurvilinearCoverage();
            var x = new double[20*4];
            var y = new double[20*4];
            var values = new double[20*4];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = i;
                y[i] = i;
                values[i] = i;
            }
            grid.Resize(20, 4, x, y);
            grid.SetValues(values);

            var exportPath = TestHelper.GetDataDir() + "TestDepth.dep";

            // Export the file
            Assert.AreEqual(true, new Delft3DDepthFileExporter().Export(grid, exportPath));
            var importedValues = Delft3DDepthFileReader.Read(exportPath, grid.Size1, grid.Size2);
            Assert.AreEqual(values, importedValues);
        }
    }
}