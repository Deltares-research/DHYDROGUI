using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    internal class HydroAreaEmbankmentImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestImportHydroAreaEmbankments()
        {
            var area = new HydroArea();
            string path = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Channels.shp");

            var importer = new HydroAreaEmbankmentImporter();
            importer.ImportItem(path, area);

            Assert.That(area.Embankments.Count == 181);
        }
    }
}