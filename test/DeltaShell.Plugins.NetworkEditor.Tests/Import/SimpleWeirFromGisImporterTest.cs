using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.X86)]
    public class SimpleWeirFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private SimpleWeirFromGisImporter simpleWeirImporter;

        [SetUp]
        public void SetUp()
        {
            channelImporter = new ChannelFromGisImporter();
            channelImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            channelImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            channelImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
            channelImporter.HydroRegion = new HydroNetwork();

            simpleWeirImporter = new SimpleWeirFromGisImporter();
            simpleWeirImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            simpleWeirImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            simpleWeirImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
        }

        [Test]
        public void ImportSimpleWeirFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("testdataBase_CF.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var weirImporterSettings = simpleWeirImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = path;
            channelImporterSettings.TableName = "Channel";
            channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            PropertyMapping propertyMapping;

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVK_Name";


            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            //SimpleWeir

            weirImporterSettings.Path = path;
            weirImporterSettings.TableName = "Weir";
            weirImporterSettings.DiscriminatorColumn = "TYPE";
            weirImporterSettings.DiscriminatorValue = "vast";
            weirImporterSettings.ColumnNameID = "KWKIDENT";
            weirImporterSettings.GeometryColumn = new MappingColumn("Weir", "Shape");
            simpleWeirImporter.HydroRegion = hydroNetwork;

            propertyMapping = weirImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Weir";
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";

            propertyMapping = weirImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Weir";
            propertyMapping.MappingColumn.ColumnName = "KWK_NAME";

            propertyMapping = weirImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Crest Level");
            propertyMapping.MappingColumn.TableName = "Weir";
            propertyMapping.MappingColumn.ColumnName = "CREST_LVL";

            propertyMapping = weirImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Crest Width");
            propertyMapping.MappingColumn.TableName = "Weir";
            propertyMapping.MappingColumn.ColumnName = "CREST_WDTH";

            propertyMapping = weirImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Discharge Coefficient");
            propertyMapping.MappingColumn.TableName = "Weir";
            propertyMapping.MappingColumn.ColumnName = "DIS_COEF";

            hydroNetwork = (HydroNetwork)simpleWeirImporter.ImportItem(null);

            Assert.AreEqual(2, hydroNetwork.Weirs.Count());

            var weir = (Weir)hydroNetwork.Weirs.First(w => w.Name == "KST_1");

            Assert.AreEqual(0.95, weir.CrestWidth);
            Assert.AreEqual(-1.49, weir.CrestLevel);
            Assert.AreEqual(0.99, ((SimpleWeirFormula)weir.WeirFormula).DischargeCoefficient);
        }

        [Test]
        public void ImportingWithDuplicatesShouldPreventDuplicatesGettingIntoNetworkTools9784()
        {
            var shapefilesFolderPath = TestHelper.GetTestFilePath("shapefile_Tools9784");
            var branchShapeFileFilePath = Path.Combine(shapefilesFolderPath, "shapes2_network_Branches.shp");
            var weirShapeFileFilePath = Path.Combine(shapefilesFolderPath, "shapes2_network_Weirs.shp"); // This file contains Weir1, Weir3 and Weir4 2x

            channelImporter.FeatureFromGisImporterSettings.Path = branchShapeFileFilePath;
            channelImporter.FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == "Name").MappingColumn.ColumnName = "Name";

            simpleWeirImporter.FeatureFromGisImporterSettings.Path = weirShapeFileFilePath;
            simpleWeirImporter.FeatureFromGisImporterSettings.PropertiesMapping.First(p => p.PropertyName == "Name").MappingColumn.ColumnName = "Name";

            simpleWeirImporter.HydroRegion = (HydroNetwork)channelImporter.ImportItem(null);

            var hydroNetwork = (HydroNetwork)simpleWeirImporter.ImportItem(null);
            var channel1 = hydroNetwork.Channels.First(c => c.Name == "Channel1");
            Assert.AreEqual(3, channel1.Weirs.Count());
            Assert.AreEqual("Weir3", channel1.Weirs.ElementAt(0).Name);
            Assert.AreEqual("Weir4", channel1.Weirs.ElementAt(1).Name);
            Assert.AreEqual("Weir1", channel1.Weirs.ElementAt(2).Name);

            Assert.IsFalse(hydroNetwork.Channels.SelectMany(c => c.BranchFeatures.Select(bf => bf.Name)).
                                        GroupBy(name => name).
                                        Any(nameGroup => nameGroup.Count() > 1), "There should be no duplicates.");
        }
    }
}
