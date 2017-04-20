using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class Delft3DDepthFileImporterTest
    {
        private readonly string delft3DGridFile = TestHelper.GetTestFilePath("Noorderstrand.grd");
        private readonly string delft3DDepFile = TestHelper.GetTestFilePath("Noorderstrand.dep");
        
        [Test]
        public void ImportDepth()
        {
            var grid = Delft3DGridFileReader.Read(delft3DGridFile);
            var importer = new Delft3DDepthFileImporter("test");

            var targetBathy = new CurvilinearCoverage(grid);
            importer.ImportItem(delft3DDepFile, targetBathy);

            Assert.AreEqual(163 * 361, grid.GetValues().Count); // nx,* ny
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ImportAndShowDepth()
        {
            var grid = Delft3DGridFileReader.Read(delft3DGridFile);
            var importer = new Delft3DDepthFileImporter("test");

            var targetBathy = new CurvilinearCoverage(grid);
            importer.ImportItem(delft3DDepFile, targetBathy);

            var layer = new DiscreteGridPointCoverageLayer { Coverage = grid, ShowFaces = false, ShowLines = true, ShowVertices = false };
            var map = new Map {Layers = {layer}};
            MapTestHelper.ShowModal(map);
        }
    }
}
