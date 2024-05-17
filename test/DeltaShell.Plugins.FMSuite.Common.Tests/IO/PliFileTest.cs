using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class PliFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGroupableFeaturePliFileAssignsGroupName()
        {
            var groupName = "CrsGroup1_crs.pli";
            string filePath = TestHelper.GetTestFilePath(Path.Combine(@"HydroAreaCollection", groupName));
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var pliFile = new PliFile<ObservationCrossSection2D>();
                IList<ObservationCrossSection2D> readObjects = pliFile.Read(filePath);
                List<IGrouping<string, ObservationCrossSection2D>> groups = readObjects.GroupBy(g => g.GroupName).ToList();
                Assert.That(groups.Count, Is.EqualTo(1));
                Assert.That(groups.First().Key, Is.EqualTo(filePath.Replace(@"\", "/")));
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadFeatureWithColumnsAndLocationNames()
        {
            var feature = new Feature2D
            {
                Name = "line",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(-12345, 54321),
                        new Coordinate(-23451, 43215),
                        new Coordinate(-34512, 32154),
                        new Coordinate(-45123, 21453)
                    }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            feature.Attributes.Add("Column3", new List<double>(new[]
            {
                0.5,
                1.4,
                2.3,
                3.1
            }));
            feature.Attributes.Add("Locations", new List<string>(new[]
            {
                "loc_a",
                "loc_b",
                "loc_c",
                "loc_d"
            }));

            var file = new PliFile<Feature2D>();
            file.Write("feature.pli", new[]
            {
                feature
            });

            IList<Feature2D> features = file.Read("feature.pli");

            Assert.AreEqual(1, features.Count);

            Feature2D featureCopy = features[0];

            Assert.AreEqual(feature.Name, featureCopy.Name);
            Assert.AreEqual(feature.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.AreEqual(feature.Attributes["Column3"], featureCopy.Attributes["Column3"]);
            Assert.AreEqual(feature.Attributes["Locations"], featureCopy.Attributes["Locations"]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadFeaturesWithColumnsAndLocationNames()
        {
            var feature1 = new Feature2D
            {
                Name = "line_1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(-12345, 54321),
                        new Coordinate(-23451, 43215),
                        new Coordinate(-34512, 32154),
                        new Coordinate(-45123, 21453)
                    }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            feature1.Attributes.Add("Column3", new List<double>(new[]
            {
                0.5,
                1.4,
                2.3,
                3.1
            }));
            feature1.Attributes.Add("Locations", new List<string>(new[]
            {
                "loc_a",
                "loc_b",
                "loc_c",
                "loc_d"
            }));

            var feature2 = new Feature2D
            {
                Name = "line_2",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(-54321, 12345),
                        new Coordinate(-43215, 23451),
                        new Coordinate(-32154, 34512)
                    }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            feature2.Attributes.Add("Column3", new List<double>(new[]
            {
                -0.5,
                -1.4,
                -2.3
            }));
            feature2.Attributes.Add("Column4", new List<double>(new[]
            {
                -0.6,
                -1.5,
                -2.4
            }));
            feature2.Attributes.Add("Column5", new List<double>(new[]
            {
                -0.7,
                -1.6,
                -2.5
            }));
            feature2.Attributes.Add("Locations", new List<string>(new[]
            {
                "loc_a*",
                "loc_b*",
                "loc_c*"
            }));

            var file = new PliFile<Feature2D>();
            file.Write("features.pli", new[]
            {
                feature1,
                feature2
            });

            IList<Feature2D> features = file.Read("features.pli");

            Assert.AreEqual(2, features.Count());

            Feature2D featureCopy = features[0];

            Assert.AreEqual(feature1.Name, featureCopy.Name);
            Assert.AreEqual(feature1.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.AreEqual(feature1.Attributes["Column3"], featureCopy.Attributes["Column3"]);
            Assert.AreEqual(feature1.Attributes["Locations"], featureCopy.Attributes["Locations"]);
            Assert.AreEqual(feature1.Name, featureCopy.Name);

            featureCopy = features[1];

            Assert.AreEqual(feature2.Name, featureCopy.Name);
            Assert.AreEqual(feature2.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.AreEqual(feature2.Attributes["Column3"], featureCopy.Attributes["Column3"]);
            Assert.AreEqual(feature2.Attributes["Column4"], featureCopy.Attributes["Column4"]);
            Assert.AreEqual(feature2.Attributes["Column5"], featureCopy.Attributes["Column5"]);
            Assert.AreEqual(feature2.Attributes["Locations"], featureCopy.Attributes["Locations"]);
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void WriteManyFeaturesWithColumnsAndLocationNames()
        {
            const int featureCount = 10000;
            const int pointsPerFeature = 100;
            const string fileName = "many_features.pli";

            var featureCollection = new List<Feature2D>();
            for (var i = 0; i < featureCount; ++i)
            {
                var feature = new Feature2D {Name = "feature_" + i};
                double elevation = Math.Tan((2 * i * Math.PI) / pointsPerFeature);
                List<Coordinate> points = Enumerable.Range(0, pointsPerFeature).Select(t => new Coordinate(t, elevation * t)).ToList();
                feature.Geometry = new LineString(points.ToArray());
                feature.Attributes = new DictionaryFeatureAttributeCollection
                {
                    {"Column3", points.Select(p => p.Y / (1 + (p.X * p.X))).ToList()},
                    {"Column4", points.Select(p => p.X / (1 + (p.Y * p.Y))).ToList()},
                    {"Locations", Enumerable.Range(0, pointsPerFeature).Select(p => "pt_" + p).ToList()}
                };
                featureCollection.Add(feature);
            }

            var file = new PliFile<Feature2D>();
            try
            {
                TestHelper.AssertIsFasterThan(16000, () => file.Write(fileName, featureCollection));
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ReadManyFeaturesWithColumnsAndLocationNames()
        {
            const int featureCount = 10000;
            const int pointsPerFeature = 100;
            const string fileName = "many_features.pli";

            var featureCollection = new List<Feature2D>();
            for (var i = 0; i < featureCount; ++i)
            {
                var feature = new Feature2D {Name = "feature_" + i};
                double elevation = Math.Tan((2 * i * Math.PI) / pointsPerFeature);
                List<Coordinate> points = Enumerable.Range(0, pointsPerFeature).Select(t => new Coordinate(t, elevation * t)).ToList();
                feature.Geometry = new LineString(points.ToArray());
                feature.Attributes = new DictionaryFeatureAttributeCollection
                {
                    {"Column3", points.Select(p => p.Y / (1 + (p.X * p.X))).ToList()},
                    {"Column4", points.Select(p => p.X / (1 + (p.Y * p.Y))).ToList()},
                    {"Locations", Enumerable.Range(0, pointsPerFeature).Select(p => "pt_" + p).ToList()}
                };
                featureCollection.Add(feature);
            }

            var file = new PliFile<Feature2D>();
            try
            {
                file.Write(fileName, featureCollection);
                TestHelper.AssertIsFasterThan(20000, () => file.Read(fileName));
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [Test]
        public void GivenSimplePliFile_WhenReadingFile_ThenAllDataIsReadAndStoredAsFeatureAttributes()
        {
            var testFileName = "OneSimpleFixedWeir_fxw.pli";
            string testDir = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("HydroAreaCollection"));
            string filePath = Path.Combine(testDir, testFileName);

            var fileReader = new PliFile<Feature2D>();
            IList<Feature2D> fixedWeirs = fileReader.Read(filePath);
            Assert.That(fixedWeirs.Count, Is.EqualTo(1));

            Feature2D fixedWeir = fixedWeirs.FirstOrDefault();
            IFeatureAttributeCollection attributes = fixedWeir.Attributes;
            Assert.That(attributes.Count, Is.EqualTo(8));

            attributes.CheckDoubleValuesForColumn("Column3", 10.96, 10.89);
            attributes.CheckDoubleValuesForColumn("Column4", 3.5, 3.0);
            attributes.CheckDoubleValuesForColumn("Column5", 3.2, 3.3);
            attributes.CheckDoubleValuesForColumn("Column6", 4.0, 3.8);
            attributes.CheckDoubleValuesForColumn("Column7", 4.0, 4.0);
            attributes.CheckDoubleValuesForColumn("Column8", 4.0, 4.0);
            attributes.CheckDoubleValuesForColumn("Column9", 0.0, 0.0);
            attributes.CheckStringValuesForColumn("WeirType", "V", "V");
        }

        [Test]
        public void GivenSimpleFixedWeir_WhenWritingToPlizFile_ThenAllAttributeValuesAreWritten()
        {
            string filePath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("HydroAreaCollection/OneSimpleFixedWeir_fxw.pli"));
            string writeToFilePath = Path.Combine(Path.GetDirectoryName(filePath), "WrittenFixedWeirs_fxw.pli");
            try
            {
                var fileReaderWriter = new PliFile<Feature2D>();
                IList<Feature2D> fixedWeirs = fileReaderWriter.Read(filePath);
                fileReaderWriter.Write(writeToFilePath, fixedWeirs);

                string[] originalContent = File.ReadAllLines(filePath);
                string[] resultingContent = File.ReadAllLines(writeToFilePath);

                // Check if values in the resulting file are equal to the values in the original file.
                Assert.That(originalContent.Length, Is.EqualTo(resultingContent.Length));
                for (var i = 0; i < originalContent.Length; i++)
                {
                    var originalSeparator = '\t';
                    if (i == 1)
                    {
                        originalSeparator = ' ';
                    }

                    var resultingSeparator = ' ';

                    string[] originalLineContent = originalContent[i].Split(originalSeparator).Where(s => s != string.Empty).ToArray();
                    string[] resultingLineContent = resultingContent[i].Split(resultingSeparator).Where(s => s != string.Empty).ToArray();
                    Assert.That(originalLineContent.Length == resultingLineContent.Length);
                    for (var n = 0; n < resultingLineContent.Length; n++)
                    {
                        var equalValues = false;
                        if (i < 2)
                        {
                            equalValues = originalLineContent[n].Trim() == resultingLineContent[n].Trim();
                        }
                        else if (n < resultingLineContent.Length - 1)
                        {
                            double originalValue = double.Parse(originalLineContent[n], CultureInfo.InvariantCulture);
                            double resultingValue = double.Parse(resultingLineContent[n], CultureInfo.InvariantCulture);
                            equalValues = originalValue == resultingValue;
                        }
                        else if (n == resultingLineContent.Length - 1)
                        {
                            equalValues = originalLineContent[n].Trim() == resultingLineContent[n].Trim();
                        }

                        Assert.That(equalValues, string.Format("Values in line " + (i + 1) + " in written file differs from the original value.\nOriginal: {0}\nResult: {1}", originalContent[i], resultingContent[i]));
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }
        }

        [Test]
        public void GivenPliFileWithStringValueInTheWrongColumn_WhenReading_ThenExceptionIsThrown()
        {
            Assert.That(() => ReadPliFile("HydroAreaCollection/IncorrectFormatForFixedWeir_fxw.pli"),
                Throws.InstanceOf<FormatException>().With.Message.StartsWith("Invalid placement of string value 'V' on line 3 in file"));
        }

        [Test]
        public void GivenFeature2DWithLowerColumnCountInPliFile_WhenReading_ThenExceptionIsThrown()
        {
            Assert.That(() => ReadPliFile("HydroAreaCollection/WrongColumnCountDefinedForWeir_fxw.pli"),
                Throws.InstanceOf<FormatException>().With.Message.StartsWith("Invalid point row (expected 9 columns, but was 11) on line 3 in file"));
        }

        [Test]
        public void GivenValidPliFileWithLocationNamesOnPoints_WhenReading_ThenLocationNamesAreStoredInFeatureAttributes()
        {
            IList<Feature2D> features = ReadPliFile("HydroAreaCollection/FeatureWithLocationNamesOnPoints_fxw.pli");
            Assert.That(features.Count, Is.EqualTo(1));
            Feature2D feature = features.FirstOrDefault();
            Assert.NotNull(feature);

            IFeatureAttributeCollection attributes = feature.Attributes;
            Assert.That(attributes.Keys.Contains(Feature2D.LocationKey));
            var locationNames = (GeometryPointsSyncedList<string>) attributes[Feature2D.LocationKey];
            Assert.That(locationNames[0], Is.EqualTo("point1"));
            Assert.That(locationNames[1], Is.EqualTo("point2"));
        }

        [Test]
        public void GivenPliFileThatContainsOnePoint_WhenReadingPliFile_ThenPointIsReturned()
        {
            IList<Feature2D> sources = ReadPliFile("structures/SourceSink01.pli");
            Assert.That(sources.Count, Is.EqualTo(1));
        }

        #region Test helper methods

        private static IList<Feature2D> ReadPliFile(string relativeFilePath)
        {
            string filePath =
                TestHelper.CreateLocalCopy(
                    TestHelper.GetTestFilePath(relativeFilePath));
            IList<Feature2D> features = new List<Feature2D>();
            try
            {
                var fileReader = new PliFile<Feature2D>();
                features = fileReader.Read(filePath); // Should throw exception
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }

            return features;
        }

        #endregion
    }
}
