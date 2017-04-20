using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    public class ShapeFileReaderAndWriterTest
    {
        const string OutputFolder = "TestFiles";

        private List<DelftTools.Utils.Tuple<IGeometry,IDictionary<string,object>>> featureCollection;
        private string fileName;
        private string filePath;
        private string shpFile;
        private string dbfFile;
        private string shxFile;
        private string shpFilePath;
        private string shxFilePath;
        private string dbfFilePath;

        #region Setup

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            bool dirExists = false;

            if (Directory.Exists(OutputFolder))
            {
                try
                {
                    Directory.Delete(OutputFolder, true);
                }
                catch
                {
                    dirExists = true;
                }
            }

            if (!dirExists)
                Directory.CreateDirectory(OutputFolder);            
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
            fileName = "TestFile_" + Guid.NewGuid().ToString().Replace("-", "_");
            shpFile = fileName + ".shp";
            dbfFile = fileName + ".dbf";
            shxFile = fileName + ".shx";

            shpFilePath = Path.Combine(OutputFolder, this.shpFile);
            shxFilePath = Path.Combine(OutputFolder, this.shxFile);
            dbfFilePath = Path.Combine(OutputFolder, this.dbfFile);

            if (File.Exists(shpFilePath)) File.Delete(shpFilePath);
            if (File.Exists(shxFilePath)) File.Delete(shxFilePath);
            if (File.Exists(dbfFilePath)) File.Delete(dbfFilePath);

            featureCollection = new List<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>>();
        }

        [TearDown]
        public void TestTearDown()
        {
            if (File.Exists(shpFilePath)) File.Delete(shpFilePath);
            if (File.Exists(shxFilePath)) File.Delete(shxFilePath);
            if (File.Exists(dbfFilePath))
            {
                // disposing the dbf file could take longer
                for (int i = 0; i < 3; i++)
                    try
                    {
                        File.Delete(dbfFilePath);
                    }
                    catch (IOException)
                    {
                        System.Threading.Thread.Sleep(200);
                    }
            }
        }

        #endregion

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateFile_ValidPathAndFeaturesArgumentIsNull_Throws()
        {
            ShapeFileWriter.Create(@"", fileName, null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateFile_ValidPathAndFeaturesArgumentIsEmpty_Throws()
        {
            ShapeFileWriter.Create(@"", fileName, featureCollection);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateFile_InvalidName_Throws()
        {
            ShapeFileWriter.Create(@"", "", featureCollection);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateFile_NoFeatureDataToWrite_Throws()
        {
            ShapeFileWriter.Create(@"", fileName, featureCollection);
        }

        
        [Test]
        [Ignore(TestCategory.WorkInProgress)]
        [ExpectedException(typeof(NotSupportedException))]        
        public void CreateFile_FeatureCollectionContainsNotSupportedFeature_Throws()
        {
            var feature = CreateFeature("POLYGON (( 10 10, 10 20, 20 20, 20 15, 10 10))", new Dictionary<string, object>());
            featureCollection.Add(feature);
            ShapeFileWriter.Create(@"", fileName, featureCollection);
        }

        [Test]
        [Ignore(TestCategory.WorkInProgress)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateFile_FeatureCollectionContainsDifferentGeometryTypes_Throws()
        {
            var emptyLineString = CreateFeature("LINESTRING EMPTY", new Dictionary<string, object>());
            var emptyPoint = CreateFeature("POINT EMPTY", new Dictionary<string, object>());

            featureCollection.Add(emptyLineString);
            featureCollection.Add(emptyPoint);

            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);
        }

        [Test]
        [Ignore("Could not fix this test yet, I dont know if this is allowed")]
        public void CreateFile_FeatureCollectionContainsEmptyPoint_ShapeFileIsCreated()
        {
            var emptyPoint = CreateFeature("POINT EMPTY", new Dictionary<string, object>());

            featureCollection.Add(emptyPoint);

            // call
            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);

            // checks
            AssertThatMandatoryFilesExist();

            var reader = new ShapeFileReader(this.shpFilePath);
            var retrievedFeatureCollection = reader.Read();
            Assert.IsNotNull(retrievedFeatureCollection);
            Assert.IsTrue(retrievedFeatureCollection.Count() == 1, "There are no items in the feature collection");
            Assert.AreEqual("Point", featureCollection.ElementAt(0).First.GeometryType);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]        
        public void CreateFile_FeatureContainsInvalidAttributeName_Throws()
        {
            const string atributeName = "";
            featureCollection.Add(CreateFeature("LINESTRING (10 10, 10 20)",
                                                new Dictionary<string, object> {{atributeName, "ID001"}}));

            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateFile_FeatureContainsLongAttributeName_Throws()
        {
            const string atributeName = "AA1234567890";
            featureCollection.Add(CreateFeature("LINESTRING (10 10, 10 20)",
                                                new Dictionary<string, object> { { atributeName, "ID001" } }));

            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);
        }

        [Test]
        public void CreateFile_ValidPathAndFeatureCollectionContainsOneLineString_ShapeFileIsCreated()
        {
            // setup
            const string attributeName = "ID123456789";
            const string attributeValue = "ID001";
            var attributes = new Dictionary<string, object> {{attributeName, attributeValue}};

            featureCollection.Add(CreateFeature("LINESTRING (10 10, 10 20)", attributes));

            // call
            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);

            // checks
            AssertThatMandatoryFilesExist();

            var reader = new ShapeFileReader(this.shpFilePath);
            var retrievedFeatureCollection = reader.Read();
            Assert.IsNotNull(retrievedFeatureCollection);
            Assert.IsTrue(retrievedFeatureCollection.Count() == 1, "There are no items in the feature collection");
            Assert.AreEqual(attributeValue, retrievedFeatureCollection.First().Second[attributeName]);
            Assert.AreEqual("LineString", featureCollection.ElementAt(0).First.GeometryType);
        }


        [Test]
        public void CreateFile_ValidPathAndFeatureCollectionContainsOneLineStringAndOneMultiLineString_ShapeFileIsCreated()
        {
            var featureAttributes = new Dictionary<string, object>
                                    {
                                        {"ID", "ID001"}, 
                                        {"ATTR01", 1}, 
                                        {"ATTR02", 2.01},
                                        {"ATTR03", false}, 
                                        {"ATTR04", new DateTime(2012, 1, 1, 0, 0, 0)}
                                    };

            featureCollection.Add(CreateFeature("LINESTRING (10 10, 10 20)", featureAttributes));
            featureCollection.Add(CreateFeature("MULTILINESTRING ((10 10, 20 20, 10 40),(40 40, 30 30, 40 20, 30 10))", featureAttributes));
            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);

            AssertThatMandatoryFilesExist();

            var reader = new ShapeFileReader(this.shpFilePath);
            var retrievedCollection = reader.Read();
            Assert.IsNotNull(retrievedCollection);
            Assert.IsTrue(retrievedCollection.Count() == 2);
            DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>> elementAt = retrievedCollection.ElementAt(0);
            foreach (var kvp in elementAt.Second)
            {
                Assert.IsTrue(featureAttributes.ContainsKey(kvp.Key));
                Assert.AreEqual(featureAttributes[kvp.Key], elementAt.Second[kvp.Key]);
            }
            Assert.IsTrue(featureCollection.Any(f => f.First.GeometryType == "LineString"));
            Assert.IsTrue(featureCollection.Any(f => f.First.GeometryType == "MultiLineString"));
        }

        [Test]
        public void CreateFile_ValidPathAndRepositoryContainsOnePointAndOneAttribute_ShapeFileIsCreated()
        {
            //setup
            const string attributeName = "ID123456789";
            const string attributeValue = "ID001";

            var attributes = new Dictionary<string, object> {{attributeName, attributeValue}};

            featureCollection.Add(CreateFeature("POINT(0 0)", attributes));
            
            //call
            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);

            //checks
            AssertThatMandatoryFilesExist();

            var reader = new ShapeFileReader(this.shpFilePath);
            var retrievedFeatureCollection = reader.Read();
            Assert.IsNotNull(retrievedFeatureCollection);
            Assert.IsTrue(retrievedFeatureCollection.Count() == 1, "There are no items in the feature collection"); 
            Assert.AreEqual("Point", featureCollection.ElementAt(0).First.GeometryType);
        }

        [Test]
        [Ignore("Cant write multipoint yet")]
        public void CreateFile_ValidPathAndRepositoryContainsMultyPoint_ShapeFileIsCreated()
        {
            //setup
            const string attributeName = "ID123456789";
            const string attributeValue = "ID001";

            var attributes = new Dictionary<string, object> { { attributeName, attributeValue } };

            featureCollection.Add(CreateFeature("POINT(0 0)", attributes));
            featureCollection.Add(CreateFeature("MULTIPOINT (10 40, 40 30, 20 20, 30 10)", attributes));
            
            //call
            ShapeFileWriter.Create(OutputFolder, fileName, featureCollection);

            //checks
            AssertThatMandatoryFilesExist();

            var reader = new ShapeFileReader(this.shpFilePath);
            var retrievedFeatureCollection = reader.Read();
            Assert.IsNotNull(retrievedFeatureCollection);
            Assert.IsTrue(retrievedFeatureCollection.Count() == 1, "There are no items in the feature collection");
            Assert.IsTrue(featureCollection.Any(f => f.First.GeometryType == "Point"));
            Assert.IsTrue(featureCollection.Any(f => f.First.GeometryType == "MultiPoint"));
        }

        #region Helper Methods

        private void AssertThatMandatoryFilesExist()
        {
            Assert.IsTrue(File.Exists(this.shpFilePath), string.Format("{0} does not exist", this.shpFile));
            Assert.IsTrue(File.Exists(this.dbfFilePath), string.Format("{0} does not exist", this.dbfFile));
            Assert.IsTrue(File.Exists(this.shxFilePath), string.Format("{0} does not exist", this.shxFile));
        }

        private DelftTools.Utils.Tuple<IGeometry,IDictionary<string,object>> CreateFeature(string wktGeometryString, IDictionary<string, object> attributes)
        {
            var wktReader = new WKTReader();
            var geom = wktReader.Read(wktGeometryString);
            return new DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>(geom, attributes);
        }

        #endregion
    }
}
