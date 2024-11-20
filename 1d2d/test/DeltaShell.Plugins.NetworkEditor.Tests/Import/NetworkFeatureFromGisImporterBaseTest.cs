using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class NetworkFeatureFromGisImporterBaseTest
    {
        private static readonly MockRepository mocks = new MockRepository();
        private TestNetworkFeatureFromGisImporterBase testImporter;

        [SetUp]
        public void SetUp()
        {
           testImporter = new TestNetworkFeatureFromGisImporterBase();
           testImporter.FeatureFromGisImporterSettings.Path = "dummy.mdb";

           testImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
        }

        [Test]
        public void GetSQLFromPropertiesMapping()
        {
            var sql = "SELECT [child.childName] AS child_childName FROM [parent],[child] WHERE [child.parentID] = [parent.ID]";

            testImporter.FeatureFromGisImporterSettings.TableName = "parent";
            testImporter.FeatureFromGisImporterSettings.ColumnNameID = "ID";
            testImporter.FeatureFromGisImporterSettings.RelatedTables.Add(new RelatedTable("child", "parentID"));

            var propertyMapping = new PropertyMapping("childProperty");
            propertyMapping.MappingColumn.TableName = "child";
            propertyMapping.MappingColumn.ColumnName = "childName";

            testImporter.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMapping);

            GetSQLFromPropertiesMappingTest(sql);

        }

        [Test]
        public void GetSQLFromPropertiesMappingWithDiscriminator()
        {
            var sql = "SELECT * FROM [parent] WHERE [parent.Type] = 'Mother'";

            testImporter.FeatureFromGisImporterSettings.TableName = "parent";
            testImporter.FeatureFromGisImporterSettings.DiscriminatorColumn = "Type";
            testImporter.FeatureFromGisImporterSettings.DiscriminatorValue = "Mother";

            GetSQLFromPropertiesMappingTest(sql);

        }

        [Test]
        public void GetSQLFromPropertiesMappingWithDiscriminatorAndRelation()
        {
            var sql = "SELECT [child.childName] AS child_childName FROM [parent],[child] WHERE [parent.Type] = 'Mother' AND [child.parentID] = [parent.ID]";

            testImporter.FeatureFromGisImporterSettings.TableName = "parent";
            testImporter.FeatureFromGisImporterSettings.ColumnNameID = "ID";
            testImporter.FeatureFromGisImporterSettings.DiscriminatorColumn = "Type";
            testImporter.FeatureFromGisImporterSettings.DiscriminatorValue = "Mother";
            testImporter.FeatureFromGisImporterSettings.RelatedTables.Add(new RelatedTable("child", "parentID"));

            var propertyMapping = new PropertyMapping("childProperty");
            propertyMapping.MappingColumn.TableName = "child";
            propertyMapping.MappingColumn.ColumnName = "childName";

            testImporter.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMapping);

            GetSQLFromPropertiesMappingTest(sql);

        }

        private void GetSQLFromPropertiesMappingTest(string expectedSQL)
        {
            var ogrFeatureProvider = mocks.StrictMock<OgrFeatureProvider>();

            Expect.Call(() => ogrFeatureProvider.OpenLayerWithSQL(expectedSQL));

            Expect.Call(() => ogrFeatureProvider.Open(null)).IgnoreArguments();
            Expect.Call(ogrFeatureProvider.Close);
            Expect.Call(ogrFeatureProvider.Features).Return(null);
            Expect.Call(ogrFeatureProvider.FileFilter).Return("*.mdb");

            mocks.ReplayAll();

            testImporter.FileBasedFeatureProviders.Add(ogrFeatureProvider);
            testImporter.TestGetFeatures();

            mocks.VerifyAll();
        }

        [Test]
        public void FilteringFeaturesWorks()
        {
            var featureProvider = mocks.StrictMock<IFileBasedFeatureProvider>();
            var feature1 = mocks.StrictMock<IFeature>();
            var feature2 = mocks.StrictMock<IFeature>();

            var attributes1 = new DictionaryFeatureAttributeCollection();
            attributes1["Type"] = "Apple";
            var attributes2 = new DictionaryFeatureAttributeCollection();
            attributes2["Type"] = "Orange";

            Expect.Call(feature1.Attributes).Return(attributes1).Repeat.Any();
            Expect.Call(feature2.Attributes).Return(attributes2).Repeat.Any();

            Expect.Call(() => featureProvider.Open(null)).IgnoreArguments();
            Expect.Call(featureProvider.Features).Return(new[] { feature1, feature2 });
            Expect.Call(featureProvider.FileFilter).Return("*.mdb");
            Expect.Call(featureProvider.Close);
            
            mocks.ReplayAll();
            
            testImporter.FeatureFromGisImporterSettings.DiscriminatorColumn = "Type";
            testImporter.FeatureFromGisImporterSettings.DiscriminatorValue = "Apple";

            testImporter.FileBasedFeatureProviders.Add(featureProvider);

            var returnedFeatures = testImporter.TestGetFeatures();
            
            Assert.AreEqual(1, returnedFeatures.Count);
            Assert.AreEqual(feature1, returnedFeatures.OfType<IFeature>().First());

            mocks.VerifyAll();

        }

        public class TestNetworkFeatureFromGisImporterBase : NetworkFeatureFromGisImporterBase
        {
            public override string Name
            {
                get { return "TestNetworkFeatureFromGisImporterBase"; }
            }

            public override object ImportItem(string path, object target = null)
            {
                throw new NotImplementedException();
            }

            public IList<IFeature> TestGetFeatures()
            {
                return GetFeatures();
            }
        }
    }
}
