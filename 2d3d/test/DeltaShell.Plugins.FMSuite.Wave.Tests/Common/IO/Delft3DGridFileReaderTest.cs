using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class Delft3DGridFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReaderReadsDelft3DGrid()
        {
            string delft3DGridFile = TestHelper.GetTestFilePath("Noorderstrand.grd");

            CurvilinearGrid grid = Delft3DGridFileReader.Read(delft3DGridFile);

            Assert.AreEqual(361, grid.Size1);
            Assert.AreEqual(163, grid.Size2);
            Assert.AreEqual(163 * 361, grid.X.Values.Count);
            Assert.AreEqual(163 * 361, grid.Y.Values.Count);
        }
    }
}