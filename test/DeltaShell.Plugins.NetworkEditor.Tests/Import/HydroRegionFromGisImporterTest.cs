using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.X86)]
    public class HydroRegionFromGisImporterTest
    {
        private HydroRegionFromGisImporter importer;

        [SetUp]
        public void SetUp()
         {
             importer = new HydroRegionFromGisImporter();
             importer.HydroRegion = new HydroNetwork();
         }

         [Test]
         public void OgrAndSQL()
         {
             var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
             var ogrFeatureProvider = new OgrFeatureProvider(path);
             ogrFeatureProvider.OpenLayerWithSQL("Select Cross_section_definition.*,locations.Shape FROM Cross_section_definition, locations WHERE locations.PROIDENT = Cross_section_definition.PROIDENT");

             Assert.AreEqual(349, ogrFeatureProvider.Features.Count);
         }

         [Test]
         public void OgrAndSQLWithRelatedTable()
         {
             var path = TestHelper.GetTestFilePath("testdatabase_CF.mdb");
             var sql =
                 "SELECT Cross_section_definition.*, [locations.SHAPE],[locations.SOURCE] AS 'datasource' FROM Cross_section_definition, [locations] WHERE [locations.PROIDENT] = [Cross_section_definition.PROIDENT]";
             var ogrFeatureProvider = new OgrFeatureProvider(path);
             ogrFeatureProvider.OpenLayerWithSQL(sql);

             Assert.AreEqual(16, ogrFeatureProvider.Features.Count);
         }

         [Test]
         public void OgrFeatureProviderOpenWithSQLTest()
         {
             var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
             var provider = new OgrFeatureProvider();
             var sql = "SELECT Cross_section_definition.*, [locations.Shape] FROM Cross_section_definition, locations WHERE [Cross_section_definition.TYPE] = 'tabulated' AND [locations.PROIDENT] = [Cross_section_definition.PROIDENT]";
             provider.Path = path;
             provider.OpenLayerWithSQL(sql);

             var features = provider.Features;

             Assert.AreEqual("No Error", "No Error");
         }
    }
}
