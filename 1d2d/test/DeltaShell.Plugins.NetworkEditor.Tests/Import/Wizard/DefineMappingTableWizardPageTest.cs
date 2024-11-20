using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import.Wizard
{
    [TestFixture]
    public class DefineMappingTableWizardPageTest
    {
        private HydroRegionFromGisImporter importer;
        private DefineMappingTableWizardPage page;


        [SetUp]
        public void SetUp()
        {
            importer = new HydroRegionFromGisImporter();
            importer.FileBasedFeatureProviders.Add(new ShapeFile());
            importer.FileBasedFeatureProviders.Add(new OgrFeatureProvider());

            page = new DefineMappingTableWizardPage();
        }

        [TearDown]
        public void TearDown()
        {
            page.Dispose();
            page = null;
            importer = null;
        }


        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowDefineMappingTableWizardPageWithChannelAndWHCS()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");

            //CHANNELS
            var featureImporter = FeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter(typeof(ChannelFromGisImporter));
            featureImporter.FeatureFromGisImporterSettings.Path = path;
            featureImporter.FeatureFromGisImporterSettings.TableName = "Channel";
            SetPossibleMappingColumns(featureImporter, path, "Channel");
            importer.FeatureFromGisImporters.Add(featureImporter);

            //CROSSSECTION
            featureImporter = FeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter(typeof(CrossSectionZWFromGisImporter));
            featureImporter.FeatureFromGisImporterSettings.Path = path;
            featureImporter.FeatureFromGisImporterSettings.TableName = "Cross_section_definition";
            ((CrossSectionZWFromGisImporter)featureImporter).NumberOfLevels = 3;
            SetPossibleMappingColumns(featureImporter, path, "Cross_section_definition");
            importer.FeatureFromGisImporters.Add(featureImporter);
            
            page.HydroRegionFromGisImporter = importer;

            WindowsFormsTestHelper.ShowModal(page);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowDefineMappingTableWizardPageWithChannelAndWHCSShapeFileBased()
        {
            var pathChannels = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Channels.shp");
            var pathCrossSections = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/CrossSections.shp");

            //CHANNELS
            var featureImporter = NetworkFeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter(typeof(ChannelFromGisImporter));
            featureImporter.FeatureFromGisImporterSettings.Path = pathChannels;
            featureImporter.FeatureFromGisImporterSettings.TableName = "Channel";
            SetPossibleMappingColumns(featureImporter, pathChannels, "Channel");
            importer.FeatureFromGisImporters.Add(featureImporter);

            //CROSSSECTION
            featureImporter = NetworkFeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter(typeof(CrossSectionZWFromGisImporter));
            featureImporter.FeatureFromGisImporterSettings.Path = pathCrossSections;
            featureImporter.FeatureFromGisImporterSettings.TableName = "Cross_section_definition";
            ((CrossSectionZWFromGisImporter)featureImporter).NumberOfLevels = 3;
            SetPossibleMappingColumns(featureImporter, pathCrossSections, "Cross_section_definition");
            importer.FeatureFromGisImporters.Add(featureImporter);

            page.HydroRegionFromGisImporter = importer;

            WindowsFormsTestHelper.ShowModal(page);
        }

        private void SetPossibleMappingColumns(FeatureFromGisImporterBase featureImporter, string path, string tableName)
        {
            IList<string> lstColumnNames;
            List<MappingColumn> possibleMappingColumns = new List<MappingColumn>();
            var schemaReader = new ShapeFileSchemaReader() { Path = path };
            schemaReader.OpenConnection();

            lstColumnNames = schemaReader.GetColumnNames(tableName);
            foreach (var columnName in lstColumnNames)
            {
                possibleMappingColumns.Add(new MappingColumn(tableName, columnName));
            }
            featureImporter.PossibleMappingColumns.AddRange(possibleMappingColumns);

            schemaReader.CloseConnection();
        }
    }
}
