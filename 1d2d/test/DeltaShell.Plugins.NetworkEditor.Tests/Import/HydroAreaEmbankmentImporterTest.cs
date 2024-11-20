using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    class HydroAreaEmbankmentImporterTest
    {

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestImportHydroAreaEmbankments()
        {
            HydroArea area = new HydroArea();
            var path = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Channels.shp");

            HydroAreaEmbankmentImporter importer = new HydroAreaEmbankmentImporter();
            importer.ImportItem(path, area);

            Assert.That(area.Embankments.Count == 181);
        }
    }
}
