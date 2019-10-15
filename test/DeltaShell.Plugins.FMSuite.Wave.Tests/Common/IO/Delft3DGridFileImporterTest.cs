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
    class Delft3DGridFileImporterTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ImportGrid()
        {
            var delft3DGridFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(WaveModelTest).Assembly, "Noorderstrand.grd");

            var importer = new Delft3DGridFileImporter("test");
            var grid = importer.ImportItem(delft3DGridFile);

            var layer = new DiscreteGridPointCoverageLayer { Coverage = (DiscreteGridPointCoverage)grid };
            var map = new Map {Layers = {layer}};
            MapTestHelper.ShowModal(map);
        }
    }
}
