using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using DeltaShell.Plugins.FMSuite.Common.IO.Writers;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class Delft3DGridFileWriterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteGridFileTest()
        {
            string gridFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\outer.grd");
            var targetPath = "testwriter.grd";

            CurvilinearGrid grid = Delft3DGridFileReader.Read(gridFilePath);
            Delft3DGridFileWriter.Write(grid, targetPath);
            CurvilinearGrid reReadGrid = Delft3DGridFileReader.Read(targetPath);

            Assert.AreEqual(grid.Size1, reReadGrid.Size1);
            Assert.AreEqual(grid.Size2, reReadGrid.Size2);
            Assert.AreEqual(grid.Attributes[CurvilinearGrid.CoordinateSystemKey],
                            reReadGrid.Attributes[CurvilinearGrid.CoordinateSystemKey]);

            for (var i = 0; i < grid.X.Values.Count; ++i)
            {
                Assert.AreEqual(grid.X.Values[i], reReadGrid.X.Values[i]);
                Assert.AreEqual(grid.Y.Values[i], reReadGrid.Y.Values[i]);
            }
        }
    }
}